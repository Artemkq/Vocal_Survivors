using UnityEngine;
using UnityEngine.UI;

public class RhythmUI : MonoBehaviour
{
    [Header("Ссылки на элементы UI")]
    public RectTransform leftFillBar;
    public RectTransform rightFillBar;
    public Image centerTarget;

    [Header("Настройки")]
    public float startXOffset = 200f; // На сколько разлетаются полоски
    public float returnSpeed = 10f;   // Скорость возвращения к центру
    public Color hitColor = Color.red;
    public Color missColor = Color.white;

    private float _currentOffset;

    void Start()
    {
        // Подписываемся на событие удара барабана
        if (BeatConductor.Instance != null)
            BeatConductor.Instance.OnBeat += FlashUI;
    }

    void FlashUI()
    {
        // В момент удара выталкиваем полоски на максимум
        _currentOffset = startXOffset;
    }

    void Update()
    {
        if (BeatConductor.Instance == null) return;

        // 1. Плавно возвращаем полоски к центру (0)
        _currentOffset = Mathf.Lerp(_currentOffset, 0f, Time.deltaTime * returnSpeed);

        // Устанавливаем позиции
        leftFillBar.anchoredPosition = new Vector2(-_currentOffset, leftFillBar.anchoredPosition.y);
        rightFillBar.anchoredPosition = new Vector2(_currentOffset, rightFillBar.anchoredPosition.y);

        // 2. Прозрачность зависит от удаления (чем дальше от центра, тем прозрачнее)
        float alpha = _currentOffset / startXOffset;
        SetAlpha(leftFillBar.GetComponent<Image>(), alpha);
        SetAlpha(rightFillBar.GetComponent<Image>(), alpha);

        // 3. Подсветка центра (попадание в бит)
        // Если игрок нажал пробел в окне — красим в красный (hitColor)
        if (BeatConductor.Instance.WasPressedThisWindow)
        {
            centerTarget.color = hitColor;
        }
        else
        {
            centerTarget.color = missColor;
        }
    }

    void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    private void OnDestroy()
    {
        if (BeatConductor.Instance != null)
            BeatConductor.Instance.OnBeat -= FlashUI;
    }
}
