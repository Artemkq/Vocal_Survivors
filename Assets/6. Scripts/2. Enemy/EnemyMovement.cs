using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : Sortable
{
    protected EnemyStats stats;
    protected Transform player;
    protected Rigidbody2D rb;
    protected NavMeshAgent agent;

    protected Vector2 knockbackVelocity;
    protected float knockbackDuration;

    public enum OutOffFrameAction { none, respawnAtEdge, despawn }
    public OutOffFrameAction outOffFrameAction = OutOffFrameAction.respawnAtEdge;

    [System.Flags]
    public enum KnockbackVariance { duration = 1, velocity = 2 }
    public KnockbackVariance knockbackVariance = KnockbackVariance.velocity;

    protected bool spawnedOutOffFrame = false;
    [HideInInspector] public bool giveExperienceOnDeath = true;

    // --- НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ РИТМА ---
    [Header("Beat Settings")]
    [Tooltip("Во сколько раз скорость увеличится в момент бита")]
    public float speedBoostMultiplier = 4f;
    [Tooltip("Как быстро скорость возвращается от рывка к обычной (чем выше, тем резче)")]
    public float decelerationSpeed = 8f;
    private float currentBeatSpeed;

    protected SpriteRenderer sr; // Ссылка на компонент отрисовки
    protected SpriteRenderer shadowSr; // Ссылка на компонент отрисовки тени

    protected override void Start()
    {
        base.Start();
        sr = GetComponentInChildren<SpriteRenderer>(); // Ищем спрайт внутри объекта

        // Ищем спрайт тени только на дочернем объекте с именем "Shadow"
        Transform shadowT = transform.Find("Shadow");
        if (shadowT != null)
        {
            shadowSr = shadowT.GetComponent<SpriteRenderer>();
        }

        rb = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();
        spawnedOutOffFrame = !WaveManager.IsWithinBoundaries(transform);
        stats = GetComponent<EnemyStats>();

        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        if (allPlayers.Length > 0)
        {
            player = allPlayers[Random.Range(0, allPlayers.Length)].transform;
        }

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            transform.rotation = Quaternion.identity;
        }

        // ПОДПИСКА НА БИТ
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat += ApplyBeatImpulse;
        }
    }

    // МЕТОД РЫВКА
    protected virtual void ApplyBeatImpulse()
    {
        // В момент бита скорость подскакивает до максимума
        currentBeatSpeed = stats.Actual.moveSpeed * speedBoostMultiplier;
    }

    protected virtual void Update()
    {
        if (knockbackDuration > 0)
        {
            if (agent != null) agent.enabled = false;
            transform.position += (Vector3)knockbackVelocity * Time.deltaTime;
            knockbackDuration -= Time.deltaTime;
        }
        else
        {
            if (agent != null && !agent.enabled)
            {
                agent.enabled = true;
                agent.Warp(transform.position);
            }

            // --- ИЗМЕНЕНО: РАСЧЕТ ЗАТУХАНИЯ СКОРОСТИ ---
            // Теперь скорость плавно падает не до 0, а до stats.Actual.moveSpeed (базовой скорости)
            currentBeatSpeed = Mathf.Lerp(currentBeatSpeed, stats.Actual.moveSpeed, Time.deltaTime * decelerationSpeed);

            Move();
            HandleOutOffFrameAction();
        }
    }

    public virtual void Move()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        bool playerIsRight = direction.x > 0;

        // --- ЛОГИКА ПОВОРОТА БЕЗ ИСКАЖЕНИЙ ---
        if (sr != null)
        {
            // Поворачиваем основной спрайт
            sr.flipX = !playerIsRight;
        }

        if (shadowSr != null)
        {
            // Поворачиваем тень так же, как и основной спрайт
            shadowSr.flipX = sr.flipX;
        }

        // --- ДВИЖЕНИЕ ---
        // ... (весь ваш код движения через agent, rb или transform.position)
        if (agent != null && agent.enabled)
        {
            agent.speed = currentBeatSpeed;
            agent.SetDestination(player.position);
        }
        else if (rb)
        {
            Vector2 targetPos = Vector2.MoveTowards(rb.position, player.position, currentBeatSpeed * Time.deltaTime);
            rb.MovePosition(targetPos);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, currentBeatSpeed * Time.deltaTime);
        }
    }

    protected virtual void OnDestroy()
    {
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat -= ApplyBeatImpulse;
        }
    }

    // Остальные методы (Despawn, Knockback и т.д.) остаются без изменений...
    public void Despawn(float delay = 3.0f)
    {
        StartCoroutine(DelayedKill(delay));
    }

    IEnumerator DelayedKill(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (this != null && gameObject != null)
        {
            giveExperienceOnDeath = false;
            GetComponent<EnemyStats>()?.Kill();
        }
    }

    public virtual void Knockback(Vector2 velocity, float duration)
    {
        if (knockbackDuration > 0) return;
        if (knockbackVariance == 0) return;
        float pow = 1;
        bool reducesVelocity = (knockbackVariance & KnockbackVariance.velocity) > 0,
             reducesDuration = (knockbackVariance & KnockbackVariance.duration) > 0;
        if (reducesVelocity && reducesDuration) pow = 0.5f;
        knockbackVelocity = velocity * (reducesVelocity ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
        knockbackDuration = duration * (reducesDuration ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
    }

    protected virtual void HandleOutOffFrameAction()
    {
        if (!WaveManager.IsWithinBoundaries(transform))
        {
            switch (outOffFrameAction)
            {
                case OutOffFrameAction.respawnAtEdge:
                    transform.position = WaveManager.GeneratePosition();
                    break;
                case OutOffFrameAction.despawn:
                    if (!spawnedOutOffFrame) Despawn();
                    break;
            }
        }
        else spawnedOutOffFrame = false;
    }
}
