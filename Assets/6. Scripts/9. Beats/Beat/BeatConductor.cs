using UnityEngine;
using System;

public class BeatConductor : MonoBehaviour
{
    public static BeatConductor Instance;

    [Header("Настройки музыки")]
    public AudioSource musicSource;

    [Header("Анализ барабанов")]
    public AudioSource drumsSource;
    [Range(0.01f, 1.0f)] public float threshold = 0.2f; // Порог громкости для удара
    public float cooldownBetweenBeats = 0.2f; // Минимальное время между ударами, чтобы не частило

    [Header("Настройка сложности")]
    public float timingWindow = 0.15f;

    [Header("Отладка (Debug)")]
    [Tooltip("Текущая громкость барабанов (смотрите сюда при запуске)")]
    public float currentRmsView;

    public bool IsInBeatWindow { get; private set; }
    public bool WasPressedThisWindow { get; private set; }

    public event Action OnBeat;

    private float[] _samples = new float[256]; // Объявлено в начале класса
    private float _lastBeatTime;
    private float _beatTimer; // Сколько времени еще открыто окно после удара

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!musicSource.isPlaying) return;

        AnalyzeDrums();
        HandleInput();
    }

    private void AnalyzeDrums()
    {
        // Анализируем работающую дорожку барабанов
        drumsSource.GetOutputData(_samples, 0);

        float sum = 0;
        for (int i = 0; i < _samples.Length; i++)
        {
            sum += _samples[i] * _samples[i];
        }

        float rmsValue = Mathf.Sqrt(sum / _samples.Length);
        currentRmsView = rmsValue;

        // Детектор удара
        if (rmsValue > threshold && Time.time > _lastBeatTime + cooldownBetweenBeats)
        {
            TriggerBeat();
        }

        // Окно нажатия
        if (_beatTimer > 0)
        {
            _beatTimer -= Time.deltaTime;
            IsInBeatWindow = true;
        }
        else
        {
            IsInBeatWindow = false;
            WasPressedThisWindow = false;
        }
    }

    private void TriggerBeat()
    {
        _lastBeatTime = Time.time;
        _beatTimer = timingWindow; // Открываем окно на заданное время

        OnBeat?.Invoke(); // Вызываем атаку оружия
        // Debug.Log("Drum Hit Detected!");
    }

    private void HandleInput()
    {
        if (IsInBeatWindow && Input.GetKeyDown(KeyCode.Space))
        {
            WasPressedThisWindow = true;
        }
    }
}
