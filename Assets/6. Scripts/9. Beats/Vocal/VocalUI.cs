using UnityEngine;
using UnityEngine.UI;

public class VocalUI : MonoBehaviour
{
    public Slider vocalSlider;
    public Image fillImage;
    public Color lowColor = Color.blue;
    public Color highColor = Color.magenta;

    void Update()
    {
        if (VocalAnalyzer.Instance == null) return;

        float volume = VocalAnalyzer.Instance.CurrentLoudness;

        // Обновляем ползунок (нормализуем значение от 0 до 1)
        vocalSlider.value = Mathf.Lerp(vocalSlider.value, volume / 2f, Time.deltaTime * 10f);

        // Меняем цвет от синего к кислотно-розовому при крике
        fillImage.color = Color.Lerp(lowColor, highColor, vocalSlider.value);
    }
}
