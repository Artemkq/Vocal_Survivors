using UnityEngine;
using Terresquall;

public class PlayerMovement : Sortable
{
    public const float DEFAULT_MOVESPEED = 2.5f;

    // --- НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ РИТМА ---
    [Header("Beat Movement")]
    public float dashSpeed = 8f;
    public float dashDistance = 2f; // Теперь используем дистанцию вместо абстрактной скорости
    public float movementDuration = 0.12f;

    private Vector2 fixedDashDir; // Зафиксированное направление рывка
    private float moveTimer;

    //Movement
    [HideInInspector] public float lastHorizontalVector;
    [HideInInspector] public float lastVerticalVector;
    [HideInInspector] public float lastHorizontalDirection = 1f;
    [HideInInspector] public Vector2 moveDir;
    [HideInInspector] public Vector2 lastMovedVector;

    //References
    Rigidbody2D rb;
    PlayerStats player;

    public Vector3 ProjectileSpawnPoint
    {
        get
        {
            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                return spriteRenderer.bounds.center;
            }
            return transform.position;
        }
    }

    protected override void Start()
    {
        base.Start();
        player = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        lastMovedVector = new Vector2(1, 0f);

        // --- ПОДПИСКА НА БИТ ---
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat += PerformDash;
        }
    }

    void Update()
    {
        InputManagement();
        if (moveTimer > 0) moveTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        Move();
    }

    // --- НОВЫЙ МЕТОД: ВЫЗЫВАЕТСЯ СТРОГО В БИТ ---
    void PerformDash()
    {
        // Обычный рывок в бит (стандартная дистанция)
        if (moveDir.sqrMagnitude > 0.01f && moveTimer <= 0)
        {
            StartFixedDash(moveDir, movementDuration, dashDistance);
        }
    }

    // Вспомогательный метод теперь принимает дистанцию
    void StartFixedDash(Vector2 direction, float duration, float distance)
    {
        if (moveTimer > 0) return; // Если уже в рывке — игнорируем
        StartCoroutine(DashRoutine(direction.normalized, duration, distance));
    }

    private System.Collections.IEnumerator DashRoutine(Vector2 direction, float duration, float distance)
    {
        moveTimer = duration;
        Vector2 startPos = rb.position;
        Vector2 endPos = startPos + direction * distance; // Используем переданную дистанцию
        float elapsed = 0f;

        rb.linearVelocity = Vector2.zero; // Убираем инерцию

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            rb.MovePosition(Vector2.Lerp(startPos, endPos, percent));
            yield return null;
        }

        rb.MovePosition(endPos);
        moveTimer = 0;
    }

    void InputManagement()
    {
        // Игнорируем ввод во время паузы или завершения игры
        if (GameManager.instance.isGameOver || GameManager.instance.isPaused)
        {
            moveDir = Vector2.zero;
            return;
        }

        float moveX, moveY;
        if (VirtualJoystick.CountActiveInstances() > 0)
        {
            moveX = VirtualJoystick.GetAxisRaw("Horizontal");
            moveY = VirtualJoystick.GetAxisRaw("Vertical");
        }
        else
        {
            moveX = Input.GetAxisRaw("Horizontal");
            moveY = Input.GetAxisRaw("Vertical");
        }

        // Обновляем moveDir только для логики "намерения" игрока
        moveDir = new Vector2(moveX, moveY).normalized;

        if (moveDir.x != 0)
        {
            lastHorizontalVector = moveDir.x;
            lastHorizontalDirection = moveDir.x;
            lastMovedVector = new Vector2(lastHorizontalVector, 0f);
        }

        if (moveDir.y != 0)
        {
            lastVerticalVector = moveDir.y;
            lastMovedVector = new Vector2(0f, lastVerticalVector);
        }

        if (moveDir.x != 0 && moveDir.y != 0)
        {
            lastMovedVector = new Vector2(lastHorizontalVector, lastVerticalVector);
        }

        if (Input.GetKeyDown(KeyCode.Space) && BeatConductor.Instance != null)
        {
            if (BeatConductor.Instance.IsInBeatWindow)
            {
                if (moveDir.sqrMagnitude > 0.001f && moveTimer <= 0)
                {
                    ComboManager.Instance?.AddCombo();
                    ScoreManager.Instance?.AddPoints();

                    // УДВОЕННАЯ ДИСТАНЦИЯ: dashDistance * 2f
                    StartFixedDash(moveDir, movementDuration * 1.2f, dashDistance * 2f);
                }
            }
            else
            {
                ComboManager.Instance?.ResetCombo();
            }
        }
    }

    // 3. Метод Move теперь отвечает ТОЛЬКО за остановку в паузе
    public virtual void Move()
    {
        if (GameManager.instance.isGameOver || GameManager.instance.isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Если рывка нет, скорость должна быть 0 (или обычный бег, если он есть)
        if (moveTimer <= 0)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // --- ОТПИСКА ДЛЯ ЧИСТОТЫ КОДА ---
    void OnDestroy()
    {
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat -= PerformDash;
        }
    }
}
