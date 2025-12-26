using UnityEngine;
using System;

public class BeatConductor : MonoBehaviour
{
    public static BeatConductor Instance;

    [Header("Настройки музыки")]
    public AudioSource musicSource;

    [Header("Анализ барабанов")]
    public AudioSource drumsSource;
    [Range(0.01f, 1.0f)] public float threshold = 0.2f;
    public float cooldownBetweenBeats = 0.2f;

    [Header("Настройка сложности")]
    public float timingWindow = 0.5f;

    [Header("Отладка (Debug)")]
    public float currentRmsView;

    public bool IsInBeatWindow { get; private set; }
    public bool WasPressedThisWindow { get; private set; }
    public bool HasAttemptedThisBeat { get; private set; } // Флаг блокировки спама

    public event Action OnBeat;

    private float[] _samples = new float[256];
    private float _lastBeatTime;
    private float _beatTimer;

    void Awake() => Instance = this;

    void Update()
    {
        if (!musicSource.isPlaying) return;
        AnalyzeDrums();
    }

    private void AnalyzeDrums()
    {
        drumsSource.GetOutputData(_samples, 0);
        float sum = 0;
        for (int i = 0; i < _samples.Length; i++) sum += _samples[i] * _samples[i];
        float rmsValue = Mathf.Sqrt(sum / _samples.Length);
        currentRmsView = rmsValue;

        if (rmsValue > threshold && Time.time > _lastBeatTime + cooldownBetweenBeats)
        {
            TriggerBeat();
        }

        if (_beatTimer > 0)
        {
            _beatTimer -= Time.deltaTime;
            IsInBeatWindow = true;
        }
        else
        {
            IsInBeatWindow = false;
            // Сбрасываем всё, когда окно полностью закрылось
            WasPressedThisWindow = false;
            HasAttemptedThisBeat = false;
        }
    }

    private void TriggerBeat()
    {
        _lastBeatTime = Time.time;
        _beatTimer = timingWindow;

        // Сброс состояния для нового удара
        WasPressedThisWindow = false;
        HasAttemptedThisBeat = false;

        OnBeat?.Invoke();
    }

    // Централизованный метод регистрации нажатия
    public void RegisterPlayerTap()
    {
        // Если уже была попытка в этом окне — игнорируем
        if (HasAttemptedThisBeat) return;

        HasAttemptedThisBeat = true; // Сжигаем попытку

        if (IsInBeatWindow)
        {
            WasPressedThisWindow = true;
        }
    }
}
