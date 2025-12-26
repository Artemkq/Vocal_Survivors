using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Добавлено для работы с NavMesh

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : EntityStats
{
    [System.Serializable]
    public class Resistances
    {
        [Range(-1f, 1f)] public float freeze = 0f, kill = 0f, debuff = 0f;
        // ... (операторы +, * остаются без изменений) ...
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
        public static Stats operator *(Stats s1, float factor) { return Boost(s1, factor, s1.curseBoosts); }
        public static Stats operator ^(Stats s1, float factor) { return Boost(s1, factor, s1.levelBoosts); }
        public static Stats operator +(Stats s1, Stats s2)
        {
            s1.maxHealth += s2.maxHealth;
            s1.moveSpeed += s2.moveSpeed;
            s1.damage += s2.damage;
            s1.knockbackMultiplier += s2.knockbackMultiplier;
            s1.resistances += s2.resistances;
            return s1;
        }
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
    public Stats Actual => actualStats;

    public BuffInfo[] attackEffects;

    [Header("Damage Feedback")]
    public Color damageColor = new Color(1, 0, 0, 1);
    public float damageFlashDuration = 0.2f;
    public float deathFadeTime = 0.6f;

    private KillComboManager cachedCombo;

    Collider2D enemyCollider;
    EnemyMovement movement;
    SpriteRenderer sr; // Кэшируем для быстрой смены цвета

    // *** ИЗМЕНЕНО: УДАЛЕН СТАТИЧЕСКИЙ СЧЕТЧИК 'count' ***

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<EnemyMovement>();
        enemyCollider = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // ОПТИМИЗАЦИЯ: Выносим расчет статов в отдельный метод, который вызывается РАЗ при спавне
    public void InitializeStats()
    {
        RecalculateStats();
        health = actualStats.maxHealth;

        // Сброс визуальных эффектов (если враг из пула был красный/прозрачный)
        if (sr != null) sr.color = Color.white;
    }

    protected override void Start()
    {
        // ВАЖНО: В пулинге Start вызывается только 1 раз в жизни объекта.
        // Поэтому всю логику "оживления" переносим в OnEnable.
        base.Start();
    }

    // *** НОВОЕ: МЕТОДЫ ONENABLE И ONDISABLE ДЛЯ РЕГИСТРАЦИИ В ПУЛЕ ***
    protected virtual void OnEnable()
    {
        // Регистрация в WaveManager
        if (WaveManager.instance != null) WaveManager.instance.RegisterEnemy(this);

        // Сброс состояния
        if (enemyCollider != null) enemyCollider.enabled = true;
        if (movement != null) movement.enabled = true;

        InitializeStats();
    }

    protected virtual void OnDisable()
    {
        if (WaveManager.instance != null) WaveManager.instance.DeregisterEnemy(this);

        // ОСТАНОВКА КОРУТИН: Это критично! 
        // Если враг умер, а вспышка урона еще идет — будет ошибка.
        StopAllCoroutines();
    }
    // *******************************************************************

    public override bool ApplyBuff(BuffData data, int variant = 0, float durationMultiplier = 1f)
    {
        if ((data.type & BuffData.Type.freeze) > 0)
            if (Random.value <= Actual.resistances.freeze) return false;
        if ((data.type & BuffData.Type.debuff) > 0)
            if (Random.value <= Actual.resistances.debuff) return false;
        return base.ApplyBuff(data, variant, durationMultiplier);
    }

    // ОПТИМИЗАЦИЯ RecalculateStats:
    // Убедитесь, что этот метод не делает GameObject.Find или тяжелых поисков.
    public override void RecalculateStats()
    {
        // Просто пересчитываем базу на основе проклятия и уровня
        actualStats = baseStats * GameManager.GetCumulativeCurse();
        actualStats = actualStats ^ (float)GameManager.GetCumulativeLevels();

        // Обновляем скорость в скрипте движения
        if (movement != null) movement.UpdateSpeed(actualStats.moveSpeed);
    }

    public override void TakeDamage(float dmg)
    {
        // 1. Быстрая проверка: если враг уже "мертв" (в процессе исчезновения), игнорируем урон
        if (enemyCollider != null && !enemyCollider.enabled) return;

        // --- БЛОК BEAT HELL ---
        float multiplier = 1f;
        // ОПТИМИЗАЦИЯ: проверка BeatConductor через Instance? (null-conditional)
        if (BeatConductor.Instance != null && BeatConductor.Instance.IsInBeatWindow)
            multiplier += 1.0f;

        float totalDmg = dmg * multiplier;
        // --- КОНЕЦ БЛОКА ---

        health -= totalDmg;

        if (totalDmg > 0)
        {
            // ОПТИМИЗАЦИЯ: Вместо StartCoroutine используем простую смену цвета.
            // Корутины при массовом уроне создают "мусор" (GC), который тормозит игру.
            TriggerFlash();
            GameManager.GenerateFloatingText(Mathf.FloorToInt(totalDmg).ToString(), transform);
        }

        if (health <= 0) Kill();
    }

    public void TakeDamage(float dmg, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        // Вызываем основной метод урона
        TakeDamage(dmg);

        // Если враг еще жив и у него есть скрипт движения — применяем отброс
        if (health > 0 && movement != null)
        {
            // Рассчитываем направление от источника урона
            Vector2 dir = (Vector2)transform.position - sourcePosition;
            movement.Knockback(dir.normalized * knockbackForce, knockbackDuration);
        }
    }

    public void TakeDamage(float dmg, Vector2 sourcePosition, float knockbackForce)
    {
        TakeDamage(dmg, sourcePosition, knockbackForce, 0.2f);
    }

    // Замена корутины вспышки на быстрый метод
    void TriggerFlash()
    {
        StopCoroutine("DamageFlash"); // Останавливаем старую, если еще идет
        StartCoroutine(DamageFlash());
    }


    public override void RestoreHealth(float amount)
    {
        if (health < actualStats.maxHealth)
        {
            health += amount;
            if (health > actualStats.maxHealth) health = actualStats.maxHealth;
        }
    }

    IEnumerator DamageFlash()
    {
        ApplyTint(damageColor);
        yield return new WaitForSeconds(damageFlashDuration);
        RemoveTint(damageColor);
    }

    public override void Kill()
    {
        // 1. Сначала засчитываем комбо
        if (cachedCombo == null) cachedCombo = FindAnyObjectByType<KillComboManager>();
        if (cachedCombo != null) cachedCombo.OnEnemyKilled();

        // Ищем компонент дропа
        DropRateManager dropsScript = GetComponent<DropRateManager>();
        if (dropsScript != null)
        {
            // ВАЖНО: сначала проверяем условия, потом вызываем генерацию
            // Убеждаемся, что movement позволяет дроп (враг не за границей экрана)
            dropsScript.active = (movement != null) ? movement.giveExperienceOnDeath : true;

            // ВЫЗЫВАЕМ СПАВН ОПЫТА
            dropsScript.GenerateDrops();
        }

        // Выключаем коллайдер и движение
        if (enemyCollider != null) enemyCollider.enabled = false;
        if (movement != null) movement.enabled = false;

        StartCoroutine(KillFade());
    }

    // *** ИЗМЕНЕНО: KillFade ТЕПЕРЬ ИСПОЛЬЗУЕТ ОБЪЕКТ ПУЛИНГ ВМЕСТО DESTROY ***
    IEnumerator KillFade()
    {
        float t = 0;
        Color startColor = sprite.color;

        // ОПТИМИЗАЦИЯ: Используем null вместо WaitForEndOfFrame (экономит память)
        while (t < deathFadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / deathFadeTime);
            sprite.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        gameObject.SetActive(false);
    }
    // *************************************************************************

    void OnTriggerStay2D(Collider2D col)
    {
        if (enemyCollider == null || !enemyCollider.enabled || actualStats.damage <= 0) return;

        if (col.CompareTag("Player")) // Быстрая проверка по тегу перед GetComponent
        {
            if (col.TryGetComponent(out PlayerStats p))
            {
                p.TakeDamage(actualStats.damage);
                // Применяем баффы только если они есть
                if (attackEffects != null && attackEffects.Length > 0)
                {
                    foreach (BuffInfo b in attackEffects) p.ApplyBuff(b);
                }
            }
        }
    }

    // *** ИЗМЕНЕНО: УДАЛЕН МЕТОД ONDESTROY() ***
}
