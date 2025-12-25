using UnityEngine;

public class DashGhost : MonoBehaviour
{
    private SpriteRenderer sr;
    private float alpha;
    private float fadeSpeed = 3f; // Скорость исчезновения

    public void Init(Sprite senderSprite, Vector3 position, Quaternion rotation, Vector3 scale, bool flipX)
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = senderSprite;
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
        sr.flipX = flipX;

        alpha = 0.6f; // Начальная прозрачность (0.6 - полупрозрачный)
        sr.color = new Color(0.5f, 0.8f, 1f, alpha); // Голубоватый "неоновый" оттенок
    }

    void Update()
    {
        alpha -= Time.deltaTime * fadeSpeed;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

        if (alpha <= 0)
        {
            Destroy(gameObject); // В будущем заменим на Pooling для оптимизации
        }
    }
}
