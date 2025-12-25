using UnityEngine;

public class PlayerMovement : Sortable
{
    public const float DEFAULT_MOVESPEED = 2.5f;

    [Header("Base Movement")]
    public float baseMoveSpeed = 3f; // Постоянная медленная скорость

    [Header("Beat Dash (Shift)")]
    public float dashDistance = 4f; // Дистанция рывка
    public float dashDuration = 0.15f; // Длительность рывка
    public float dashCooldown = 0.2f; // Защита от спама

    private float _moveTimer;
    private float _dashCooldownTimer;

    // References
    private Rigidbody2D rb;
    private PlayerStats player;

    [HideInInspector] public Vector2 moveDir;
    [HideInInspector] public Vector2 lastMovedVector;
    [HideInInspector] public float lastHorizontalVector;
    [HideInInspector] public float lastVerticalVector;
    [HideInInspector] public float lastHorizontalDirection = 1f;

    [Header("VFX")]
    public GameObject ghostPrefab;
    public float ghostSpawnInterval = 0.03f; // Как часто оставлять след

    public System.Action OnDashStarted;
    public System.Action OnDashFinished;

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

        // ОТПИСЫВАЕМСЯ от автоматического рывка, если он был
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat -= PerformDash;
        }
    }

    void Update()
    {
        InputManagement();

        if (_moveTimer > 0) _moveTimer -= Time.deltaTime;
        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Если мы НЕ в состоянии рывка, двигаемся обычно
        if (_moveTimer <= 0)
        {
            ApplyBaseMovement();
        }
    }

    void InputManagement()
    {
        if (GameManager.instance.isGameOver || GameManager.instance.isPaused)
        {
            moveDir = Vector2.zero;
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDir = new Vector2(moveX, moveY).normalized;

        if (moveX != 0)
        {
            lastHorizontalVector = moveX;
            // --- ВОЗВРАЩАЕМ ЭТУ СТРОКУ ---
            lastHorizontalDirection = moveX;
        }
        if (moveY != 0)
        {
            lastVerticalVector = moveY;
        }

        if (moveDir.sqrMagnitude > 0)
        {
            lastMovedVector = moveDir;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Space))
        {
            TryDash();
        }
    }

    void TryDash()
    {
        if (BeatConductor.Instance == null) return;

        // Проверяем: нажаты ли Space И Shift одновременно для "Blast Dash"
        bool isSpecialDash = Input.GetKey(KeyCode.LeftShift);

        if (BeatConductor.Instance.IsInBeatWindow && _moveTimer <= 0 && _dashCooldownTimer <= 0)
        {
            RhythmManager.Instance?.AddHit();

            // Оповещаем другие скрипты (например, BlastDash для звука LPF)
            OnDashStarted?.Invoke();

            StartCoroutine(DashRoutine(moveDir.sqrMagnitude > 0 ? moveDir : lastMovedVector, dashDuration, dashDistance));
            _dashCooldownTimer = dashCooldown;
        }
        else if (!BeatConductor.Instance.IsInBeatWindow)
        {
            // ПРОМАХ: Можно добавить визуальный эффект осечки или сброс комбо
            RhythmManager.Instance?.ResetCombo();
            // Debug.Log("Missed Beat Dash!");
        }
    }

    void ApplyBaseMovement()
    {
        if (GameManager.instance.isGameOver || GameManager.instance.isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Обычное передвижение из Vampire Survivors
        rb.linearVelocity = moveDir * baseMoveSpeed;
    }

    private System.Collections.IEnumerator DashRoutine(Vector2 direction, float duration, float distance)
    {
        _moveTimer = duration;
        Vector2 startPos = rb.position;
        Vector2 endPos = startPos + direction.normalized * distance;
        float elapsed = 0f;
        float lastGhostTime = 0f; // Таймер для призраков

        SpriteRenderer playerSR = GetComponentInChildren<SpriteRenderer>();

        rb.linearVelocity = Vector2.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            rb.MovePosition(Vector2.Lerp(startPos, endPos, percent));

            // СПАВН ПРИЗРАКА
            if (elapsed - lastGhostTime >= ghostSpawnInterval)
            {
                GameObject ghost = Instantiate(ghostPrefab);
                ghost.GetComponent<DashGhost>().Init(
                    playerSR.sprite,
                    transform.position,
                    transform.rotation,
                    playerSR.transform.localScale,
                    playerSR.flipX
                );
                lastGhostTime = elapsed;
            }

            yield return null;
        }

        rb.MovePosition(endPos);
        _moveTimer = 0;

        // В КОНЦЕ РЫВКА: Оповещаем систему приземления
        OnDashFinished?.Invoke();
    }

    // Метод оставлен пустым, чтобы не было ошибок подписки
    void PerformDash() { }
}
