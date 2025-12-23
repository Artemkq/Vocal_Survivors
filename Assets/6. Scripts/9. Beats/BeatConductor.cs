using UnityEngine;
using System;

public class BeatConductor : MonoBehaviour
{
    public static BeatConductor Instance;

    [Header("Настройки музыки")]
    public float bpm = 120f;
    public AudioSource musicSource;

    [Header("Настройка сложности")]
    [Tooltip("Размер окна (в секундах), когда нажатие засчитывается")]
    public float timingWindow = 0.12f;

    public float BeatPosition { get; private set; }
    public bool IsInBeatWindow { get; private set; }

    // Это "событие", на которое будут подписываться другие объекты
    public event Action OnBeat;

    private float _secondsPerBeat;
    private int _lastReportedBeat;

    void Awake()
    {
        Instance = this;
        _secondsPerBeat = 60f / bpm;
    }

    void Update()
    {
        if (!musicSource.isPlaying) return;

        // Рассчитываем текущую позицию в битах
        float currentBeatPosition = musicSource.time / _secondsPerBeat;

        // ПРОВЕРКА НА ЛУП (LOOP):
        // Если текущий бит стал меньше предыдущего зафиксированного, 
        // значит музыка началась заново.
        if (currentBeatPosition < _lastReportedBeat)
        {
            _lastReportedBeat = -1; // Сбрасываем счетчик для нового круга
        }

        BeatPosition = currentBeatPosition;

        // Проверяем: наступил ли новый целый бит?
        if ((int)BeatPosition > _lastReportedBeat)
        {
            _lastReportedBeat = (int)BeatPosition;
            OnBeat?.Invoke();
        }

        // Окно допуска
        float distanceFromBeat = Mathf.Abs(BeatPosition - Mathf.Round(BeatPosition)) * _secondsPerBeat;
        IsInBeatWindow = distanceFromBeat <= timingWindow;
    }
}
