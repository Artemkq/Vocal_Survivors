using UnityEngine;

public abstract class SpawnData : ScriptableObject
{
    [Tooltip("Время между появлениями (в секундах).")]
    public float spawnInterval = 1f;

    [Tooltip("Как долго (в секундах) будут появляться враги.")]
    [Min(0.1f)] public float timeElapsed = 60;

    [Tooltip("Сколько врагов появляется за один интервал?")]
    public Vector2Int spawnsPerTick = new Vector2Int(1, 1);

    [Tooltip("Список всех возможных игровых объектов, которые можно создать")]
    public GameObject[] enemies = new GameObject[1];

    //Returns an array of prefabs that we should spawn
    //Takes an optional parameter of how many enemies are on the screen at the moment
    public virtual GameObject[] GetSpawns(int totalEnemies = 0)
    {
        //Determine how many enemies to spawn
        int count = Random.Range(spawnsPerTick.x, spawnsPerTick.y);

        //Generate the result
        GameObject[] result = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            //Randomly picks one of the possible spawns and inserts it
            //intro the result array
            result[i] = enemies[Random.Range(0, enemies.Length)];
        }

        return result;
    }

    //Get a random spawn interval between the min and max values
    public virtual float GetSpawnInterval()
    {
        return spawnInterval;
    }

    // Добавьте этот метод в SpawnData.cs, если его там еще нет:
    public virtual float GetTimeElapsed()
    {
        return timeElapsed;
    }
}
