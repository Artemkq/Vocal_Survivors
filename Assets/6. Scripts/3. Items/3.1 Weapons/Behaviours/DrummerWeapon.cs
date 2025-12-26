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

        // 1. УРОН
        DealDamageInArea(finalRadius);

        // 2. ВИЗУАЛ
        if (currentStats.auraPrefab != null)
        {
            GameObject visualObj = Instantiate(currentStats.auraPrefab.gameObject, owner.transform.position, Quaternion.identity);
            visualObj.transform.localScale = new Vector3(finalRadius, finalRadius, 1);

            DrumWave dw = visualObj.GetComponent<DrumWave>();
            if (dw != null)
            {
                // Вызываем без передачи цвета
                dw.SetupWave(finalRadius);
            }
            else
            {
                Destroy(visualObj, 0.5f);
            }
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
