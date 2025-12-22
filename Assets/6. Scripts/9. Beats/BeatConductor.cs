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

        // Рассчитываем текущую позицию в битах (например, 1.1, 1.2...)
        BeatPosition = musicSource.time / _secondsPerBeat;

        // Проверяем: наступил ли новый целый бит?
        if ((int)BeatPosition > _lastReportedBeat)
        {
            _lastReportedBeat = (int)BeatPosition;
            OnBeat?.Invoke(); // Сообщаем всем: "БИТ!"
        }

        // Проверяем: находится ли игрок сейчас в "окне допуска"
        float distanceFromBeat = Mathf.Abs(BeatPosition - Mathf.Round(BeatPosition)) * _secondsPerBeat;
        IsInBeatWindow = distanceFromBeat <= timingWindow;
    }
}
