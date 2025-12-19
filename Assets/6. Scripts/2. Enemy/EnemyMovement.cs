using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Добавляем using для NavMeshAgent

public class EnemyMovement : Sortable
{
    protected EnemyStats stats;
    protected Transform player;
    protected Rigidbody2D rb;
    protected NavMeshAgent agent; // Добавляем ссылку на NavMeshAgent

    protected Vector2 knockbackVelocity;
    protected float knockbackDuration;

    public enum OutOffFrameAction { none, respawnAtEdge, despawn }
    public OutOffFrameAction outOffFrameAction = OutOffFrameAction.respawnAtEdge;

    [System.Flags]
    public enum KnockbackVariance { duration = 1, velocity = 2 }
    public KnockbackVariance knockbackVariance = KnockbackVariance.velocity;

    protected bool spawnedOutOffFrame = false;
    [HideInInspector] public bool giveExperienceOnDeath = true;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>(); // Пытаемся получить компонент
        spawnedOutOffFrame = !WaveManager.IsWithinBoundaries(transform);
        stats = GetComponent<EnemyStats>();

        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        if (allPlayers.Length > 0)
        {
            player = allPlayers[Random.Range(0, allPlayers.Length)].transform;
        }

        // --- ФИКС ПРОБЛЕМЫ ПОВОРОТА NAVMESHAGENT ---
        if (agent != null)
        {
            // Запрещаем агенту управлять вращением (это убирает поворот на -90 по X)
            agent.updateRotation = false;
            agent.updateUpAxis = false;

            // Убеждаемся, что спрайт смотрит на камеру (сброс поворотов X и Y)
            transform.rotation = Quaternion.identity;
        }
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
                // Помогаем агенту понять, где он оказался после пинка
                agent.Warp(transform.position);
            }

            // Вызываем Move (внутри него agent.speed = stats.Actual.moveSpeed)
            Move();
            HandleOutOffFrameAction();
        }
    }

    //If the enemy falls outside of the frame, handle it
    protected virtual void HandleOutOffFrameAction()
    {
        //Handle the enemy when it is out of frame
        if (!WaveManager.IsWithinBoundaries(transform))
        {
            switch (outOffFrameAction)
            {
                case OutOffFrameAction.none:
                default:
                    break;
                case OutOffFrameAction.respawnAtEdge:
                    //If the enemy is outside the camera frame, teleport it back to the edge of the frame
                    transform.position = WaveManager.GeneratePosition();
                    break;
                case OutOffFrameAction.despawn:
                    //Dont destroy if it is spawned outside the frame
                    if (!spawnedOutOffFrame)
                    {
                        // --- ИЗМЕНЕНО ---
                        // Вызываем наш новый метод деспавна без опыта
                        Despawn();
                    }
                    break;
            }
        }
        else spawnedOutOffFrame = false;
    }

    // Используется RingEventData и HandleOutOffFrameAction для удаления врага без опыта
    // Этот метод только ЗАПУСКАЕТ процесс деспавна по таймеру.
    public void Despawn(float delay = 3.0f)
    {
        // Мы НЕ устанавливаем giveExperienceOnDeath = false ЗДЕСЬ. 
        // Если игрок убьет врага до истечения таймера, опыт выпадет.

        // Запускаем корутину с задержкой, используя переданное значение 'delay'
        StartCoroutine(DelayedKill(delay));
    }

    IEnumerator DelayedKill(float delay)
    {
        yield return new WaitForSeconds(delay);

        // *** ВОТ ГДЕ МЫ УСТАНАВЛИВАЕМ ФЛАГ ***
        // Если объект дожил до этого момента (не был убит игроком), 
        // значит, он умирает от истечения lifespan'а, и опыт давать не нужно.
        if (this != null && gameObject != null)
        {
            giveExperienceOnDeath = false; // <-- Перемещено сюда

            // Повторно проверяем, что объект еще существует и вызываем Kill через EnemyStats
            GetComponent<EnemyStats>()?.Kill();
        }
    }

    //This is meant to be called from other scripts to create knockback
    public virtual void Knockback(Vector2 velocity, float duration)
    {
        //Ignore the knockback if the duration is greater than 0
        if (knockbackDuration > 0) return;

        //Ignore knockback if the knockback type is set to nont
        if (knockbackVariance == 0) return;

        //Only change the factor is the multiplier is not 0 or 1
        float pow = 1;
        bool reducesVelocity = (knockbackVariance & KnockbackVariance.velocity) > 0,
             reducesDuration = (knockbackVariance & KnockbackVariance.duration) > 0;

        if (reducesVelocity && reducesDuration) pow = 0.5f;

        //Check which knockback values to affect
        knockbackVelocity = velocity * (reducesVelocity ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
        knockbackDuration = duration * (reducesDuration ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
    }

    public virtual void Move()
    {
        if (agent != null && agent.enabled && player != null)
        {
            // 1. Всегда берем скорость из Stats. 
            // Теперь не важно, что написано в инспекторе агента, 
            // скрипт EnemyStats будет главным источником.
            agent.speed = stats.Actual.moveSpeed;

            // 2. Устанавливаем цель
            agent.SetDestination(player.position);
        }
        // --- РЕЗЕРВНЫЙ ВАРИАНТ С RIGIDBODY (как было раньше) ---
        else if (rb)
        {
            rb.MovePosition(Vector2.MoveTowards
                (
                rb.position,
                player.transform.position,
                stats.Actual.moveSpeed * Time.deltaTime
                )
            );
        }
        else if (player != null)
        {
            // Движение без Rigidbody
            transform.position = Vector2.MoveTowards
                (
                transform.position,
                player.transform.position,
                stats.Actual.moveSpeed * Time.deltaTime
                );
        }
    }
}
