using UnityEngine;

/// <summary>
/// Component to be attached to all Weapon prefabs. The Weapon prefab works together with the WeaponData ScrtiptableObjects to manage and run the behaviours of all weapons in the game
/// </summary>


public abstract class Weapon : Item
{
    [System.Serializable]
    public class Stats : LevelData
    {
        [Header("Visuals")]
        public Projectile projectilePrefab; //If attached, a projectile will spawn every time the weapon cools down
        public Aura auraPrefab; //If attached, an aura will spawn when weapon is equipped
        public ParticleSystem hitEffect, procEffect;
        public Rect spawnVariance;

        [Header("Values")]
        public float damage;
        public float area;
        public float speed; //Нужно устанавливать 10 по умолчанию
        public int amount;
        public float duration; //if duration = 0, it will last forever
        public int pierce;
        public int cooldown; // Теперь это будет восприниматься как "количество битов"
        public float projectileInterval;
        public float knockback;
        public int poolLimit;

        [Header("Other Values")]
        public float damageVariance;

        [Header("Dont USE FOR NOW Values")] //Нужно придумать применение
        public float chance; 
        public float criticalMultiplier; 
        public bool blockedByWalls;

        public EntityStats.BuffInfo[] appliedBuffs;

        //Allows us to use the + operator to add 2 Stats together
        //Very important later when we want to increase our weapon stats
        public static Stats operator +(Stats s1, Stats s2)
        {
            Stats result = new Stats();
            result.name = s2.name ?? s1.name;
            result.description = s2.description ?? s1.description;

            result.projectilePrefab = s2.projectilePrefab ?? s1.projectilePrefab;
            //result.auraPrefab = s2.auraPrefab ?? s1.auraPrefab;

            result.hitEffect = s2.hitEffect == null ? s1.hitEffect : s2.hitEffect;
            result.procEffect = s2.procEffect == null ? s1.procEffect : s2.procEffect;
            
            // Проверяем, имеет ли s2.spawnVariance ненулевые параметры.
            // Если да, используем его. Если нет (оно пустое), используем s1.
            result.spawnVariance = (s2.spawnVariance.width != 0 || s2.spawnVariance.height != 0 || s2.spawnVariance.x != 0 || s2.spawnVariance.y != 0) ? s2.spawnVariance : s1.spawnVariance;

            result.duration = s1.duration + s2.duration;
            result.damage = s1.damage + s2.damage;
            result.damageVariance = s1.damageVariance + s2.damageVariance;
            result.area = s1.area + s2.area;
            result.speed = s1.speed + s2.speed;
            result.cooldown = s1.cooldown + s2.cooldown;
            result.amount = s1.amount + s2.amount;
            result.pierce = s1.pierce + s2.pierce;
            result.projectileInterval = s1.projectileInterval + s2.projectileInterval;
            result.knockback = s1.knockback + s2.knockback;
            result.appliedBuffs = s2.appliedBuffs == null || s2.appliedBuffs.Length <= 0 ? s1.appliedBuffs : s2.appliedBuffs;

            return result;
        }

        //Get damage dealt
        public float GetDamage()
        {
            return damage + Random.Range(0, damageVariance);
        }
    }

    protected Stats currentStats;

    protected float currentCooldown;

    protected PlayerMovement movement; //Reference to the players movement

    // --- ИЗМЕНЕНО: Добавляем подписку на Бит при появлении оружия ---
    protected virtual void OnEnable()
    {
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat += HandleBeatAttack;
        }
    }

    protected virtual void OnDisable()
    {
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat -= HandleBeatAttack;
        }
    }

    // --- ИЗМЕНЕНО: Этот метод вызывается каждый раз, когда звучит бит ---
    private void HandleBeatAttack()
    {
        currentCooldown -= 1f; // Уменьшаем кулдаун на 1 бит

        if (currentCooldown <= 0f)
        {
            Attack(currentStats.amount + owner.Stats.amount);
        }
    }

    //For dynamically created weapons, call initialise to set everything up
    public virtual void Initialise(WeaponData data)
    {
        base.Initialise(data);
        this.data = data;
        currentStats = data.baseStats;
        movement = GetComponentInParent<PlayerMovement>();
        ActivateCooldown();
    }

    // --- ИЗМЕНЕНО: Удаляем логику из Update, так как теперь мы зависим от OnBeat ---
    protected virtual void Update()
    {
        // Теперь Update пустой, чтобы не стрелять по обычному времени.
    }

    //Levels up the weapon by 1, and calculates the corresponding stats

    public override bool DoLevelUp(bool updateUI = true)
    {
        base.DoLevelUp(updateUI);

        //Prevent level up if we are already at max level
        if (!CanLevelUp())
        {
            Debug.LogWarning(string.Format("Cannot level up {0} to Level {1}, max level of {2} already reached.", name, currentLevel, data.maxLevel));
            return false;
        }

        //Otherwise, add stats of the next level to our weapon
        currentStats += (Stats)data.GetLevelData(++currentLevel);
        return true;
    }

    //Lets us check whether this weapon can attack at this current moment
    public virtual bool CanAttack()
    {
        if (Mathf.Approximately(owner.Stats.might, 0)) return false;
        return currentCooldown <= 0;
    }

    //Performs an attack with the weapon
    //Returns true of the attack was successful
    //This doesnt do anything. We have to override this at the child class to add a behaviout

    protected virtual bool Attack(int attackCount = 1)
    {
        if (CanAttack())
        {
            ActivateCooldown();
            return true;
        }
        return false;
    }

    // --- ИЗМЕНЕНО: Теперь кулдаун устанавливается в БИТАХ ---
    public virtual bool ActivateCooldown(bool strict = false)
    {
        // Если включен строгий режим и откат еще не прошел — ничего не делаем
        if (strict && currentCooldown > 0) return false;

        // 1. Берем базовое количество битов (например, 4)
        // 2. Умножаем на модификатор игрока (например, 0.75 для ускорения)
        // 3. RoundToInt превращает результат (3.0) в целое число
        int actualBeats = Mathf.RoundToInt(currentStats.cooldown * Owner.Stats.cooldown);

        // ВАЖНО: Оружие не может стрелять чаще, чем каждый 1 бит (на каждый удар)
        // Mathf.Max гарантирует, что значение не упадет до 0 или отрицательного
        currentCooldown = Mathf.Max(1, actualBeats);

        return true;
    }

    public virtual float GetDamage() { return currentStats.GetDamage() * owner.Stats.might; }
    public virtual float GetArea() { return currentStats.area * owner.Stats.area; }
    public virtual Stats GetStats() { return currentStats; }

    public void ApplyBuffs(EntityStats e)
    {
        if (e == null || GetStats().appliedBuffs == null) return;
        foreach (EntityStats.BuffInfo b in GetStats().appliedBuffs)
        {
            if (owner != null) e.ApplyBuff(b, owner.Actual.duration);
        }
    }
}
