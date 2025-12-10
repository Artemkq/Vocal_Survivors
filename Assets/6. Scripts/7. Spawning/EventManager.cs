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
        public float durationLeft; // Оставшееся время работы события (из EventData.timeElapsed)
        public float currentCooldown; // Кулдаун внутри самого события (например, между спавнами мобов)
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
                // Если событие активно и проходит проверку шанса (ваша старая логика)
                // (e.IsActive() использует ваш старый 'delay', e.CheckIfWillHappen использует 'chance' и 'luckFactor')
                if (e.IsActive() && e.CheckIfWillHappen(allPlayers[Random.Range(0, allPlayers.Length)]))
                {
                    // Запускаем событие, добавляя его в список активных
                    runningEvents.Add(new RunningEventState
                    {
                        data = e,
                        durationLeft = e.timeElapsed, // Длительность взята из SpawnData/EventData
                        currentCooldown = 0 // Начнем внутренний кулдаун сразу
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
            // 1. Уменьшаем общую длительность события
            e.durationLeft -= Time.deltaTime;
            if (e.durationLeft <= 0)
            {
                toRemove.Add(e); // Событие истекло, помечаем на удаление
                continue;
            }

            // 2. Уменьшаем внутренний интервал спавна/активации (cooldown между тиками спавна)
            e.currentCooldown -= Time.deltaTime;
            if (e.currentCooldown <= 0)
            {
                // Выбираем случайного игрока и активируем эффект/спавн
                e.data.Activate(allPlayers[Random.Range(0, allPlayers.Length)]);

                // Сбрасываем внутренний кулдаун (получаем его из GetSpawnInterval в SpawnData)
                e.currentCooldown = e.data.GetSpawnInterval();
            }
        }

        // Удаляем все завершившиеся события из основного списка
        foreach (RunningEventState e in toRemove)
        {
            runningEvents.Remove(e);
        }
    }
}