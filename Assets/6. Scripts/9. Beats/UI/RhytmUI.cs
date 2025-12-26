using UnityEngine;
using UnityEngine.UI;

public class RhythmUI : MonoBehaviour
{
    [Header("Ссылки на элементы UI")]
    public Animator drumAnimator; // Ссылка на аниматор барабана и палочек
    public Image centerTarget;    // Сам барабан (для изменения цвета, если нужно)

    [Header("Полоски (Опционально)")]
    public RectTransform leftFillBar;
    public RectTransform rightFillBar;
    public float startXOffset = 200f;
    public float returnSpeed = 10f;

    private float _currentOffset;

    void Start()
    {
        // Подписываемся только если экземпляр существует
        if (BeatConductor.Instance != null)
        {
            // На всякий случай сначала отписываемся, чтобы не было дубликатов
            BeatConductor.Instance.OnBeat -= PlayDrumAnimation;
            BeatConductor.Instance.OnBeat += PlayDrumAnimation;
        }
    }

    void PlayDrumAnimation()
    {
        // 1. Запускаем анимацию удара палочками
        if (drumAnimator != null)
        {
            drumAnimator.SetTrigger("OnBeatHit");
        }

        // 2. Оставляем старый эффект разлета полосок (теперь это может быть "звуковая волна")
        _currentOffset = startXOffset;
    }

    void Update()
    {
        if (BeatConductor.Instance == null) return;

        // Плавный возврат полосок/палочек (если вы не используете аниматор для них)
        _currentOffset = Mathf.Lerp(_currentOffset, 0f, Time.deltaTime * returnSpeed);

        if (leftFillBar != null && rightFillBar != null)
        {
            leftFillBar.anchoredPosition = new Vector2(-_currentOffset, leftFillBar.anchoredPosition.y);
            rightFillBar.anchoredPosition = new Vector2(_currentOffset, rightFillBar.anchoredPosition.y);

            float alpha = _currentOffset / startXOffset;
            SetAlpha(leftFillBar.GetComponent<Image>(), alpha);
            SetAlpha(rightFillBar.GetComponent<Image>(), alpha);
        }

        // Центральный барабан теперь не меняет цвет, но мы можем добавить 
        // небольшую подсветку при успешном нажатии WasPressedThisWindow, если захотите позже.
    }

    void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    // Добавьте этот метод (вызывается при выключении объекта)
    private void OnDisable()
    {
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat -= PlayDrumAnimation;
        }
    }

    // Добавьте этот метод (вызывается при уничтожении объекта)
    private void OnDestroy()
    {
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat -= PlayDrumAnimation;
        }
    }
}
