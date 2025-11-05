using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    
    public EnemyScriptableObject enemyData;

    //Current stats
    [HideInInspector] public float currentMoveSpeed;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentDamage;

    public float despawnDistance = 30f;
    Transform player;

    void Awake()
    {
        currentMoveSpeed = enemyData.MoveSpeed;
        currentHealth = enemyData.MaxHealth;
        currentDamage = enemyData.Damage;
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>().transform;
    }

    void Update()
    {
        if (Vector2.Distance(transform.position, player.position) >= despawnDistance)
        {
            ReturnEnemy();
        }
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;

        if (currentHealth <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        //Reference the script from the collider and deal damage using TakeDamage()
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerStats player = col.gameObject.GetComponent<PlayerStats>();
            player.TakeDamage(currentDamage); //Make sure to use currentDamage instead of weaponData.damage in case any damage multipliers in the future
        }
    }

    void OnDestroy()
    {
        EnemySpawner es = FindAnyObjectByType<EnemySpawner>();

        if (es != null)
        {
            es.OnEnemyKilled();
        }
        else
        {
            //Debug.LogWarning("ENEMY OBJECT IS NULL");
        }
    }

    void ReturnEnemy()
    {
        EnemySpawner es = FindAnyObjectByType<EnemySpawner>();
        transform.position = player.position + es.relativeSpawnPoints[Random.Range(0, es.relativeSpawnPoints.Count)].position;
    }
}
