using UnityEngine;

public abstract class EventData : SpawnData
{
    [Header("Event Data")]

    [Tooltip("Время в минутах (от 0 до 30), когда это событие должно быть активировано")]
    [Range(0f, 30f)] public float triggerMinutes;

    [Tooltip("Задержка в секундах с которой это событие произойдёт, а также частота спавна")]
    public float delayPlusSpawnInterval = 0;

    [Tooltip("Вероятность срабатывания этого события в процентах, где 0,1 = 10%")]
    [Range(0f, 1f)] public float chance = 1f;

    [Tooltip("Количество повторений. 0 означает бесконечное повторение")]
    [Min(0)] public int maxRepeats = 1;

    [Tooltip("Влияние удачи на вероятность этого события")]
    [Range(0f, 1f)] public float luckFactor = 1f;

    public abstract bool Activate(PlayerStats player = null);

    //Checks wheter this event is currently active
    public bool IsActive()
    {
        if (!GameManager.instance) return false;
        if (GameManager.instance.GetElapsedTime() > delayPlusSpawnInterval) return true;
        return false;
    }

    //Calculates a random probability of this event happening
    public bool CheckIfWillHappen(PlayerStats s)
    {
        //Probability of 1 means it always happens
        if (chance >= 1) return true;

        //Otherwise, get a random number and see if we pass the probability test
        if (chance / Mathf.Max(1, (s.Stats.luck * luckFactor)) >= Random.Range(0f, 1f))
            return true;

        return false;
    }
}
