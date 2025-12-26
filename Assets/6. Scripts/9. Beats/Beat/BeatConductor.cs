using UnityEngine;
using System;

public class BeatConductor : MonoBehaviour
{
    public static BeatConductor Instance;

    [Header("Настройки музыки")]
    public AudioSource musicSource;

    [Header("Анализ ритма (Kick/Bass)")]
    public AudioSource drumsSource;
    [Range(0.01f, 1.0f)] public float threshold = 0.15f;
    public float cooldownBetweenBeats = 0.25f;

    [Header("Настройка окна (секунды)")]
    [Tooltip("Рекомендуется 0.15 - 0.2 для четкого ритма")]
    public float timingWindow = 0.15f;

    public bool IsInBeatWindow { get; private set; }
    public bool WasPressedThisWindow { get; private set; }
    public bool HasAttemptedThisBeat { get; private set; }

    public event Action OnBeat;

    private float[] _samples = new float[256];
    private float _lastBeatTime;
    private float _beatTimer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (musicSource == null || !musicSource.isPlaying) return;

        AnalyzeDrums();
        UpdateWindows();
    }

    private void AnalyzeDrums()
    {
        // Используем dspTime для идеальной синхронизации со звуком
        float currentDspTime = (float)AudioSettings.dspTime;

        drumsSource.GetOutputData(_samples, 0);
        float sum = 0;
        // Оптимизация: проверяем только первые 64 сэмпла (обычно там низкие частоты)
        for (int i = 0; i < 64; i++) sum += _samples[i] * _samples[i];
        float rmsValue = Mathf.Sqrt(sum / 64);

        if (rmsValue > threshold && currentDspTime > _lastBeatTime + cooldownBetweenBeats)
        {
            _lastBeatTime = currentDspTime;
            TriggerBeat();
        }
    }

    private void UpdateWindows()
    {
        if (_beatTimer > 0)
        {
            _beatTimer -= Time.deltaTime;
            IsInBeatWindow = true;
        }
        else
        {
            IsInBeatWindow = false;
            WasPressedThisWindow = false;
            HasAttemptedThisBeat = false;
        }
    }

    private void TriggerBeat()
    {
        _beatTimer = timingWindow;
        WasPressedThisWindow = false;
        HasAttemptedThisBeat = false;

        // Рассылаем событие врагам
        OnBeat?.Invoke();
    }

    public void RegisterPlayerTap()
    {
        if (HasAttemptedThisBeat) return;
        HasAttemptedThisBeat = true;

        if (IsInBeatWindow)
        {
            WasPressedThisWindow = true;
            // Здесь можно добавить визуальный фидбек "Perfect!"
        }
    }
}
