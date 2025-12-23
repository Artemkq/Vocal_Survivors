using UnityEngine;
using TMPro;

public class ComboUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboText;

    private void Start() // Используем Start вместо OnEnable
    {
        // Проверяем наличие Instance, чтобы избежать NullReference
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.OnComboChanged += UpdateComboDisplay;
            // Сразу обновляем текст текущими значениями
            UpdateComboDisplay(ComboManager.Instance.CurrentCombo, ComboManager.Instance.ComboMultiplier);
        }
        else
        {
            Debug.LogError("ComboUI: ComboManager.Instance не найден! Проверьте, есть ли ComboManager на сцене.");
        }
    }

    private void OnDestroy() // Используем OnDestroy для отписки
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.OnComboChanged -= UpdateComboDisplay;
        }
    }

    private void UpdateComboDisplay(int comboCount, float multiplier)
    {
        if (comboText == null) return;

        if (comboCount <= 0)
        {
            comboText.gameObject.SetActive(false);
            return;
        }

        comboText.gameObject.SetActive(true);
        // Форматирование: x5 COMBO (с новой строки множитель)
        comboText.text = $"x{comboCount} COMBO\n<size=80%>{multiplier:F1}x Damage</size>";
    }
}
