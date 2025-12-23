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
        // Если игрок зажал направление к моменту бита
        if (moveDir.sqrMagnitude > 0.01f)
        {
            StartFixedDash(moveDir, movementDuration);
        }
    }

    // Вспомогательный метод для запуска рывка
    void StartFixedDash(Vector2 direction, float duration)
    {
        fixedDashDir = direction.normalized;
        moveTimer = duration;
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

        // НОВЫЙ КОД ДЛЯ КОМБО (Пробел)
        if (Input.GetKeyDown(KeyCode.Space) && BeatConductor.Instance != null)
        {
            if (BeatConductor.Instance.IsInBeatWindow)
            {
                ComboManager.Instance?.AddCombo();
                // Рывок по пробелу теперь тоже фиксирует текущее направление ввода
                Vector2 dashInput = moveDir.sqrMagnitude > 0.01f ? moveDir : (Vector2)lastMovedVector;
                StartFixedDash(dashInput, movementDuration * 1.2f);
            }
            else
            {
                ComboManager.Instance?.ResetCombo();
            }
        }
    }

    public virtual void Move()
    {
        if (GameManager.instance.isGameOver || GameManager.instance.isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (moveTimer > 0)
        {
            // ИСПОЛЬЗУЕМ fixedDashDir вместо moveDir
            // Это гарантирует, что даже если игрок отпустит кнопки в середине рывка,
            // персонаж долетит ровно по вектору, который был в начале.
            rb.linearVelocity = fixedDashDir * dashSpeed * player.Stats.moveSpeed;
        }
        else
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
