using UnityEngine;

public class BossManager : MonoBehaviour
{
    int currentBossIndex; //The index of the current boss [Remember, a list starts from 0]

    // Нам все еще нужно отслеживать, был ли спавн для текущего босса уже завершен
    bool hasSpawnedCurrentBoss = false;

    // int currentBossSpawnCount = 0; //Tracks how many enemies current boss has spawned

    public BossData[] data;
    public Camera referenceCamera;

    [Tooltip("If there are more than this number of enemies, stop spawning any more. For performance")]
    public int maximumEnemyCount = 300;

    // float spawnTimer; //Timer used to determine when to spawn the next group of enemy

    float currentBossDuration = 0f;
    public bool boostedByCurse = true;

    public static BossManager instance;

    void Start()
    {
        if (instance) Debug.LogWarning("There is more than 1 Spawn Manager in the Scene! Plese remove the extras");
        instance = this;
    }

    void Update()
    {
        currentBossDuration += Time.deltaTime; // Это время с начала текущей фазы босса/игры

        // Если все боссы уже прошли, выходим
        if (currentBossIndex >= data.Length)
        {
            enabled = false;
            return;
        }

        BossData currentBoss = data[currentBossIndex];
        float triggerTimeInSeconds = currentBoss.triggerMinutes * 60f;

// --- НОВАЯ ЛОГИКА ---

        // 1. Проверяем время и флаг однократного спавна
        if (!hasSpawnedCurrentBoss && currentBossDuration >= triggerTimeInSeconds)
        {
            Debug.Log($"Активация босса {currentBoss.name} в {currentBossDuration}s! Спавним всех врагов.", this);

            // Получаем массив всех врагов, которые должны появиться (используем GetSpawns, который теперь возвращает весь массив enemies)
            GameObject[] spawns = currentBoss.GetSpawns(); 
            
            int totalEnemiesToSpawn = spawns.Length;

            // Спавним всех врагов сразу
            foreach (GameObject prefab in spawns)
            {
                // Проверка лимита производительности
                if (HasExceededMaxEnemies())
                {
                    Debug.LogWarning("Превышено максимальное количество врагов в сцене, прерывание спавна босса.");
                    break; // Прерываем цикл спавна
                }

                Instantiate(prefab, GeneratePosition(), Quaternion.identity);
            }
            
            // Устанавливаем флаг, что этот босс был заспавнен
            hasSpawnedCurrentBoss = true;
            // currentBossSpawnCount больше не нужен для отслеживания *количества* заспавненного в Update,
            // но нам нужно знать, сколько всего должно было быть заспавнено для логики HasBossEnded().
            // Поскольку мы удалили totalSpawns из BossData, логика завершения волны изменится.
        }
        
        // 2. Проверяем, пора ли переходить к следующему боссу
        // Эта проверка должна происходить каждый кадр, а не только после спавна.
        if (hasSpawnedCurrentBoss && HasBossEnded())
        {
            currentBossIndex++;
            currentBossDuration = 0; 
            hasSpawnedCurrentBoss = false; // Сбрасываем флаг для следующего босса

            if (currentBossIndex >= data.Length)
            {
                Debug.Log("All bosses have been spawned! Shutting down", this);
                enabled = false;
            }
        }
    }

    //Allows other scripts to check if we have exceeded the maximum number of enemies
    public static bool HasExceededMaxEnemies()
    {
        if (!instance) return false; //If there is no spawn manager, dont limit max enemies
        if (WaveManager.instance.activeEnemies.Count > instance.maximumEnemyCount) return true;
        return false;
    }

    public bool HasBossEnded()
    {
        BossData currentBoss = data[currentBossIndex];

        // Если mustKillAll включен, волна не заканчивается, пока есть враги.
        if (currentBoss.mustKillAll)
        {
            if (WaveManager.instance.activeEnemies.Count > 0)
            {
                return false; // Еще не закончилась, враги живы
            }
        }
        
        // Если mustKillAll выключен ИЛИ если mustKillAll включен, но врагов уже нет,
        // волна считается законченной сразу после спавна.
        return true; 
    }

    void Reset()
    {
        referenceCamera = Camera.main;
    }

    //Creates a new lacation where we can place the enemy at
    public static Vector3 GeneratePosition()
    {
        //If there is no reference camera, then get one
        if (!instance.referenceCamera) instance.referenceCamera = Camera.main;

        //Give a warning if the camera is not orthographic
        if (!instance.referenceCamera.orthographic)
            Debug.LogWarning("The reference camera is not orthographic! This will cause enemy spawns to sometimes appear within camera boundaries!");

        //Generate a position outside of camera boundaries using 2 random numbers
        float x = Random.Range(0f, 1f), y = Random.Range(0f, 1f);

        //Then, randomly choose whether we want to round the x or the y value
        switch (Random.Range(0, 2))
        {
            case 0:
            default:
                return instance.referenceCamera.ViewportToWorldPoint(new Vector3(Mathf.Round(x), y));
            case 1:
                return instance.referenceCamera.ViewportToWorldPoint(new Vector3(x, Mathf.Round(y)));
        }
    }

    //Checking if the enemy is wighin the cameras boundaries
    public static bool IsWithinBoundaries(Transform checkedObject)
    {
        //Get the camera to check if we are within boundaries
        Camera c = instance && instance.referenceCamera ? instance.referenceCamera : Camera.main;

        Vector2 viewport = c.WorldToViewportPoint(checkedObject.position);
        if (viewport.x < 0f || viewport.x > 1f) return false;
        if (viewport.y < 0f || viewport.y > 1f) return false;
        return true;
    }
}
