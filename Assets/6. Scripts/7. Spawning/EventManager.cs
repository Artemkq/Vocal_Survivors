using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Понадобится для сортировки

public class EventManager : MonoBehaviour
{
    // float currentEventCooldown = 0;

    public EventData[] events; // Теперь это наш таймлайн событий
    public static EventManager instance;

    // Мы используем эту структуру только для отслеживания *запущенных* событий
    [System.Serializable]
    public class RunningEventState
    {
        public EventData data;
        public float durationLeft; // УДАЛЯЕМ это поле, оно больше не нужно
        public float currentCooldown; // Кулдаун внутри самого события (например, между спавнами мобов)
        public int repeatsLeft; // НОВОЕ ПОЛЕ: Сколько активаций осталось
    }

    List<RunningEventState> runningEvents = new List<RunningEventState>();
    // Список событий, которые еще не произошли, отсортированный по времени
    List<EventData> plannedEvents;
    PlayerStats[] allPlayers;
    float gameTimer = 0f; // Новый таймер игры, отсчитывающий время с начала сцены

    //Start is called before the first frame update
    void Start()
    {
        if (instance) Debug.LogWarning("There is more than 1 Spawn Manager in the Scene! Please remove the extras");
        instance = this;

        // Находим всех игроков на старте сцены
        allPlayers = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

        // 1. Инициализируем и сортируем события по triggerTime
        if (events != null && events.Length > 0)
        {
            // Копируем события в список и сортируем по triggerTime по возрастанию
            plannedEvents = events.OrderBy(e => e.triggerMinutes).ToList();
        }
        else
        {
            plannedEvents = new List<EventData>();
        }

        // currentEventCooldown = firstTriggerDelay > 0 ? firstTriggerDelay : triggerInterval;
        // allPlayers = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
    }

    //Update is called once per frame
    void Update()
    {
        // Убедимся, что игроки существуют
        if (allPlayers == null || allPlayers.Length == 0) return;

        // Обновляем таймер игры
        gameTimer += Time.deltaTime;

        // --- Логика Планировщика (Timeline Logic) ---
        CheckTimelineEvents();

        // --- Логика Активных Событий (Running Events Logic) ---
        UpdateRunningEvents();
    }

    /// <summary>
    /// Проверяет список запланированных событий и запускает те, чье время пришло.
    /// </summary>
    private void CheckTimelineEvents()
    {
        // Итерируемся в обратном порядке, чтобы безопасно удалять из списка plannedEvents при необходимости
        for (int i = plannedEvents.Count - 1; i >= 0; i--)
        {
            EventData e = plannedEvents[i];

            // Если текущее время игры достигло или превысило время триггера события
            if (gameTimer >= e.triggerMinutes * 60f)
            {
                // При запуске события инициализируем repeatsLeft из maxRepeats
                if (e.IsActive() && e.CheckIfWillHappen(allPlayers[Random.Range(0, allPlayers.Length)]))
                {
                    // Запускаем событие, добавляя его в список активных
                    runningEvents.Add(new RunningEventState
                    {
                        data = e,
                        // durationLeft = e.timeElapsed, // УДАЛЕНО
                        currentCooldown = 0, // Начнем внутренний кулдаун сразу
                        repeatsLeft = e.maxRepeats == 0 ? int.MaxValue : e.maxRepeats // 0 в конфиге означает бесконечность
                    });
                }
                // В любом случае (запустилось или пропущено по шансу), 
                // удаляем событие из списка ожидания, чтобы оно не сработало повторно.
                plannedEvents.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Обновляет состояние всех запущенных в данный момент событий.
    /// </summary>
    private void UpdateRunningEvents()
    {
        List<RunningEventState> toRemove = new List<RunningEventState>();

        foreach (RunningEventState e in runningEvents)
        {
            // УДАЛЕНО: Логика уменьшения durationLeft удалена.

            // 2. Уменьшаем внутренний интервал спавна/активации (cooldown между тиками спавна)
            e.currentCooldown -= Time.deltaTime;
            if (e.currentCooldown <= 0)
            {
                // Если активация успешна И это не бесконечное событие
                if (e.data.Activate(allPlayers[Random.Range(0, allPlayers.Length)]) && e.repeatsLeft != int.MaxValue)
                {
                    e.repeatsLeft--;
                }

                // Сбрасываем внутренний кулдаун
                e.currentCooldown = e.data.GetSpawnInterval();
            }

            // НОВАЯ ЛОГИКА ЗАВЕРШЕНИЯ: Проверяем, кончились ли повторения ИЛИ прошло ли время

            // Если событие в SpawnData имеет timeElapsed > 0, то оно управляется временем
            if (e.data.GetTimeElapsed() > 0)
            {
                // Нам нужно как-то отследить время, если мы его убрали из RunningEventState.
                // Лучше всего вернуть durationLeft, но использовать его опционально.
                // Давайте откатим удаление durationLeft и используем оба поля.
            }

            // --- Откат изменений: Вернем durationLeft и сделаем проверку более умной ---

            /* 
            // Возвращаем durationLeft обратно в RunningEventState, чтобы он работал для стандартных событий.
            // Нужно просто изменить логику удаления.
            */
        }

        // Давайте внесем финальную правку в UpdateRunningEvents, используя оба подхода
        UpdateRunningEvents_V2();
    }
    // Новая версия UpdateRunningEvents, которая учитывает оба режима работы
    private void UpdateRunningEvents_V2()
    {
        List<RunningEventState> toRemove = new List<RunningEventState>();

        foreach (RunningEventState e in runningEvents)
        {
            // Если событие управляется временем (GetTimeElapsed() > 0), уменьшаем таймер
            if (e.data.GetTimeElapsed() > 0f)
            {
                // Нужен durationLeft в RunningEventState. Верните его.
                e.durationLeft -= Time.deltaTime;
                if (e.durationLeft <= 0)
                {
                    toRemove.Add(e); 
                    continue;
                }
                
            }
            // В противном случае оно управляется repeatsLeft, который мы будем уменьшать при активации

            // Уменьшаем внутренний интервал спавна/активации
            e.currentCooldown -= Time.deltaTime;
            if (e.currentCooldown <= 0)
            {
                // Выполняем активацию
                bool activatedSuccessfully = e.data.Activate(allPlayers[Random.Range(0, allPlayers.Length)]);

                if (activatedSuccessfully)
                {
                    // Если событие управляется повторениями (GetTimeElapsed() == 0f) И это не бесконечность (MaxValue)
                    if (e.data.GetTimeElapsed() == 0f && e.repeatsLeft != int.MaxValue)
                    {
                        e.repeatsLeft--;
                    }

                    // Сбрасываем внутренний кулдаун
                    e.currentCooldown = e.data.GetSpawnInterval();
                }
            }

            // Проверка на удаление в конце цикла
            bool timeElapsedFinished = e.data.GetTimeElapsed() > 0f && e.durationLeft <= 0f ; // Зависит от durationLeft
            bool repeatsFinished = e.data.GetTimeElapsed() == 0f && e.repeatsLeft <= 0;

            if (timeElapsedFinished || repeatsFinished)
            {
                toRemove.Add(e);
            }
        }

        // Удаляем все завершившиеся события из основного списка
        foreach (RunningEventState e in toRemove)
        {
            runningEvents.Remove(e);
        }
    }
}