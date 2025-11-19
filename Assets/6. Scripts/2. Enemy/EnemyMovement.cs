using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    protected EnemyStats enemy;
    protected Transform player;

    protected Vector2 knockbackVelocity;
    protected float knockbackDuration;

    public enum OutOffFrameAction { none, respawnAtEdge, despawn }
    public OutOffFrameAction outOffFrameAction = OutOffFrameAction.respawnAtEdge;

    protected bool spawnedOutOffFrame = false;

    protected virtual void Start()
    {
        spawnedOutOffFrame = !SpawnManager.IsWithinBoundaries(transform);
        enemy = GetComponent<EnemyStats>();

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
                case OutOffFrameAction.none: default:
                    break;
                case OutOffFrameAction.respawnAtEdge:
                    //If the enemy is outside the camera frame, teleport it back to the edge of the frame
                    transform.position = SpawnManager.GeneratePosition();
                    break;
                case OutOffFrameAction.despawn:
                    //Dont destroy if it is spawned outside the frame
                    if (!spawnedOutOffFrame)
                    {
                        Destroy(gameObject);
                    }
                    break;
            }
        }
        else spawnedOutOffFrame = false;
    }

    //This is meant to be called from other scripts to create knockback
    public virtual void Knockback(Vector2 velocity, float duration)
    {
        //Ignore the knockback if the duration is greater than 0
        if (knockbackDuration > 0) return;

        //Begins the knockback
        knockbackVelocity = velocity;
        knockbackDuration = duration;
    }

    public virtual void Move()
    {
        //Constantly move the enemy towards the player
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, enemy.currentMoveSpeed  * Time.deltaTime);
    }
}
