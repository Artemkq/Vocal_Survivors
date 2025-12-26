using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Base Movement")]
    public float baseMoveSpeed = 3f;

    [Header("Beat Dash (Shift)")]
    public float dashDistance = 4f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.2f;

    private float _moveTimer;
    private float _dashCooldownTimer;

    private Rigidbody2D rb;
    private PlayerStats player;

    [HideInInspector] public Vector2 moveDir;
    [HideInInspector] public Vector2 lastMovedVector;
    [HideInInspector] public float lastHorizontalDirection = 1f;

    public System.Action OnDashStarted;
    public System.Action OnDashFinished;

    [Header("VFX")]
    public GameObject ghostPrefab;
    public float ghostSpawnInterval = 0.03f;

    // Добавьте эти переменные, которые запрашивают ваши другие скрипты
    [HideInInspector] public float lastHorizontalVector;
    [HideInInspector] public float lastVerticalVector;

    void Start()
    {
        player = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        lastMovedVector = Vector2.right;

        if (BeatConductor.Instance != null)
            BeatConductor.Instance.OnBeat -= PerformDash;
    }

    void Update()
    {
        InputManagement();
        if (_moveTimer > 0) _moveTimer -= Time.deltaTime;
        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (_moveTimer <= 0) ApplyBaseMovement();
    }

    // Добавьте это свойство для спавна снарядов (ошибки в ProjectileWeapon и SwordSlashWeapon)
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

    void InputManagement()
    {
        if (GameManager.instance.isGameOver || GameManager.instance.isPaused) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDir = new Vector2(moveX, moveY).normalized;

        // ОБНОВЛЕНИЕ: Заполняем данные для аниматора и оружия
        if (moveX != 0)
        {
            lastHorizontalDirection = moveX;
            lastHorizontalVector = moveX; // Для PlayerAnimator
        }
        if (moveY != 0)
        {
            lastVerticalVector = moveY; // Для PlayerAnimator
        }

        if (moveDir.sqrMagnitude > 0) lastMovedVector = moveDir;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool alreadyAttempted = BeatConductor.Instance.HasAttemptedThisBeat;
            BeatConductor.Instance.RegisterPlayerTap();

            if (!alreadyAttempted)
            {
                TryDash();
            }
        }
    }

    void TryDash()
    {
        // Теперь рывок срабатывает только если кондуктор подтвердил окно
        if (BeatConductor.Instance.WasPressedThisWindow && _moveTimer <= 0 && _dashCooldownTimer <= 0)
        {
            RhythmManager.Instance?.AddHit();
            OnDashStarted?.Invoke();
            StartCoroutine(DashRoutine(moveDir.sqrMagnitude > 0 ? moveDir : lastMovedVector, dashDuration, dashDistance));
            _dashCooldownTimer = dashCooldown;
        }
        else if (!BeatConductor.Instance.IsInBeatWindow)
        {
            RhythmManager.Instance?.ResetCombo();
        }
    }

    void ApplyBaseMovement()
    {
        if (GameManager.instance.isGameOver || GameManager.instance.isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = moveDir * baseMoveSpeed;
    }

    private IEnumerator DashRoutine(Vector2 direction, float duration, float distance)
    {
        _moveTimer = duration;
        Vector2 startPos = rb.position;
        Vector2 endPos = startPos + direction.normalized * distance;
        float elapsed = 0f;
        float lastGhostTime = 0f;
        SpriteRenderer playerSR = GetComponentInChildren<SpriteRenderer>();

        rb.linearVelocity = Vector2.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(startPos, endPos, elapsed / duration));

            if (elapsed - lastGhostTime >= ghostSpawnInterval)
            {
                GameObject ghost = Instantiate(ghostPrefab);
                ghost.GetComponent<DashGhost>().Init(playerSR.sprite, transform.position, transform.rotation, playerSR.transform.localScale, playerSR.flipX);
                lastGhostTime = elapsed;
            }
            yield return null;
        }

        rb.MovePosition(endPos);
        _moveTimer = 0;
        OnDashFinished?.Invoke();
    }

    void PerformDash() { }
}
