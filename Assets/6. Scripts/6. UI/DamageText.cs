using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshProUGUI tmPro;
    private RectTransform rect;
    private float timer;
    private float duration = 0.8f;
    private Vector3 worldPosition;
    private Camera cam;

    void Awake()
    {
        tmPro = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
    }

    public void Setup(string text, Vector3 spawnPos, Camera referenceCamera)
    {
        tmPro.text = text;
        cam = referenceCamera;
        timer = 0;

        // Сначала устанавливаем позицию в мировых координатах
        worldPosition = spawnPos + new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0f);

        // СРАЗУ пересчитываем позицию на экране, чтобы не было "прыжка" в первом кадре
        rect.position = cam.WorldToScreenPoint(worldPosition);

        // И только теперь включаем
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (timer >= duration)
        {
            gameObject.SetActive(false);
            return;
        }

        timer += Time.deltaTime;
        float alpha = 1 - (timer / duration);

        // Оптимизация: меняем альфу только если она сильно изменилась
        Color c = tmPro.color;
        c.a = alpha;
        tmPro.color = c;

        // Двигаем виртуальную точку и пересчитываем позицию
        worldPosition += Vector3.up * (Time.deltaTime * 1.5f);
        rect.position = cam.WorldToScreenPoint(worldPosition);
    }
}
