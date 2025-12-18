using UnityEngine.AI;
using NavMeshPlus.Components; // Для NavMeshPlus
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public List<GameObject> terrainChunks;
    public GameObject player;
    public float checkerRadius;
    public LayerMask terrainMask;
    public GameObject currentChunk;
    Vector3 playerLastPosition;


    [Header("Optimization")]
    public List<GameObject> spawnedChunks;
    public GameObject latestChunk;
    public float maxOpDist;
    float optimizerCooldown;
    public float optimizerCooldepownDur;

    [SerializeField] private NavMeshSurface navMeshSurface; // Ссылка на NavMesh Surface на сцене (можно назначить вручную в инспекторе)

    [Header("NavMesh Settings")]
    [SerializeField] private float bakeDelayBetweenChunks = 1f; // Задержка между Bake для каждого нового чанка (в секундах)
    [SerializeField] private float initialBakeDelay = 1f; // Начальная задержка перед первым BuildNavMesh (для инициализации Tilemap)
    [SerializeField] private bool useBaking = false; // ОТКЛЮЧЕНО: Из-за ошибок NavMeshPlus при частых Bake. Используй BakeAllChunksManually() вместо этого
    private int pendingChunksCount = 0; // Количество чанков в очереди на Bake
    private bool isBaking = false; // Флаг, что Bake в процессе

    void Start()
    {
        playerLastPosition = player.transform.position;

        // Автоматически находим NavMeshSurface на сцене (включая неактивные объекты)
        if (navMeshSurface == null)
        {
            // Ищем среди активных объектов
            navMeshSurface = FindAnyObjectByType<NavMeshSurface>();

            // Если не нашли, ищем среди неактивных
            if (navMeshSurface == null)
            {
                navMeshSurface = FindAnyObjectByType<NavMeshSurface>(FindObjectsInactive.Include);
            }

            if (navMeshSurface == null)
            {
                Debug.LogError("NavMeshSurface не найден на сцене! Проверь, что объект NavMeshSurface существует и имеет компонент NavMeshSurface.");
            }
            else
            {
                Debug.Log("NavMeshSurface успешно найден: " + navMeshSurface.gameObject.name);
            }
        }
    }


    void Update()
    {
        ChunkChecker();
        ChunkOptimizer();
    }

    void ChunkChecker()
    {
        if (!currentChunk)
        {
            return;
        }

        Vector3 moveDir = player.transform.position - playerLastPosition;
        playerLastPosition = player.transform.position;

        string directionName = GetDirectionName(moveDir);

        CheckAndSpawnChunk(directionName);

        //Check additional adjacent directions for diagonal chunks
        if (directionName.Contains("Up"))
        {
            CheckAndSpawnChunk("Up");
        }

        if (directionName.Contains("Down"))
        {
            CheckAndSpawnChunk("Down");
        }

        if (directionName.Contains("Right"))
        {
            CheckAndSpawnChunk("Right");
        }

        if (directionName.Contains("Left"))
        {
            CheckAndSpawnChunk("Left");
        }
    }

    void CheckAndSpawnChunk(string direction)
    {
        if (!Physics2D.OverlapCircle(currentChunk.transform.Find(direction).position, checkerRadius, terrainMask))
        {
            SpawnChunk(currentChunk.transform.Find(direction).position);
        }
    }

    string GetDirectionName(Vector3 direction)
    {
        direction = direction.normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            //Moving horizontally more than vertically
            if (direction.y > 0.5f)
            {
                //Also moving upwards
                return direction.x > 0 ? "Right Up" : "Left Up";
            }
            else if (direction.y < -0.5f)
            {
                //Also moving downwards
                return direction.x > 0 ? "Right Down" : "Left Down";
            }
            else
            {
                //Moving straight horizontally
                return direction.x > 0 ? "Right" : "Left";
            }
        }
        else
        {
            //Moving vertically more than horizontally
            if (direction.x > 0.5f)
            {
                //Also moving right
                return direction.y > 0 ? "Right Up" : "Right Down";
            }
            else if (direction.x < -0.5f)
            {
                //Also moving left
                return direction.y > 0 ? "Left Up" : "Left Down";
            }
            else
            {
                //Moving straight vertically
                return direction.y > 0 ? "Up" : "Down";
            }
        }
    }

    // Метод для постановки чанка в очередь на Bake
    public void BakeNavMesh()
    {
        if (!useBaking || navMeshSurface == null)
            return;

        // Защита от переполнения очереди
        if (pendingChunksCount >= 50)
        {
            Debug.LogWarning($"Очередь Bake переполнена! Текущее количество: {pendingChunksCount}");
            return;
        }

        pendingChunksCount++;

        // Если Bake уже запущен, просто добавляем в очередь
        if (isBaking)
        {
            return;
        }

        // Запускаем очередь Bake с распределенной задержкой для каждого чанка
        StartCoroutine(ProcessBakeQueue());
    }

    // Обработка очереди Bake с задержкой между каждым чанком
    private System.Collections.IEnumerator ProcessBakeQueue()
    {
        isBaking = true;

        while (pendingChunksCount > 0)
        {
            // 1. Ждем стабилизации объектов
            yield return new WaitForSeconds(initialBakeDelay);

            if (navMeshSurface != null)
            {
                // Сбрасываем очередь, так как один Bake обновит всё сразу вокруг игрока
                pendingChunksCount = 0;

                // 2. ОПТИМИЗАЦИЯ: Чтобы не было лага, мы временно снижаем качество запекания 
                // или используем Physics Colliders.

                // ВАЖНО: Перед вызовом BuildNavMesh мы убеждаемся, что не делаем это слишком часто
                navMeshSurface.BuildNavMesh();

                Debug.Log($"[Bake] ✓ Сетка обновлена. Чанков в памяти: {spawnedChunks.Count}");
            }

            // 3. ДАЕМ ИГРЕ "ПОДЫШАТЬ" (Это убирает микро-фриз после запекания)
            yield return new WaitForEndOfFrame();

            pendingChunksCount--;
            yield return new WaitForSeconds(bakeDelayBetweenChunks);
        }

        isBaking = false;
    }

    void SpawnChunk(Vector3 spawnPosition)
    {
        int rand = Random.Range(0, terrainChunks.Count);
        latestChunk = Instantiate(terrainChunks[rand], spawnPosition, Quaternion.identity);
        spawnedChunks.Add(latestChunk);

        // Вместо мгновенного вызова, используйте Invoke или таймер
        CancelInvoke("TriggerDeferredBake");
        Invoke("TriggerDeferredBake", 0.5f); // Запечь только через 0.5 сек после последнего спавна
    }

    void TriggerDeferredBake()
    {
        BakeNavMesh();
    }

    /// <summary>
    /// Простой метод для Bake после задержки без попыток очистки
    /// </summary>
    private System.Collections.IEnumerator BakeNavMeshAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (navMeshSurface != null)
        {
            try
            {
                navMeshSurface.BuildNavMesh();
                Debug.Log($"✓ NavMesh обновлен. Спавнено чанков: {spawnedChunks.Count}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠ Ошибка при Bake: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Вызови этот метод через небольшую задержку после появления чанков 
    /// (например, в конце сцены или когда игрок остановился)
    /// </summary>
    public void BakeAllChunksManually()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface не найден!");
            return;
        }

        Debug.Log("Начинаем полный Bake всех чанков...");
        StartCoroutine(ManualBakeRoutine());
    }

    private System.Collections.IEnumerator ManualBakeRoutine()
    {
        // Ждем кадр для полной инициализации всех чанков
        yield return new WaitForSeconds(0.5f);

        bool shouldWait = false;

        try
        {
            // Очищаем старые данные
            if (navMeshSurface.navMeshData != null)
            {
                navMeshSurface.RemoveData();
                shouldWait = true;
            }

            // Строим новый NavMesh
            navMeshSurface.BuildNavMesh();
            Debug.Log("✓ NavMesh успешно создан!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ Ошибка при Bake: {e.Message}");
        }

        // Ждем после очистки если нужно было очищать данные
        if (shouldWait)
        {
            yield return new WaitForSeconds(0.2f);
        }
    }

    void ChunkOptimizer()
    {
        // 1. ПРОВЕРКА: Если сейчас идет запекание NavMesh, выходим из метода.
        // Мы не должны отключать или удалять объекты, пока NavMeshPlus их сканирует!
        if (isBaking)
        {
            return;
        }

        optimizerCooldown -= Time.deltaTime;
        if (optimizerCooldown > 0f) return;
        optimizerCooldown = optimizerCooldepownDur;

        // Используем обратный цикл for, чтобы безопасно удалять объекты из списка
        for (int i = spawnedChunks.Count - 1; i >= 0; i--)
        {
            GameObject chunk = spawnedChunks[i];

            // Проверка на null (если объект уже был удален кем-то другим)
            if (chunk == null)
            {
                spawnedChunks.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(player.transform.position, chunk.transform.position);

            // 2. УДАЛЕНИЕ: Если чанк ОЧЕНЬ далеко, удаляем его из памяти
            if (dist > maxOpDist * 2.5f) // Коэффициент 2.5, чтобы не удалять то, что только что скрылось
            {
                spawnedChunks.RemoveAt(i);
                Destroy(chunk);
                continue; // Переходим к следующему чанку
            }

            // 3. СКРЫТИЕ: Если чанк просто вне зоны видимости, выключаем его
            bool shouldBeActive = dist <= maxOpDist;
            if (chunk.activeSelf != shouldBeActive)
            {
                chunk.SetActive(shouldBeActive);
            }
        }
    }
}
