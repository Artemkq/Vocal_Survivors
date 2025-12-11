using UnityEngine;

[CreateAssetMenu(fileName = "Swarm Event Data", menuName = "2D Top-down Rogue-like/Event Data/Swarm")]

public class SwarmEventData : EventData
{
    [Header("Swarm Data")]
    [Range(0f, 360f)] public float possibleAngles = 360f;
    [Min(0)] public float spawnRadius = 2f, spawnDistance = 20f;

    // ИЗМЕНЕНО: Переопределяем метод получения интервала спавна
    public override float GetSpawnInterval()
    {
        // Delay будет использоваться как фиксированный интервал между спавнами
        return delayPlusSpawnInterval;
    }

    // ИЗМЕНЕНО: Возвращаем специальное значение (например, 0), 
    // чтобы EventManager понял, что длительность ограничена повторениями, а не временем.
    public override float GetTimeElapsed()
    {
        return 0f; // Сигнализирует EventManager'у использовать maxRepeats
    }

    public override bool Activate(PlayerStats player = null)
    {
        //Only activate this if the player is present
        if (player)
        {
            //Otherwise, we spawn a mob outside of the screen and move it towards the player
            float randomAngle = Random.Range(0, possibleAngles) * Mathf.Deg2Rad;
            foreach (GameObject o in GetSpawns())
            {
                Instantiate(o, player.transform.position + new Vector3
                    (
                    (spawnDistance + Random.Range(-spawnRadius, spawnRadius)) * Mathf.Cos(randomAngle),
                    (spawnDistance + Random.Range(-spawnRadius, spawnRadius)) * Mathf.Sin(randomAngle)
                    ),
                    Quaternion.identity);
            }
        }
        return true; // Важно вернуть true, чтобы менеджер знал, что активация произошла успешно
    }
}
