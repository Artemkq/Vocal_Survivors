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
    public float speedBoostMultiplier = 4f; // Сила прыжка
    public float decelerationSpeed = 8f;    // Резкость остановки
    private float currentBeatSpeed;         // Текущая расчетная скорость

    protected override void Start()
    {
        base.Start();
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
        // Устанавливаем взрывную скорость в момент удара музыки
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

            // РАСЧЕТ ЗАТУХАНИЯ СКОРОСТИ
            // Плавно снижаем скорость от прыжка до нуля
            currentBeatSpeed = Mathf.Lerp(currentBeatSpeed, 0, Time.deltaTime * decelerationSpeed);

            Move(); // Вызываем движение
            HandleOutOffFrameAction();
        }
    }

    public virtual void Move()
    {
        if (agent != null && agent.enabled && player != null)
        {
            // ПРИМЕНЯЕМ НАШУ РИТМ-СКОРОСТЬ ВМЕСТО СТАТИЧНОЙ
            agent.speed = currentBeatSpeed;
            agent.SetDestination(player.position);
        }
        else if (rb && player != null)
        {
            // Для Rigidbody тоже применяем currentBeatSpeed
            rb.MovePosition(Vector2.MoveTowards(rb.position, player.position, currentBeatSpeed * Time.deltaTime));
        }
        else if (player != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, currentBeatSpeed * Time.deltaTime);
        }
    }

    protected virtual void OnDestroy()
    {
        // ОТПИСКА, чтобы не было ошибок при смерти врага
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat -= ApplyBeatImpulse;
        }
    }

    // --- ОСТАЛЬНЫЕ ТВОИ МЕТОДЫ БЕЗ ИЗМЕНЕНИЙ ---

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
