using System;
using System.Collections;
using UnityEngine;

[SelectionBase]

public class Player : MonoBehaviour
{

    public static Player Instance { get; private set; }
    public event EventHandler OnPlayerDeath;
    public event EventHandler OnFlashBlink;

    [Header("Player Settings")]
    [SerializeField] private float movingSpeed = 10f;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float damageRecoveryTime = 0.5f;
    
    [Header("Dash Settings")]    
    [SerializeField] private int dashSpeed = 3;
    [SerializeField] private float dashTime = 0.1f;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float dashCooldownTime = 0.25f;

    Vector2 inputVector;

    private Rigidbody2D _rb;
    private KnockBack _knockBack;

    private readonly float _minMovingSpeed = 0.1f;
    private bool _isRunning = false;

    private int _currentHealth;
    private bool _canTakeDamage;
    private bool _isAlive;
    private bool _isDashing;
    private float _initialMovingSpeed;
    

    private Camera _mainCamera;

    private void Awake()
    {
        Instance = this;
        _rb = GetComponent<Rigidbody2D>();
        _knockBack = GetComponent<KnockBack>();

        _mainCamera = Camera.main;
        _initialMovingSpeed = movingSpeed;
    }

    private void Start()
    {
        _currentHealth = maxHealth;
        _canTakeDamage = true;
        _isAlive = true;
        GameInput.Instance.OnPlayerAttack += GameInput_OnPlayerAttack;
        GameInput.Instance.OnPlayerDash += GameInput_OnPlayerDash;
    }

    private void Update()
    {
        inputVector = GameInput.Instance.GetMovementVector();
    }

    private void FixedUpdate()
    {
        if (_knockBack.IsGettingKnockedBack)
            return;

        HandleMovement();
    }

    public bool IsAlive() => _isAlive;

    public void TakeDamage(Transform damageSource, int damage)
    {
        if (_canTakeDamage && _isAlive)
        {
            _canTakeDamage = false;
            _currentHealth = Mathf.Max(0, _currentHealth -= damage);
            _knockBack.GetKnockedBack(damageSource);

            OnFlashBlink?.Invoke(this, EventArgs.Empty);

            StartCoroutine(DamageRecoveryRoutine());

        }

        DetectDeath();

    }

    private void DetectDeath()
    {
        if (_currentHealth == 0 && _isAlive )
        {
            _isAlive = false;
            _knockBack.StopKnockBackMovement();
            GameInput.Instance.DisableMovement();
            OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPlayerDash(object sender, EventArgs e)
    {
       //Dash();
    }

    private void Dash()
    {
        if (!_isDashing)
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        //  орутина выполн€ет все до Yeild команды
        _isDashing = true;
        movingSpeed *= dashSpeed;
        trailRenderer.emitting = true;

        yield return new WaitForSeconds(dashTime);

        // ј после Yeild выполн€ет все остальное спуст€ врем€ указанное в WaitForSec
        trailRenderer.emitting = false;
        movingSpeed = _initialMovingSpeed;

        yield return new WaitForSeconds(dashCooldownTime);
        _isDashing = false;

    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(damageRecoveryTime);
        _canTakeDamage = true;
    }


    public bool IsRunning()
    {
        return _isRunning;
    }

    public Vector3 GetPlayerScreenPosition()
    {
        Vector3 playerScreenPosition = _mainCamera.WorldToScreenPoint(transform.position);
        return playerScreenPosition;
    }

    private void GameInput_OnPlayerAttack(object sender, System.EventArgs e)
    {
        ActiveWeapon.Instance.GetActiveWeapon().Attack();
    }

    private void HandleMovement()
    {

        _rb.MovePosition(_rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime));

        if (Mathf.Abs(inputVector.x) > _minMovingSpeed || Mathf.Abs(inputVector.y) > _minMovingSpeed)
        {
            _isRunning = true;
        }
        else
        {
            _isRunning = false;
        }
    }

    private void OnDestroy()
    {
        GameInput.Instance.OnPlayerAttack -= GameInput_OnPlayerAttack;
    }


}
