// Отвечает за всю механику комбо убийств и взаимодействие с UI

using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class KillComboManager : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI totalKillsText;
    public TextMeshProUGUI alertText;
    public Image timerBar;

    [Header("Настройки логики")]
    public float timeLimit = 3f;
    private float _currentTimer;
    private int _killCount = 0;
    private int _totalKillCount = 0;
    private int _cycleCount = 0;

    [Header("Визуальный эффект (Punch)")]
    public float punchScale = 1.1f;
    public float lerpSpeed = 8f;
    private Vector3 _originalScale;
    private Transform _transform;

    [Header("Ресурсы")]
    public AudioClip[] milestoneSounds;
    public string[] milestoneTexts = { "KILLING SPREE", "RAMPAGE", "MULTI KILL", "ULTRA KILL", "MONSTER KILL", "HOLY SHIT", "GOD LIKE" };
    public Color[] cycleColors;

    private AudioSource _audioSource;

    void Awake()
    {
        _transform = transform;
        _originalScale = _transform.localScale;
        _audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (alertText != null) alertText.gameObject.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        // Возвращение размера (эффект пульсации)
        if (Mathf.Abs(_transform.localScale.x - _originalScale.x) > 0.001f)
        {
            _transform.localScale = Vector3.Lerp(_transform.localScale, _originalScale, Time.deltaTime * lerpSpeed);
        }

        // Логика таймера убийств
        if (_killCount > 0)
        {
            _currentTimer -= Time.deltaTime;
            if (timerBar != null) timerBar.fillAmount = _currentTimer / timeLimit;
            if (_currentTimer <= 0) ResetCombo();
        }
    }

    // Метод, который вызывается извне при убийстве
    public void OnEnemyKilled()
    {
        _killCount++;
        _totalKillCount++;
        _currentTimer = timeLimit;

        if (_killCount % 100 == 0 && _killCount > 0) TriggerMilestone();

        UpdateUI();
    }

    // ВАЖНО: Этот метод будет вызываться из RhythmManager.cs
    // через поле public UIPuncher otherPanelPuncher;
    // так как сигнатура метода совпадает (Punch)
    public void Punch()
    {
        _transform.localScale = _originalScale * punchScale;
    }

    private void TriggerMilestone()
    {
        _cycleCount++;

        // ПРОВЕРКА: Если цикл больше 7, выходим из метода, 
        // чтобы не проигрывать звуки и не менять текст.
        if (_cycleCount > 7) return;

        int index = _cycleCount - 1; // Теперь Clamp не нужен, так как выше есть проверка

        // Звук
        if (index < milestoneSounds.Length && milestoneSounds[index] != null)
            _audioSource.PlayOneShot(milestoneSounds[index]);

        // Текст алертов
        if (alertText != null)
        {
            alertText.gameObject.SetActive(true);
            alertText.text = milestoneTexts[index];

            if (index < cycleColors.Length)
            {
                Color c = cycleColors[index];
                alertText.color = c;
                if (timerBar != null) timerBar.color = c;
            }
        }
    }

    private void UpdateUI()
    {
        bool hasActiveCombo = _killCount > 0;
        if (comboText) { comboText.gameObject.SetActive(hasActiveCombo); comboText.text = _killCount.ToString(); }
        if (timerBar) timerBar.gameObject.SetActive(hasActiveCombo);
        if (totalKillsText) { totalKillsText.gameObject.SetActive(_totalKillCount > 0); totalKillsText.text = _totalKillCount.ToString(); }
    }

    private void ResetCombo()
    {
        _killCount = 0;
        _cycleCount = 0;
        if (alertText) alertText.gameObject.SetActive(false);
        if (timerBar) { timerBar.color = Color.white; timerBar.fillAmount = 0; }
        UpdateUI();
    }
}
