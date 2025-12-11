using UnityEngine;

[CreateAssetMenu(fileName = "Ring Event Data", menuName = "2D Top-down Rogue-like/Event Data/Wall")]

public class WallEventData : EventData
{
    [Header("Wall Data")]
    public ParticleSystem spawnEffectPrefab;
    public Vector2 scale = new Vector2(1, 1);
    [Min(0)] public float spawnRadius = 10f, duration = 15f;

    // ИЗМЕНЕНО: Переопределяем метод получения интервала спавна
    public override float GetSpawnInterval()
    {
        // delayPlusSpawnInterval используется как фиксированный интервал между активациями кольца
        return delayPlusSpawnInterval;
    }

    // ИЗМЕНЕНО: Возвращаем 0, чтобы EventManager понял, что длительность 
    // управляется повторениями (maxRepeats), а не общим временем (timeElapsed)
    public override float GetTimeElapsed()
    {
        return 0f;
    }

    public override bool Activate(PlayerStats player = null)
    {
        //Only activate this if the player is present
        if (player)
        {
            GameObject[] spawns = GetSpawns();
            float angleOffset = 2 * Mathf.PI / Mathf.Max(1, spawns.Length);
            float currentAngle = 0;
            foreach (GameObject g in spawns)
            {
                //Calculate the spawn position
                Vector3 spawnPosition = player.transform.position + new Vector3
                (
                    spawnRadius * Mathf.Cos(currentAngle) * scale.x,
                    spawnRadius * Mathf.Sin(currentAngle) * scale.y
                );

                //If a particle effect is assigned, play it on the position
                if (spawnEffectPrefab)
                    Instantiate(spawnEffectPrefab, spawnPosition, Quaternion.identity);

                //Then spawn the enemy
                GameObject s = Instantiate(g, spawnPosition, Quaternion.identity);

                //If there is a lifespan on the mob, set them to be destroyed
                if (duration > 0)
                {
                    // Получаем компонент EnemyMovement у созданного врага
                    EnemyMovement enemyMovement = s.GetComponent<EnemyMovement>();

                    if (enemyMovement != null)
                    {
                        // ИЗМЕНЕНО: Передаем duration в метод Despawn()
                        enemyMovement.Despawn(duration);
                    }
                    else
                    {
                        // Запасной вариант, если по какой-то причине нет EnemyMovement
                        Destroy(s, duration);
                    }
                }
                currentAngle += angleOffset;
            }
        }
        return true;
    }
}
