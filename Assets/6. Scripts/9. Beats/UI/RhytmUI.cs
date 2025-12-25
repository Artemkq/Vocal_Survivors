using UnityEngine;
using UnityEngine.UI;

public class RhythmUI : MonoBehaviour
{
    [Header("Ссылки на элементы UI")]
    public RectTransform leftFillBar;
    public RectTransform rightFillBar;
    public Image centerTarget;    // Объект в центре (ваша уникальная иконка)

    [Header("Настройки")]
    [Tooltip("Начальное смещение полос от центра (например, 200px)")]
    public float startXOffset = 600f;
    public Color hitColor = Color.green; // Цвет в "окне"
    public Color missColor = Color.white; // Цвет вне "окна"

    // Переменная для хранения текущей позиции в доле бита (от 0.0 до 1.0)
    private float _currentFraction;

    void Update()
    {
        if (BeatConductor.Instance == null) return;

        // Расчет позиции в такте (от 0.0 до 1.0)
        _currentFraction = BeatConductor.Instance.BeatPosition % 1f;

        // 1. Движение полос от края к центру
        // Lerp от начального смещения (startOffset) до 0 (центра)
        float currentXPosition = Mathf.Lerp(startXOffset, 0f, _currentFraction);

        // Устанавливаем позиции. Левая полоса двигается в минус по X, правая в плюс по X.
        // При _currentFraction = 0f, они на startXOffset. При _currentFraction = 1f, они в центре (0f).
        leftFillBar.anchoredPosition = new Vector2(-currentXPosition, leftFillBar.anchoredPosition.y);
        rightFillBar.anchoredPosition = new Vector2(currentXPosition, rightFillBar.anchoredPosition.y);

        // 2. Плавное исчезание при приближении к центру
        // Мы хотим, чтобы они исчезали в конце цикла (когда _currentFraction близко к 1.0)
        // Используем степень (_currentFraction^2) для более резкого исчезания в самом конце
        float alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(_currentFraction, 2));

        Color leftColor = leftFillBar.GetComponent<Image>().color;
        leftColor.a = alpha;
        leftFillBar.GetComponent<Image>().color = leftColor;

        Color rightColor = rightFillBar.GetComponent<Image>().color;
        rightColor.a = alpha;
        rightFillBar.GetComponent<Image>().color = rightColor;

        // 3. Подсветка центрального индикатора
        if (BeatConductor.Instance.IsInBeatWindow)
        {
            centerTarget.color = hitColor;        }
        else
        {
            centerTarget.color = missColor;
        }
    }
}
