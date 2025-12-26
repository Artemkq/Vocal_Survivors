using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static EnemyStats;

public class PlayerStats : EntityStats
{
    CharacterData characterData;
    public CharacterData.Stats baseStats;
    [SerializeField] CharacterData.Stats actualStats;

    public CharacterData.Stats Stats
    {
        get { return actualStats; }
        set { actualStats = value; }
    }

    public CharacterData.Stats Actual
    {
        get { return actualStats; }
    }

    #region Current Stats Properties

    public float CurrentHealth
    {        
        get { return health; }
        
        //If we try and set the current health, the UI interface
        //on the pause screen will also updated
        
        set
        {
            //Check if the value has changed

            if (health != value)
            {
                health = value;
                UpdateHealthBar();
            }
        }
    }

    #endregion

    [Header("Visuals")]
    public ParticleSystem damageEffect; //If damage is dealt
    public ParticleSystem blockedEffect; //If armor completely blocks damage

    //Experience and level of the player
    [Header("Experience/Level")]
    public int experience = 0;
    public int level = 1;
    public int experienceCap;

    //Class for defining a level range and the corresponding experience cap increase for that range
    [System.Serializable] public class LevelRange
    {
        public int startLevel;
        public int endLevel;
        public int experienceCapIncrease;
    }

    //I-Frames
    [Header("I-Frames")]
    public float invincibilityDuration;
    float invincibilityTimer;
    bool isInvincible;

    public List<LevelRange> levelRanges;

    PlayerInventory inventory;
    PlayerCollector collector;

    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TMP_Text levelText;

    PlayerAnimator playerAnimator;

    protected override void Awake()
    {
        base.Awake(); // < --������������ ����� ������� �������������

        characterData = UICharacterSelector.GetData();

        inventory = GetComponent<PlayerInventory>();
        collector = GetComponentInChildren<PlayerCollector>();

        //Assign the variables
        baseStats = actualStats = characterData.stats;
        collector.SetRadius(actualStats.magnet);
        health = actualStats.maxHealth;

        playerAnimator = GetComponent<PlayerAnimator>();
        if (characterData.controller)
            playerAnimator.SetAnimatorController(characterData.controller);
    }

    protected override void Start()
    {
        base.Start();

        //Adds the global buff there is any
        if (UILevelSelector.globalBuff && !UILevelSelector.globalBuffAffectsPlayer)
            ApplyBuff(UILevelSelector.globalBuff);

        //Spawn the starting weapon
        inventory.Add(characterData.StartingWeapon);
        
        //Initialize the experience cap as first experience cap increase
        experienceCap = levelRanges[0].experienceCapIncrease;

        GameManager.instance.AssignChosenCharacterUI(characterData);

        UpdateHealthBar();
        UpdateExpBar();
        UpdateLevelText();
    }

    protected override void Update()
    {
        base.Update();
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
        //If the invincibility timer has reached 0, set the invincibility flag to false
        else if (isInvincible)
        {
            isInvincible = false;
        }

        Recover();

    }

    public override void RecalculateStats()
    {
        actualStats = baseStats;
        foreach (PlayerInventory.Slot s in inventory.passiveSlots)
        {
            Passive p = s.item as Passive;
            if (p)
            {
                actualStats += p.GetBoosts();
            }
        }

        //Create a variable to store all the cumulative multiplier values
        CharacterData.Stats multiplier = new CharacterData.Stats
        {
            maxHealth = 1f,
            recovery = 1f,
            armor = 1f,
            moveSpeed = 1f,
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            amount = 1,
            cooldown = 1,
            luck = 1f,
            growth = 1f,
            greed = 1f,
            curse = 1f,
            magnet = 1f,
            revival = 1
        };

        foreach (Buff b in activeBuffs)
        {
            BuffData.Stats bd = b.GetData();
            switch (bd.modifierType)
            {
                case BuffData.ModifierType.additive:
                    actualStats += bd.playerModifier;
                    break;
                case BuffData.ModifierType.multiplicative:
                    multiplier *= bd.playerModifier;
                    break;
            }
        }
        actualStats *= multiplier;
        
        //Update the PlayerCollectors radius
        collector.SetRadius(actualStats.magnet);
    }

