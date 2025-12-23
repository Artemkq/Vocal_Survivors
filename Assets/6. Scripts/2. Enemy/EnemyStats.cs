using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : EntityStats
{
    [System.Serializable]
    public class Resistances
    {
        [Range(-1f, 1f)] public float freeze = 0f, kill = 0f, debuff = 0f;

        //To allow us to multiply the resistances
        public static Resistances operator *(Resistances r, float factor)
        {
            r.freeze = Mathf.Min(1, r.freeze * factor);
            r.kill = Mathf.Min(1, r.kill * factor);
            r.debuff = Mathf.Min(1, r.debuff * factor);
            return r;
        }

        public static Resistances operator +(Resistances r, Resistances r2)
        {
            r.freeze += r2.freeze;
            r.kill = r2.kill;
            r.debuff = r2.debuff;
            return r;
        }

        //Allows us to multiply resistances by one another, for multiplicative buffs
        public static Resistances operator *(Resistances r1, Resistances r2)
        {
            r1.freeze = Mathf.Min(1, r1.freeze * r2.freeze);
            r1.kill = Mathf.Min(1, r1.kill * r2.kill);
            r1.debuff = Mathf.Min(1, r1.debuff * r2.debuff);
            return r1;
        }
    }

    [System.Serializable]
    public struct Stats
    {
        public float maxHealth, damage, moveSpeed, knockbackMultiplier;
        public Resistances resistances;

        [System.Flags]
        public enum Boostable { health = 1, moveSpeed = 2, damage = 4, knockbackMultiplier = 8, resistances = 16 }
        public Boostable curseBoosts, levelBoosts;

        private static Stats Boost(Stats s1, float factor, Boostable boostable)
        {
            if ((boostable & Boostable.health) != 0) s1.maxHealth *= factor;
            if ((boostable & Boostable.moveSpeed) != 0) s1.moveSpeed *= factor;
            if ((boostable & Boostable.damage) != 0) s1.damage *= factor;
            if ((boostable & Boostable.knockbackMultiplier) != 0) s1.knockbackMultiplier /= factor;
            if ((boostable & Boostable.resistances) != 0) s1.resistances *= factor;
            return s1;
        }

        //Use the multiply operator for curse
        public static Stats operator *(Stats s1, float factor) { return Boost(s1, factor, s1.curseBoosts); }

        //Use the XOR operator for level boosted stats
        public static Stats operator ^(Stats s1, float factor) { return Boost(s1, factor, s1.levelBoosts); }

        //Use the add operator to add stats to the enemy
        public static Stats operator +(Stats s1, Stats s2)
        {
            s1.maxHealth += s2.maxHealth;
            s1.moveSpeed += s2.moveSpeed;
            s1.damage += s2.damage;
            s1.knockbackMultiplier += s2.knockbackMultiplier;
            s1.resistances += s2.resistances;
            return s1;
        }

        //Use the multiply operator to scale stats
        //Used by the buff / debuff system
        public static Stats operator *(Stats s1, Stats s2)
        {
            s1.maxHealth *= s2.maxHealth;
            s1.moveSpeed *= s2.moveSpeed;
            s1.damage *= s2.maxHealth;
            s1.knockbackMultiplier *= s2.knockbackMultiplier;
            s1.resistances *= s2.resistances;
            return s1;
        }
    }

    public Stats baseStats = new Stats { maxHealth = 10, moveSpeed = 1, damage = 3, knockbackMultiplier = 1 };
    Stats actualStats;
    public Stats Actual
    {
        get { return actualStats; }
    }

    public BuffInfo[] attackEffects;

    [Header("Damage Feedback")]
    public Color damageColor = new Color(1, 0, 0, 1); //What the color of the damage flash should be.
    public float damageFlashDuration = 0.2f; //How long the flash should last.
    public float deathFadeTime = 0.6f; //How much time it takes for the enemy to fade.

    // --- ДОБАВЛЕНО ---
    Collider2D enemyCollider;
    // Ссылка на компонент движения
    EnemyMovement movement;

    public static int count;

    protected override void Awake()
    {
        base.Awake();
        count++;
        movement = GetComponent<EnemyMovement>();
        enemyCollider = GetComponent<Collider2D>();

        if (movement == null)
        {
            Debug.LogWarning("EnemyMovement component not found on " + gameObject.name, this);
        }

        // ДОБАВИТЬ ЭТИ СТРОКИ:
        if (enemyCollider == null)
        {
            Debug.LogWarning("Collider2D component not found on " + gameObject.name, this);
        }
    }

    protected override void Start()
    {
        base.Start();

        //Adds the global buff there is any
        if (UILevelSelector.globalBuff && !UILevelSelector.globalBuffAffectsEnemies)
            ApplyBuff(UILevelSelector.globalBuff);

        RecalculateStats();
        //Calculate the health and check for level boosts
        health = actualStats.maxHealth;
        // movement = GetComponent<EnemyMovement>();
    }

    public override bool ApplyBuff(BuffData data, int variant = 0, float durationMultiplier = 1f)
    {
        //If the debuff is a freeze, we check for freeze resistance
        //Roll a number and if it succeeds, we ignore the freeze
        if ((data.type & BuffData.Type.freeze) > 0)
            if (Random.value <= Actual.resistances.freeze) return false;

        //If the debuff is a debuff, we check for debuff resistance
        if ((data.type & BuffData.Type.debuff) > 0)
            if (Random.value <= Actual.resistances.debuff) return false;

        return base.ApplyBuff(data, variant, durationMultiplier);
    }

    //Calculates the actual stats of the enemy based on a variety of factors
    public override void RecalculateStats()
    {
        //Calculate curse boosts
        float curse = GameManager.GetCumulativeCurse(),
            level = GameManager.GetCumulativeLevels();
        actualStats = (baseStats * curse) ^ level;

        //Create a variable to store all the cumulative multiplier values
        Stats multiplier = new Stats
        {
            maxHealth = 1f,
            moveSpeed = 1f,
            damage = 1f,
            knockbackMultiplier = 1f,
            resistances = new Resistances { freeze = 1f, debuff = 1f, kill = 1f }
        };

        foreach (Buff b in activeBuffs)
        {
            BuffData.Stats bd = b.GetData();
            switch (bd.modifierType)
            {
                case BuffData.ModifierType.additive:
                    actualStats += bd.enemyModifier;
                    break;
                case BuffData.ModifierType.multiplicative:
                    multiplier *= bd.enemyModifier;
                    break;
            }
        }

        //Apply the multipliers last
        actualStats *= multiplier;
    }

    public override void TakeDamage(float dmg)
    {
        if (enemyCollider != null && !enemyCollider.enabled) return;

        // --- БЛОК BEAT HELL: РАСЧЕТ МОДИФИКАТОРА ---
        float multiplier = 1f;

        // 1. Бонус за попадание в бит (x2 урон)
        if (BeatConductor.Instance != null && BeatConductor.Instance.IsInBeatWindow)
        {
            multiplier += 1.0f;
        }

        // 2. Бонус за вокал (до +100% от громкости)
        if (VocalAnalyzer.Instance != null)
        {
            multiplier += VocalAnalyzer.Instance.CurrentLoudness;
        }

        float totalDmg = dmg * multiplier;
        // --- КОНЕЦ БЛОКА BEAT HELL ---

        health -= totalDmg;

        // Проверка на Insta-kill (используем исходный dmg, как в твоей логике)
        if (dmg == actualStats.maxHealth)
        {
            if (Random.value < actualStats.resistances.kill)
            {
                return;
            }
        }

        if (totalDmg > 0)
        {
            StartCoroutine(DamageFlash());
            // Выводим уже усиленный урон в текст
            GameManager.GenerateFloatingText(Mathf.FloorToInt(totalDmg).ToString(), transform);
        }

        if (health <= 0)
        {
            Kill();
        }
    }

    public void TakeDamage(float dmg, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        // Вызываем базовый TakeDamage, который уже усилит dmg ритмом и голосом
        TakeDamage(dmg);

        if (movement == null || health <= 0)
        {
            return;
        }

        if (knockbackForce > 0)
        {
            // БОНУС: Усиление отбрасывания в ритм
            float kMultiplier = 1f;
            if (BeatConductor.Instance != null && BeatConductor.Instance.IsInBeatWindow)
                kMultiplier = 1.5f; // В бит отбрасываем на 50% дальше

            Vector2 dir = (Vector2)transform.position - sourcePosition;
            movement.Knockback(dir.normalized * knockbackForce * kMultiplier, knockbackDuration);
        }
    }

    public override void RestoreHealth(float amount)
    {
        //Only heal the player if their current health is less than their max health
        if (health < actualStats.maxHealth)
        {
            health += amount;
            if (health > actualStats.maxHealth)
            {
                health = actualStats.maxHealth;
            }
        }
    }

    //This is a Coroutine function that makes the enemy flash when taking damage
    IEnumerator DamageFlash()
    {
        ApplyTint(damageColor);
        yield return new WaitForSeconds(damageFlashDuration);
        RemoveTint(damageColor);
    }

    public override void Kill()
    {
        // --- ДОБАВЛЯЕМ СЮДА ---
        // Ищем менеджер комбо на сцене и вызываем метод регистрации убийства
        KillComboManager comboManager = FindAnyObjectByType<KillComboManager>();
        if (comboManager != null)
        {
            comboManager.OnEnemyKilled();
        }
        // ----------------------

        DropRateManager drops = GetComponent<DropRateManager>();

        // *** ИСПРАВЛЕНИЕ ЛОГИКИ: ВОССТАНАВЛИВАЕМ ПРОВЕРКУ ФЛАГА ***
        if (movement == null) movement = GetComponent<EnemyMovement>();

        if (drops != null && movement != null)
        {
            drops.active = movement.giveExperienceOnDeath;
        }
        else if (drops != null && movement == null)
        {
            drops.active = true;
        }

        if (enemyCollider != null) enemyCollider.enabled = false;
        if (movement != null) movement.enabled = false;

        StartCoroutine(KillFade());
    }

    //This is a Coroutine function that fades the enemy away slowly
    IEnumerator KillFade()
    {
        //Waits for a single frame
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0, origAlpha = sprite.color.a;

        //This is a loop that fires evety frame
        while (t < deathFadeTime)
        {
            yield return w;
            t += Time.deltaTime;

            //Set the color for this frame
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, (1 - t / deathFadeTime) * origAlpha);
        }
        Destroy(gameObject);
    }

    // Замените OnCollisionStay2D на OnTriggerStay2D
    void OnTriggerStay2D(Collider2D col)
    {
        // *** ИСПРАВЛЕНИЕ: Используем col (Collider2D), а не col.collider ***

        // Проверка, чтобы мертвый (затухающий) враг не мог наносить урон игроку
        if (enemyCollider == null || !enemyCollider.enabled) return;

        if (Mathf.Approximately(Actual.damage, 0)) return;

        //Check for whether there is a PlayerStats object we can damage
        // Пытаемся получить компонент PlayerStats с того коллайдера, с которым столкнулись (BoxCollider2D)
        if (col.TryGetComponent(out PlayerStats p))
        {
            p.TakeDamage(Actual.damage);
            foreach (BuffInfo b in attackEffects)
                p.ApplyBuff(b);
        }
    }

    void OnDestroy()
    {
        count--;
    }
}
