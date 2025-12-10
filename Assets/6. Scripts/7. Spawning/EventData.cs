using UnityEngine;

public abstract class EventData : SpawnData
{
    [Header("Event Data")]

    [Tooltip("Время в МИНУТАХ (от 0 до 30), когда это событие должно быть активировано")]
    [Range(0f, 30f)] public int triggerMinutes;

    [Tooltip("Произойдет ли это событие?")]
    [Range(0f, 1f)] public float chance = 1f;

    [Tooltip("Насколько удача влияет на вероятность этого события")]
    [Range(0f, 1f)] public float luckFactor = 1f;

    [Tooltip("Если указано значение, это событие произойдёт только после того, как уровень проработает указанное количество секунд")]
    public float delay = 0;

    public abstract bool Activate(PlayerStats player = null);

    //Checks wheter this event is currently active
    public bool IsActive()
    {
        if (!GameManager.instance) return false;
        if (GameManager.instance.GetElapsedTime() > delay) return true;
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
