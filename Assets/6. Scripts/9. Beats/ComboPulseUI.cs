using UnityEngine;
using TMPro;

public class ComboPulseUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("Настройки анимации")]
    public float punchScale = 1.2f;    // Насколько сильно увеличивается текст
    public float lerpSpeed = 10f;     // Скорость возврата к обычному размеру

    private void Start()
    {
        // Проверка на наличие менеджеров перед подпиской
        if (BeatConductor.Instance != null)
            BeatConductor.Instance.OnBeat += Pulse;

        if (ComboManager.Instance != null)
            ComboManager.Instance.OnComboChanged += UpdateText;

        // Скрываем текст, если комбо еще нет
        UpdateText(ComboManager.Instance.CurrentCombo, ComboManager.Instance.ComboMultiplier);
    }

    private void OnDestroy()
    {
        // Обязательно отписываемся при уничтожении объекта
        if (BeatConductor.Instance != null) BeatConductor.Instance.OnBeat -= Pulse;
        if (ComboManager.Instance != null) ComboManager.Instance.OnComboChanged -= UpdateText;
    }

    private void Update()
    {
        // Плавно возвращаем размер текста к обычному (1.0)
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * lerpSpeed);
    }

    private void UpdateText(int combo, float multiplier)
    {
        if (combo <= 0)
        {
            comboText.enabled = false;
            return;
        }

        comboText.enabled = true;
        comboText.text = $"x{combo}\n<size=60%>{multiplier:F1}x</size>";

        // При каждом НОВОМ комбо можно сделать дополнительный "всплеск"
        Pulse();
    }

    private void Pulse()
    {
        // Если комбо нет, не пульсируем
        if (ComboManager.Instance.CurrentCombo <= 0) return;

        // Увеличиваем масштаб (Update плавно вернет его к 1.0)
        transform.localScale = Vector3.one * punchScale;
    }
}
