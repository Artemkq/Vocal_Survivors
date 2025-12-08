using System.Collections; // ДОБАВИТЬ ЭТУ СТРОКУ
using UnityEngine;

public class EnemyMovement : Sortable
{
    protected EnemyStats stats;
    protected Transform player;
    protected Rigidbody2D rb; //For checking if enemy has a rigidbody

    protected Vector2 knockbackVelocity;
    protected float knockbackDuration;

    public enum OutOffFrameAction { none, respawnAtEdge, despawn }
    public OutOffFrameAction outOffFrameAction = OutOffFrameAction.respawnAtEdge;

    [System.Flags]
    public enum KnockbackVariance { duration = 1, velocity = 2 }
    public KnockbackVariance knockbackVariance = KnockbackVariance.velocity;

    protected bool spawnedOutOffFrame = false;
    // ДОБАВИТЬ ЭТУ СТРОКУ:
    [HideInInspector] public bool giveExperienceOnDeath = true;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        spawnedOutOffFrame = !SpawnManager.IsWithinBoundaries(transform);
        stats = GetComponent<EnemyStats>();

        //Picks a random player on the screen, instead of always picking the 1st player
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        player = allPlayers[Random.Range(0, allPlayers.Length)].transform;
    }


    protected virtual void Update()
    {
        //If we are currently being knocked back, then process the knockback.
        if (knockbackDuration > 0)
        {
            transform.position += (Vector3)knockbackVelocity * Time.deltaTime;
            knockbackDuration -= Time.deltaTime;
        }
        else
        {
            Move();
            HandleOutOffFrameAction();
        }
    }

    //If the enemy falls outside of the frame, handle it
    protected virtual void HandleOutOffFrameAction()
    {
        //Handle the enemy when it is out of frame
        if (!SpawnManager.IsWithinBoundaries(transform))
        {
            switch (outOffFrameAction)
            {
                case OutOffFrameAction.none:
                default:
                    break;
                case OutOffFrameAction.respawnAtEdge:
                    //If the enemy is outside the camera frame, teleport it back to the edge of the frame
                    transform.position = SpawnManager.GeneratePosition();
                    break;
                case OutOffFrameAction.despawn:
                    //Dont destroy if it is spawned outside the frame
                    if (!spawnedOutOffFrame)
                    {
                        // ИЗМЕНЕНИЕ: Вызываем новую функцию Despawn() вместо Destroy(gameObject)
                        Despawn();
                    }
                    break;
            }
        }
        else spawnedOutOffFrame = false;
    }

    // ДОБАВИТЬ ЭТИ ДВА НОВЫХ МЕТОДА:

    public void Despawn()
    {
        // Устанавливаем флаг, что опыт давать не нужно при этом убийстве
        giveExperienceOnDeath = false;

        // Запускаем корутину с задержкой в 3 секунды
        StartCoroutine(DelayedKill(3.0f));
    }

    IEnumerator DelayedKill(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Повторно проверяем, что объект еще существует и вызываем Kill через EnemyStats
        if (this != null && gameObject != null)
        {
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
        //If there is a rigidbody, use it to move instead of moving the position directly
        //This optimises performance
        if (rb)
        {
            rb.MovePosition(Vector2.MoveTowards
                (
                rb.position,
                player.transform.position,
                stats.Actual.moveSpeed * Time.deltaTime
                )
            );
        }
        else
        {
            //Constatnly move the enemy towards the player
            transform.position = Vector2.MoveTowards
                (
                transform.position,
                player.transform.position,
                stats.Actual.moveSpeed * Time.deltaTime
                );
        }
    }
}
