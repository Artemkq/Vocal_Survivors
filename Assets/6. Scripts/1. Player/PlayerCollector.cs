using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D detector;

    [Header("Настройки сбора")]
    public float pullSpeed = 10f;

    // Событие для обновления UI
    public delegate void OnDiamondCollected();
    public OnDiamondCollected onDiamondCollected;

    private float diamonds;

    void Start()
    {
        player = GetComponentInParent<PlayerStats>();
        detector = GetComponent<CircleCollider2D>();
        diamonds = 0;
    }

    public void SetRadius(float r)
    {
        if (detector == null) detector = GetComponent<CircleCollider2D>();
        detector.radius = r;
    }

    public float GetDiamonds() => diamonds;

    public float AddDiamonds(float amount)
    {
        diamonds += amount;

        // Оповещаем UI, что количество алмазов изменилось
        onDiamondCollected?.Invoke();

        return diamonds;
    }

    // ВАЖНО: Вызывайте это только ПРИ ВЫХОДЕ в меню или в конце уровня!
    public void SaveDiamondsToStash()
    {
        // Прибавляем к общему сохранению в памяти
        SaveManager.LastLoadedGameData.diamonds += diamonds;
        diamonds = 0;

        // В 2025 году мы не сохраняем на диск посреди боя. 
        // Вызов SaveManager.Save() должен быть в GameManager при победе/поражении.
        Debug.Log("Алмазы добавлены в память, сохранение на диск произойдет в конце матча.");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // Быстрая проверка через слои или теги эффективнее, но TryGetComponent тоже пойдет
        if (col.CompareTag("Diamonds"))
        {
            if (col.TryGetComponent(out Pickup p))
            {
                p.Collect(player, pullSpeed);
            }
        }
    }
}
