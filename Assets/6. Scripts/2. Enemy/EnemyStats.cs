using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SpriteRenderer))]

public class EnemyStats : MonoBehaviour
{
    
    public EnemyScriptableObject enemyData;

    //Current stats
    [HideInInspector] public float currentMoveSpeed;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentDamage;

    public float despawnDistance = 30f;
    Transform player;

    [Header("Damage Feedback")]
    public Color damageColor = new Color(1, 0, 0, 1); //What the color of the damage flash should be.
    public float damageFlashDuration = 0.2f; //How long the flash should last.
    public float deathFadeTime = 0.6f; //How much time it takes for the enemy to fade.
    Color originalColor;
    SpriteRenderer sr;
    EnemyMovement movement;

    void Awake()
    {
        currentMoveSpeed = enemyData.MoveSpeed;
        currentHealth = enemyData.MaxHealth;
        currentDamage = enemyData.Damage;
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>().transform;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        movement = GetComponent<EnemyMovement>();
    }

    void Update()
    {
        if (Vector2.Distance(transform.position, player.position) >= despawnDistance)
        {
            ReturnEnemy();
        }
    }

    //This function always needs at least 2 values, the amount of damage dealt <dmg>, as well as where the damage is
    //coming from, which is passed as <sourcePosition>. The <sourcePosition> is necessary because it is used to calculate
    //the direction of the knockback

    public void TakeDamage(float dmg, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        currentHealth -= dmg;
        StartCoroutine(DamageFlash());

        //Create the text popup when enemy takes damage
        if (dmg > 0)
            GameManager.GenerateFloatingText(Mathf.FloorToInt(dmg).ToString(), transform);

        //Aply knockback if it is not zero
        if (knockbackForce > 0)
        {
            //Gets the direction of knockback
            Vector2 dir = (Vector2)transform.position - sourcePosition;
            movement.Knockback(dir.normalized * knockbackForce, knockbackDuration);
        }

        //Kills the enemy if the health drops below zero
        if (currentHealth <= 0)
        {
            Kill();
        }
    }

    //This is a Coroutine function that makes the enemy flash when taking damage
    IEnumerator DamageFlash()
    {
        sr.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration);
        sr.color = originalColor;
    }

    public void Kill()
    {
        StartCoroutine(KillFade());
    }

    //This is a Coroutine function that fades the enemy away slowly
    IEnumerator KillFade()
    {
        //Waits for a single frame
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0, origAlpha = sr.color.a;

        //This is a loop that fires evety frame
        while (t < deathFadeTime)
        {
            yield return w;
            t += Time.deltaTime;

            //Set the color for this frame
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, (1 - t / deathFadeTime) * origAlpha);
        }
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
            Debug.LogWarning("ENEMY OBJECT IS NULL");
        }
    }

    void ReturnEnemy()
    {
        EnemySpawner es = FindAnyObjectByType<EnemySpawner>();
        transform.position = player.position + es.relativeSpawnPoints[Random.Range(0, es.relativeSpawnPoints.Count)].position;
    }
}
