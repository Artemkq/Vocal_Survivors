using UnityEngine;
using System.Collections.Generic; // Убедитесь, что эта строка есть вверху

public class WaveManager : MonoBehaviour
{
    int currentWaveIndex; //The index of the current wave [Remember, a list starts from 0]
    int currentWaveSpawnCount = 0; //Tracks how many enemies current wave has spawned

    public WaveData[] data;
    public Camera referenceCamera;

    [Tooltip("If there are more than this number of enemies, stop spawning any more. For performance")]
    public int maximumEnemyCount = 300;
    float spawnTimer; //Timer used to determine when to spawn the next group of enemy
    float currentWaveDuration = 0f;
    public bool boostedByCurse = true;

    public static WaveManager instance;

    // !!! НОВОЕ: Публичный список активных врагов !!!
    public List<EnemyStats> activeEnemies = new List<EnemyStats>();

    void Start()
    {
        if (instance) Debug.LogWarning("There is more than 1 Spawn Manager in the Scene! Plese remove the extras");
        instance = this;
    }

    void Update()
    {
        //Update the spawn timer at every frame
        spawnTimer -= Time.deltaTime;
        currentWaveDuration += Time.deltaTime;

        if (spawnTimer <= 0)
        {
            //Check if we are ready to move on to the new wave
            if (HasWaveEnded())
            {
                currentWaveIndex++;
                currentWaveDuration = currentWaveSpawnCount = 0;

                //If we have gone through all the waves, disable this component
                if (currentWaveIndex >= data.Length)
                {
                    Debug.Log("All waves have been spawned! Shutting down", this);
                    enabled = false;
                }

                return;
            }

            //Do not spawn enemies if we do not meet the conditions to do so
            if (!CanSpawn())
            {
                ActivateCooldown();
                return;
            }

            //Get the array of enemies that we are spawning for this tick
            GameObject[] spawns = data[currentWaveIndex].GetSpawns(activeEnemies.Count);

            // Loop through and spawn all the prefabs
            foreach (GameObject prefab in spawns)
            {
                if (!CanSpawn()) continue;

                // ОПТИМИЗАЦИЯ: Сначала проверяем, есть ли свободный объект в пуле, 
                // чтобы не делать лишних расчетов позиций и NavMesh
                Vector3 spawnPos = GeneratePosition();
                spawnPos.z = 0;

                UnityEngine.AI.NavMeshHit hit;
                // Делаем SamplePosition только если точка действительно нужна
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    // ИСПОЛЬЗУЕМ ПУЛ ВМЕСТО INSTANTIATE
                    GameObject enemy = ObjectPooler.Instance.GetFromPool(prefab, hit.position, Quaternion.identity);

                    if (enemy != null)
                    {
                        currentWaveSpawnCount++;
                    }
                }
            }

            ActivateCooldown();
        }
    }

    // !!! НОВЫЕ МЕТОДЫ: Для регистрации и дерегистрации врагов !!!
    public void RegisterEnemy(EnemyStats enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public void DeregisterEnemy(EnemyStats enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    //Reset the spawn interval
    public void ActivateCooldown()
    {
        float curseBoost = boostedByCurse ? GameManager.GetCumulativeCurse() : 1;
        spawnTimer += data[currentWaveIndex].GetSpawnInterval() / curseBoost;
    }

    //Do we meet the conditions to be able to continue spawning?
    public bool CanSpawn()
    {
        //Dont spawn anymore if we exceed the max limit
        if (HasExceededMaxEnemies()) return false;

        //Dont spawn if we exceeded the max spawns for the wave
        if (instance.currentWaveSpawnCount > instance.data[instance.currentWaveIndex].totalSpawns) return false;

        //Dont spawn if we exceeded the waves duration
        if (instance.currentWaveDuration > instance.data[instance.currentWaveIndex].timeElapsed) return false;

        return true;
    }

    //Allows other scripts to check if we have exceeded the maximum number of enemies
    public static bool HasExceededMaxEnemies()
    {
        if (!instance) return false; //If there is no spawn manager, dont limit max enemies
        if (instance.activeEnemies.Count > instance.maximumEnemyCount) return true;
        return false;
    }

    public bool HasWaveEnded()
    {
        WaveData currentWave = data[currentWaveIndex];

        //If waveDuration is one of the exit conditions, check how long the wave has been running
        //If current wave durations is not greater than wave duration, do not exit yet
        if ((currentWave.exitConditions & WaveData.ExitCondition.waveDuration) > 0)
            if (currentWaveDuration < currentWave.timeElapsed) return false;

        //If rechedTotalSpawns is one of the exit conditions, check if we have spawned enough
        //enemies. If not, return false
        if ((currentWave.exitConditions & WaveData.ExitCondition.reachedTotalSpawns) > 0)
            if (currentWaveSpawnCount < currentWave.totalSpawns) return false;

        //Otherwise, if kill all is checked, we have to make sure there are no more enemies first
        if (currentWave.mustKillAll && activeEnemies.Count > 0)
            return false;

        return true;
    }

    void Reset()
    {
        referenceCamera = Camera.main;
    }

    //Creates a new location where we can place the enemy at (с отступом 1.1f и 360 градусов)
    public static Vector3 GeneratePosition()
    {
        // Если there is no reference camera, then get one
        if (!instance.referenceCamera) instance.referenceCamera = Camera.main;

        // Give a warning if the camera is not orthographic
        if (!instance.referenceCamera.orthographic)
            Debug.LogWarning("The reference camera is not orthographic! This will cause enemy spawns to sometimes appear within camera boundaries!");

        // --- УСТАНАВЛИВАЕМ ЖЕЛАЕМЫЙ ОТСТУП/ДИСТАНЦИЮ ---
        // Это минимальное расстояние от центра экрана до точки спавна (в единицах вьюпорта, 1.0f = край экрана)
        const float spawnDistance = 1f;

        // 1. Выбираем случайный угол в радианах (0 до 2*PI)
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);

        // 2. Используем синус и косинус для получения координат X и Y на окружности
        float x = Mathf.Cos(randomAngle) * spawnDistance;
        float y = Mathf.Sin(randomAngle) * spawnDistance;

        // 3. Преобразуем полученные координаты (которые теперь находятся на окружности радиусом 1.1f)
        //    из вьюпорта в мировые координаты.
        //    Обратите внимание: ViewportToWorldPoint ожидает координаты от 0 до 1, 
        //    но мы можем передать и другие значения для расчета точек вне экрана.
        //    Нам нужно отцентрировать координаты, добавив 0.5f к x и y (так как 0,0 в вьюпорте это левый нижний угол)

        Vector3 spawnViewportPoint = new Vector3(x + 0.5f, y + 0.5f, instance.referenceCamera.nearClipPlane);
        Vector3 worldPoint = instance.referenceCamera.ViewportToWorldPoint(spawnViewportPoint);
        worldPoint.z = 0f; // Принудительно обнуляем Z для 2D NavMesh
        return worldPoint;
    }

    //Checking if the enemy is wighin the cameras boundaries
    public static bool IsWithinBoundaries(Transform checkedObject)
    {
        // Устанавливаем отступ (padding). (Ваше предыдущее значение)
        const float padding = 1f;

        //Get the camera to check if we are within boundaries
        Camera c = instance && instance.referenceCamera ? instance.referenceCamera : Camera.main;

        Vector2 viewport = c.WorldToViewportPoint(checkedObject.position);

        // Проверяем границы с учетом отступа:
        if (viewport.x < 0f - padding || viewport.x > 1f + padding) return false;
        if (viewport.y < 0f - padding || viewport.y > 1f + padding) return false;

        return true;
    }
}
