// Отвечает за всю механику комбо ритма и взаимодействие с UI

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance;

    [Header("UI - Combo & Score")]
    [SerializeField] private TextMeshProUGUI totalPoints;
    [SerializeField] private TextMeshProUGUI multiplier;
    [SerializeField] private TextMeshProUGUI comboCount;
    [SerializeField] private Image progressBar;
    
    public KillComboManager otherPanelPuncher;

    [Header("Settings")]
    public int pointsPerHit = 50;
    public float punchScale = 1.05f;
    public float lerpSpeed = 8f;

    [Header("Colors")]
    public Color colorX1 = Color.white;
    public Color colorX2 = Color.yellow;
    public Color colorX3 = Color.green;
    public Color colorX4 = Color.magenta;

    public int CurrentCombo { get; private set; }
    public int CurrentMultiplier { get; private set; } = 1;
    public long CurrentScore { get; private set; }

    private bool _hasActedThisBeat;
    private Transform _transform;
    private Color _targetColor;

    // Кэш для строк, чтобы избежать создания мусора (GC)
    private static readonly string[] MultiplierStrings = { "x1", "x1", "x2", "x3", "x4" };

    void Awake()
    {
        Instance = this;
        _transform = transform; // Кэшируем трансформ
        _targetColor = colorX1;
    }

    void Start()
    {
        if (BeatConductor.Instance != null)
            BeatConductor.Instance.OnBeat += HandleBeatUpdate;

        UpdateUI();
    }

    void Update()
    {
        // Оптимизация: не вызываем Lerp, если размер уже близок к 1
        if (Mathf.Abs(_transform.localScale.x - 1f) > 0.001f)
        {
            _transform.localScale = Vector3.Lerp(_transform.localScale, Vector3.one, Time.deltaTime * lerpSpeed);
        }
    }

    public void AddHit()
    {
        _hasActedThisBeat = true;
        CurrentCombo++;

        // Быстрый расчет множителя без лишних условий
        int newMultiplier = CurrentCombo switch
        {
            >= 30 => 4,
            >= 20 => 3,
            >= 10 => 2,
            _ => 1
        };

        CurrentMultiplier = newMultiplier;
        CurrentScore += pointsPerHit * CurrentMultiplier;

        UpdateUI();
        Pulse();
    }

    public void ResetCombo()
    {
        if (CurrentCombo == 0) return;
        CurrentCombo = 0;
        CurrentMultiplier = 1;
        UpdateUI();
    }

    private void HandleBeatUpdate()
    {
        // Проверка на null и время один раз за бит
        var conductor = BeatConductor.Instance;
        if (conductor == null || conductor.musicSource.time < 0.5f) return;

        if (!_hasActedThisBeat && CurrentCombo > 0)
        {
            ResetCombo();
        }
        _hasActedThisBeat = false;
    }

    private void UpdateUI()
    {
        // 1. Очки: Форматируем только если они есть
        if (totalPoints != null)
            totalPoints.text = CurrentScore > 0 ? CurrentScore.ToString("N0") : string.Empty;

        // 2. Комбо: Если 0 — скрываем и выходим
        if (CurrentCombo <= 0)
        {
            multiplier.text = string.Empty;
            comboCount.text = string.Empty;
            if (progressBar) progressBar.fillAmount = 0;
            return;
        }

        // 3. Цвет: Выбираем цвет заранее
        _targetColor = CurrentMultiplier switch
        {
            2 => colorX2,
            3 => colorX3,
            4 => colorX4,
            _ => colorX1
        };

        // 4. Тексты: Используем кэшированные строки для множителя
        multiplier.text = MultiplierStrings[CurrentMultiplier];
        multiplier.color = _targetColor;

        // Используем конкатенацию только для динамического числа
        comboCount.text = CurrentCombo + " NOTES";

        // 5. Прогресс-бар
        if (progressBar != null)
        {
            progressBar.color = _targetColor;
            if (CurrentMultiplier >= 4)
            {
                progressBar.fillAmount = 1f;
            }
            else
            {
                // Упрощенная математика прогресса
                float progress = (CurrentCombo % 10) / 10f;
                progressBar.fillAmount = (progress == 0) ? 1f : progress;
            }
        }
    }

    private void Pulse()
    {
        _transform.localScale = Vector3.one * punchScale;
        if (otherPanelPuncher != null) otherPanelPuncher.Punch();
    }

    private void OnDestroy()
    {
        if (BeatConductor.Instance != null)
            BeatConductor.Instance.OnBeat -= HandleBeatUpdate;
    }
}
