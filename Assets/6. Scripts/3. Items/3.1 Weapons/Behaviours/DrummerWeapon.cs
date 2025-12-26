using UnityEngine;

public class DrummerWeapon : Weapon
{
    [Header("Drummer Settings")]
    public float perfectAreaMultiplier = 1.5f;

    protected override bool Attack(int attackCount = 1)
    {
        if (!CanAttack()) return false;

        bool isPerfect = BeatConductor.Instance.WasPressedThisWindow;
        float finalRadius = isPerfect ? GetArea() * perfectAreaMultiplier : GetArea();
        Color waveColor = isPerfect ? Color.red : Color.white;

        // 1. УРОН
        DealDamageInArea(finalRadius);

        // 2. ВИЗУАЛ
        // Берем данные из currentStats.auraPrefab, но работаем с ним как с GameObject
        if (currentStats.auraPrefab != null)
        {
            // Используем gameObject префаба напрямую
            GameObject visualObj = Instantiate(currentStats.auraPrefab.gameObject, owner.transform.position, Quaternion.identity);

            // Сразу задаем масштаб, чтобы его было видно, даже если скрипт не сработает
            visualObj.transform.localScale = new Vector3(finalRadius, finalRadius, 1);

            // Проверяем наличие скрипта DrumWave для покраски
            DrumWave dw = visualObj.GetComponent<DrumWave>();
            if (dw != null)
            {
                dw.SetupWave(finalRadius, waveColor);
            }
            else
            {
                // Если скрипта нет, удалим объект сами через 0.5 сек, чтобы не засорять память
                Destroy(visualObj, 0.5f);
            }
        }
        else
        {
            Debug.LogWarning("Aura Prefab не назначен в WeaponData!");
        }

        ActivateCooldown();
        return true;
    }

    private void DealDamageInArea(float radius)
    {
        // Physics2D.OverlapCircleAll — самый быстрый способ найти врагов в радиусе
        Collider2D[] targets = Physics2D.OverlapCircleAll(owner.transform.position, radius);

        foreach (Collider2D t in targets)
        {
            // Используем вашу логику урона
            if (t.TryGetComponent(out EnemyStats es))
            {
                es.TakeDamage(GetDamage(), owner.transform.position);
                ApplyBuffs(es);
            }
        }
    }
}
