using UnityEngine;
using Terresquall;

public class PlayerMovement : Sortable

{
    public const float DEFAULT_MOVESPEED = 5f;

    //Movement
    [HideInInspector] public float lastHorizontalVector;
    [HideInInspector] public float lastVerticalVector;
    [HideInInspector] public Vector2 moveDir;
    [HideInInspector] public Vector2 lastMovedVector;

    //References
    Rigidbody2D rb;
    PlayerStats player;

    public Vector3 ProjectileSpawnPoint
    {
        get
        {
            // Если вы используете SpriteRenderer для отображения игрока, 
            // этот код найдет его и вернет центр его границ (bounds.center).
            // Это автоматически компенсирует смещенный пивот.
            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                return spriteRenderer.bounds.center;
            }

            // Запасной вариант: если SpriteRenderer не найден, вернуть обычную позицию объекта (пивот).
            return transform.position;
        }
    }

    protected override void Start()
    {
        base.Start();
        player = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        lastMovedVector = new Vector2(1, 0f); //If we dont do this and game starts up and the player doesnt move, the projectile weapon will have no momentum
    }

    void Update()
    {
        InputManagement();
    }

    void FixedUpdate()
    {
        Move();
    }

    void InputManagement()
    {
        if (GameManager.instance.isGameOver)
        {
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
        moveDir = new Vector2(moveX, moveY).normalized;


        if (moveDir.x != 0)
        {
            lastHorizontalVector = moveDir.x;
            lastMovedVector = new Vector2(lastHorizontalVector, 0f); //Last moved X
        }

        if (moveDir.y != 0)
        {
            lastVerticalVector = moveDir.y;
            lastMovedVector = new Vector2(0f, lastVerticalVector); //Last moved Y
        }

        if (moveDir.x != 0 && moveDir.y != 0)
        {
            lastMovedVector = new Vector2(lastHorizontalVector, lastVerticalVector);
        }
    }

    void Move()
    {
        if (GameManager.instance.isGameOver)
        {
            return;
        }

        rb.linearVelocity = moveDir * DEFAULT_MOVESPEED * player.Stats.moveSpeed;
    }
}
