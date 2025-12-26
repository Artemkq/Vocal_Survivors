using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    protected EnemyStats stats;
    protected Transform player;
    protected Rigidbody2D rb;
    protected NavMeshAgent agent;

    protected Vector2 knockbackVelocity;
    protected float knockbackDuration;

    public enum OutOffFrameAction { none, respawnAtEdge, despawn }
    public OutOffFrameAction outOffFrameAction = OutOffFrameAction.respawnAtEdge;

    protected bool spawnedOutOffFrame = false;
    [HideInInspector] public bool giveExperienceOnDeath = true;

    [Header("Beat Settings")]
    public float speedBoostMultiplier = 4f;
    public float decelerationSpeed = 8f;
    private float currentBeatSpeed;

    protected SpriteRenderer sr;
    protected SpriteRenderer shadowSr;

    // --- ОПТИМИЗАЦИЯ: Кэшируем хеши имен для поиска тени ---
    private static readonly string SHADOW_NAME = "Shadow";

    void Awake()
    {
        // Переносим инициализацию в Awake для стабильности пулинга
        rb = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<EnemyStats>();
        sr = GetComponentInChildren<SpriteRenderer>();

        Transform shadowT = transform.Find(SHADOW_NAME);
        if (shadowT != null) shadowSr = shadowT.GetComponent<SpriteRenderer>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
    }

    void OnEnable()
    {
        // ОПТИМИЗАЦИЯ: Ищем игрока через GameManager (он уже закэширован там)
        if (GameManager.instance != null)
        {
            player = GameManager.instance.GetRandomPlayerTransform();
        }

        // Подписываемся на бит только при включении
        if (BeatConductor.Instance != null) BeatConductor.Instance.OnBeat += ApplyBeatImpulse;

        spawnedOutOffFrame = !WaveManager.IsWithinBoundaries(transform);

        giveExperienceOnDeath = true; // Сбрасываем флаг для нового появления
        
        // Также сбрасываем active в DropRateManager
        if (TryGetComponent(out DropRateManager drm)) drm.active = true;
    }

    void OnDisable()
    {
        // ОБЯЗАТЕЛЬНО: Отписываемся при выключении, иначе будет утечка памяти и лаги!
        if (BeatConductor.Instance != null) BeatConductor.Instance.OnBeat -= ApplyBeatImpulse;
        StopAllCoroutines();
    }

    // МЕТОД ИЗ ОШИБКИ CS1061: Теперь EnemyStats сможет его вызвать
    public void UpdateSpeed(float newSpeed)
    {
        // В BEAT HELL базовая скорость обновляется здесь, а текущая — в Move()
        if (stats != null) currentBeatSpeed = newSpeed;
    }

    protected virtual void ApplyBeatImpulse()
    {
        if (stats != null)
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
                // Warp — дорогая операция. Вызываем только если сдвинулись далеко.
                if (Vector3.Distance(agent.nextPosition, transform.position) > 0.1f)
                    agent.Warp(transform.position);
            }

            // Если рывок слишком слабый, уменьшите decelerationSpeed (например, до 4 или 5)
            float baseSpeed = (stats != null) ? stats.Actual.moveSpeed : 1f;
            currentBeatSpeed = Mathf.Lerp(currentBeatSpeed, baseSpeed, Time.deltaTime * decelerationSpeed);

            Move();

            // ОПТИМИЗАЦИЯ: Проверяем границы не каждый кадр, а раз в 10 кадров
            if (Time.frameCount % 10 == 0) HandleOutOffFrameAction();
        }
    }

    public virtual void Move()
    {
        if (player == null) return;

        // Поворот спрайта (делаем это реже или только при смене направления)
        bool playerIsRight = (player.position.x - transform.position.x) > 0;
        if (sr != null) sr.flipX = !playerIsRight;
        if (shadowSr != null) shadowSr.flipX = !playerIsRight;

        // Движение
        if (agent != null && agent.enabled)
        {
            agent.speed = currentBeatSpeed;
            // ОПТИМИЗАЦИЯ: Не пересчитываем путь каждый кадр! 
            // NavMesh очень тяжелый. Обновляем цель только если игрок отошел.
            if (Time.frameCount % 20 == 0)
                agent.SetDestination(player.position);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, currentBeatSpeed * Time.deltaTime);
        }
    }

    public virtual void Knockback(Vector2 velocity, float duration)
    {
        if (knockbackDuration > 0 || stats == null) return;

        float multiplier = stats.Actual.knockbackMultiplier;
        knockbackVelocity = velocity * multiplier;
        knockbackDuration = duration * multiplier;
    }

    protected virtual void HandleOutOffFrameAction()
    {
        if (!WaveManager.IsWithinBoundaries(transform))
        {
            if (outOffFrameAction == OutOffFrameAction.respawnAtEdge)
                transform.position = WaveManager.GeneratePosition();
            else if (outOffFrameAction == OutOffFrameAction.despawn && !spawnedOutOffFrame)
                gameObject.SetActive(false); // ПУЛИНГ: вместо Despawn()
        }
        else spawnedOutOffFrame = false;
    }

    public void Despawn(float delay = 0f)
    {
        if (delay > 0)
        {
            // Если передано время, запускаем корутину выключения через паузу
            StartCoroutine(DelayedDespawn(delay));
        }
        else
        {
            // Если время 0, выключаем мгновенно
            gameObject.SetActive(false);
        }
    }

    // Добавьте эту вспомогательную корутину:
    private IEnumerator DelayedDespawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
