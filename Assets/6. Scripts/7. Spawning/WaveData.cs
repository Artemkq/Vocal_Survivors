using UnityEngine;

[CreateAssetMenu(fileName = "Wave Data", menuName = "2D Top-down Rogue-like/Wave Data")]

public class WaveData : SpawnData
{
    [Header("Wave Data")]

    [Tooltip("Если врагов будет меньше указанного количества, то они будут продолжать появляться, пока не достигнут цели")]
    [Min(0)] public int enemyMinimum = 0;

    [Tooltip("Сколько максимельно врагов может появиться в этой волне?")]
    [Min(1)] public int totalSpawns = int.MaxValue; // ИЗМЕНЕНО с uint на int

    [System.Flags] public enum ExitCondition { waveDuration = 1, reachedTotalSpawns = 2 }
    [Tooltip("Определите факторы, которые могут привести к завершению этой волны")]
    public ExitCondition exitConditions = (ExitCondition)1;

    [Tooltip("Чтобы волна продвинулась дальше, все враги должны быть мертвы")]
    public bool mustKillAll = false;

    [HideInInspector] public int spawnCount; //The number of enemies already spawned in this wave

    //Returns an array of prefabs that this wave can spawn
    //Takes an optional parameter of how many enemies are on the screen at the moment
    public override GameObject[] GetSpawns(int totalEnemies = 0)
    {
        //Determinate how many enemies to spawn
        int count = Random.Range(spawnsPerTick.x, spawnsPerTick.y);

        //If we have less than <minimumEnemies> on the screen, we will
        //set the count to be equals to the number of enemies to spawn to
        //populate the screen untill it have <minimumEnemies> within
        if (totalEnemies + count < enemyMinimum)
            count = enemyMinimum - totalEnemies;

        //Generate the result
        GameObject[] result = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            //Randomly picks one of the possible spawns and insers it
            //intro the result array
            result[i] = enemies[Random.Range(0, enemies.Length)];
        }

        return result;
    }
}
