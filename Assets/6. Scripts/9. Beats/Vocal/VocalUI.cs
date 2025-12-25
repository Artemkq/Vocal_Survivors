// Отвечает за визуальное отображение громкости голоса: ползунок и цвет

using UnityEngine;
using UnityEngine.UI;

public class VocalUI : MonoBehaviour
{
    public Slider vocalSlider;
    public Image fillImage;
    public Color lowColor = Color.blue;
    public Color highColor = Color.magenta;

    [Header("Настройка шкалы")]
    [Tooltip("При какой громкости шкала будет заполнена на 100%")]
    public float maxUIVolume = 1.5f;
    [Tooltip("Плавность движения ползунка")]
    public float lerpSpeed = 10f;

    void Update()
    {
        if (VocalAnalyzer.Instance == null) return;

        // Берем текущую громкость
        float volume = VocalAnalyzer.Instance.CurrentLoudness;

        // Нормализуем значение (0 - пусто, 1 - полно) на основе maxUIVolume
        float targetFill = Mathf.Clamp01(volume / maxUIVolume);

        // Плавно двигаем ползунок
        vocalSlider.value = Mathf.Lerp(vocalSlider.value, targetFill, Time.deltaTime * lerpSpeed);

        // Меняем цвет
        fillImage.color = Color.Lerp(lowColor, highColor, vocalSlider.value);
    }
}
