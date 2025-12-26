using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Base Movement")]
    public float baseMoveSpeed = 5f;

    [Header("Beat Dash")]
    public float dashForce = 20f; // Перешли на силу для плавности
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.2f;

    private float _moveTimer;
    private float _dashCooldownTimer;

    private Rigidbody2D rb;
    private PlayerStats player;
    private SpriteRenderer sr;

    [HideInInspector] public Vector2 moveDir;
    [HideInInspector] public Vector2 lastMovedVector;
    [HideInInspector] public float lastHorizontalVector;
    [HideInInspector] public float lastVerticalVector;
    [HideInInspector] public float lastHorizontalDirection = 1f; // Добавьте эту строку


    [Header("VFX (Object Pooling Recommended)")]
    public GameObject ghostPrefab;
    public float ghostSpawnInterval = 0.05f; // Увеличили интервал для экономии ресурсов

    void Start()
    {
        player = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        lastMovedVector = Vector2.right;

        // ВАЖНО: Убедитесь, что в Rigidbody2D включена Interpolation!
    }

    void Update()
    {
        if (GameManager.instance.isGameOver || GameManager.instance.isPaused) return;

        InputManagement();

        if (_moveTimer > 0) _moveTimer -= Time.deltaTime;
        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (_moveTimer <= 0) ApplyBaseMovement();
    }

    void InputManagement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDir = new Vector2(moveX, moveY).normalized;

        if (moveX != 0)
        {
            lastHorizontalVector = moveX;
            lastHorizontalDirection = moveX; // Добавьте эту строку здесь
        }

        if (moveY != 0) lastVerticalVector = moveY;
        if (moveDir.sqrMagnitude > 0) lastMovedVector = moveDir;

        // Рывок на Пробел (в бит)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (BeatConductor.Instance != null)
            {
                bool alreadyAttempted = BeatConductor.Instance.HasAttemptedThisBeat;
                BeatConductor.Instance.RegisterPlayerTap();

                if (!alreadyAttempted) TryDash();
            }
        }
    }

    void TryDash()
    {
        if (BeatConductor.Instance.WasPressedThisWindow && _moveTimer <= 0 && _dashCooldownTimer <= 0)
        {
            RhythmManager.Instance?.AddHit();
            StartCoroutine(DashRoutine(moveDir.sqrMagnitude > 0 ? moveDir : lastMovedVector));
            _dashCooldownTimer = dashCooldown;
        }
        else if (!BeatConductor.Instance.IsInBeatWindow)
        {
            RhythmManager.Instance?.ResetCombo();
        }
    }

    void ApplyBaseMovement()
    {
        rb.linearVelocity = moveDir * baseMoveSpeed;
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        _moveTimer = dashDuration;
        float elapsed = 0f;
        float lastGhostTime = 0f;

        // Используем физическую скорость вместо MovePosition для плавности
        rb.linearVelocity = direction.normalized * (dashForce);

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;

            // Спавн призраков (ВНИМАНИЕ: тут все еще Instantiate, нужно будет заменить на пул!)
            if (elapsed - lastGhostTime >= ghostSpawnInterval && ghostPrefab != null)
            {
                GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
                if (ghost.TryGetComponent(out DashGhost dg))
                {
                    dg.Init(sr.sprite, transform.position, transform.rotation, sr.transform.localScale, sr.flipX);
                }
                lastGhostTime = elapsed;
            }
            yield return null;
        }

        rb.linearVelocity = Vector2.zero; // Резкая остановка после рывка
        _moveTimer = 0;
    }

    // Точка спавна снарядов для оружия
    public Vector3 ProjectileSpawnPoint => sr != null ? sr.bounds.center : transform.position;
}
