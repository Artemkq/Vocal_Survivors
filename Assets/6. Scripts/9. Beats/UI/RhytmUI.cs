using UnityEngine;
using UnityEngine.UI;

public class RhythmUI : MonoBehaviour
{
    [Header("Ссылки на элементы")]
    public Image centerTarget;    // Объект в центре
    public RectTransform beatRing; // Кольцо, которое сжимается

    [Header("Настройки")]
    public float startScale = 3f;  // Начальный размер кольца
    public Color hitColor = Color.green; // Цвет в "окне"
    public Color missColor = Color.white; // Цвет вне "окна"

    private Vector3 _initialRingScale;

    void Start()
    {
        _initialRingScale = beatRing.localScale;
    }

    void Update()
    {
        if (BeatConductor.Instance == null) return;

        // 1. Плавное сжатие кольца к биту
        // BeatPosition идет например так: 1.1, 1.2, 1.3... 
        // Мы берем только дробную часть: 0.1, 0.2...
        float fraction = BeatConductor.Instance.BeatPosition % 1f;

        // Инвертируем, чтобы кольцо уменьшалось к 1.0 (к моменту удара)
        float currentScale = Mathf.Lerp(startScale, 1f, fraction);
        beatRing.localScale = _initialRingScale * currentScale;

        // 2. Подсветка центрального индикатора
        if (BeatConductor.Instance.IsInBeatWindow)
        {
            centerTarget.color = hitColor;
            // Делаем центр чуть больше в окне бита
            centerTarget.transform.localScale = Vector3.one * 1.2f;
        }
        else
        {
            centerTarget.color = missColor;
            centerTarget.transform.localScale = Vector3.one;
        }

        // 3. Прозрачность кольца (оно исчезает сразу после бита и появляется снова)
        Color ringCol = beatRing.GetComponent<Image>().color;
        ringCol.a = Mathf.Lerp(0.1f, 0.8f, fraction);
        beatRing.GetComponent<Image>().color = ringCol;
    }
}