    public void IncreaseExperience (int amount)
    {
        experience += amount;

        LevelUpChecker();
        UpdateExpBar();
    }

    void LevelUpChecker()
    {
        if (experience >= experienceCap)
        {
            level++;
            experience -= experienceCap;

            int experienceCapIncrease = 0;
            foreach (LevelRange range in levelRanges)
            {
                if (level >= range.startLevel && level <= range.endLevel)
                {
                    experienceCapIncrease = range.experienceCapIncrease;
                    break;
                }
            }
            experienceCap += experienceCapIncrease;

            UpdateLevelText();

            GameManager.instance.StartLevelUp();

            if (experience >= experienceCap)
                LevelUpChecker();
        }
    }   
    
    void UpdateExpBar()
    {
        // *** ИСПРАВЛЕНИЕ: Добавляем проверку, существует ли еще UI-элемент ***
        if (expBar == null)
        {
            // Если панель опыта отсутствует, просто выходим из функции.
            // Возможно, UI был уничтожен, потому что игра закончилась.
            Debug.LogWarning("expBar reference is missing or destroyed. Cannot update UI.");
            return;
        }
        
        //Update exp bar fill amount
        expBar.fillAmount = (float)experience / experienceCap;
    }

    void UpdateLevelText()
    {
        // *** ИСПРАВЛЕНИЕ: Добавляем проверку для уровня тоже, на всякий случай ***
        if (levelText == null)
        {
            Debug.LogWarning("levelText reference is missing or destroyed. Cannot update UI.");
            return;
        }
        
        //Update level text
        levelText.text = "Level " + level.ToString();
    }

    public override void TakeDamage (float dmg)
    {
        //If the player is not currently invincible, reduce health and start invincibility
        if (!isInvincible)
        {
            //Take armor into account before dealing the damage
            dmg -= actualStats.armor;
            
            if (dmg > 0)
            {
                //Deal the damage
                CurrentHealth -= dmg;

                //If there is a damage effect assigned, playe it
                if (damageEffect) Destroy(Instantiate(damageEffect, transform.position, Quaternion.identity), 5f);

                if (CurrentHealth <= 0)
                {
                    Kill();
                }
                else
                {
                    //If there is a blocked effect assigned, play it
                    if (blockedEffect) Destroy(Instantiate(blockedEffect, transform.position, Quaternion.identity), 5f);
                }
                invincibilityTimer = invincibilityDuration;
                isInvincible = true;

                UpdateHealthBar();
            }
        }
    }

    void UpdateHealthBar()
    {
        // *** ИСПРАВЛЕНИЕ: И для полоски здоровья тоже ***
        if (healthBar == null)
        {
            Debug.LogWarning("healthBar reference is missing or destroyed. Cannot update UI.");
            return;
        }
        
        //Update the health bar
        healthBar.fillAmount = CurrentHealth / actualStats.maxHealth;
    }

    public override void Kill()
    {
        if (!GameManager.instance.isGameOver)
        {
            // 1. Находим сборщик и переносим алмазы из "сумки" в "сейф" (в памяти)
            PlayerCollector collector = GetComponentInChildren<PlayerCollector>();
            if (collector != null)
            {
                collector.SaveDiamondsToStash();
            }

            // 2. Вызываем финальную запись на диск (через ваш SaveManager)
            SaveManager.Save();

            // 3. Показываем UI и останавливаем игру
            GameManager.instance.AssignLevelReachedUI(level);
            GameManager.instance.GameOver();
        }
    }

    public override void RestoreHealth(float amount)
    {
        //Only heal the player if their current health is less than their max health
        if (CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += amount;

            //Make sure the player's health doesn't exceed their max health
            if (CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
            UpdateHealthBar();
        }
    }

    void Recover()
    {
    if (CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += Stats.recovery * Time.deltaTime;

            //Make sure the player's health doesn't exceed their max health
            if (CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
            UpdateHealthBar();
        }
    }
}
