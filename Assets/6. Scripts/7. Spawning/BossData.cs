using UnityEngine;

[CreateAssetMenu(fileName = "Boss Data", menuName = "2D Top-down Rogue-like/Boss Data")]

public class BossData : SpawnData
{
    [Header("Boss Data")]

    [Tooltip("Время в минутах (от 0 до 30), когда это событие должно быть активировано")]
    [Range(0f, 30f)] public float triggerMinutes;

    [Tooltip("Чтобы волна продвинулась дальше, все враги должны быть мертвы")]
    public bool mustKillAll = false;

    //Returns an array of prefabs that this boss can spawn
    //Takes an optional parameter of how many enemies are on the screen at the moment
    public override GameObject[] GetSpawns(int totalEnemies = 0)
    {
        // Просто возвращаем весь массив префабов, указанных в SO
        return enemies;
    }

    // НОВОЕ: Переопределяем, чтобы не использовать постепенный спавн
    public override float GetSpawnInterval()
    {
        // В новой логике этот метод не будет использоваться для определения частоты спавна.
        // Менеджер будет проверять время и спавнить всех сразу.
        return 9999f; // Просто заглушка
    }

    // НОВОЕ: Возвращаем 0, чтобы сигнализировать, что волна завершается сразу после однократного спавна
    public override float GetTimeElapsed()
    {
        return 0f; 
    }
}
