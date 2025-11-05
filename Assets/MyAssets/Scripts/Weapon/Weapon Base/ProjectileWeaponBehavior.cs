using UnityEngine;

/// <summary>
/// Base script of all projectile behaviours [To be placed on a prefab of a weapon that is a projectile]
/// </summary>

public class ProjectileWeaponBehavior : MonoBehaviour
{
    public WeaponScriptableObject weaponData;

    protected Vector3 direction;
    public float destroyAfterSeconds;

    //Current stats
    protected float currentDamage;
    protected float currentSpeed;
    protected float currentCooldownDuration;
    protected int currentPierce;

    void Awake()
    {
        currentDamage = weaponData.Damage;
        currentSpeed = weaponData.Speed;
        currentCooldownDuration = weaponData.CooldownDuration;
        currentPierce = weaponData.Pierce;
    }

    public float GetCurrentDamage()
    {
        return currentDamage *= FindAnyObjectByType<PlayerStats>().currentMight;
    }

    protected virtual void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }

    public void DirectionChecker(Vector3 dir)
    {
        direction = dir;

        float dirx = direction.x;
        float diry = direction.y;

        Vector3 scale = transform.localScale;
        Vector3 rotation = transform.rotation.eulerAngles;

        if (dirx < 0 && diry == 0) //Left
        {
            scale.x = scale.x * -1;
            scale.y = scale.y * -1;
        }
        
        else if (dirx == 0 && diry < 0) //Down
        {
            rotation.z = 0f;
        }

        else if (dirx == 0 && diry > 0) //Up
        {
            rotation.z = 180f;
        }

        else if (dir.x > 0 && dir.y > 0) //Right up
        {
            rotation.z = 135f;
        }

        else if (dir.x > 0 && dir.y < 0) //Right down
        {
            scale.y = scale.y * -1;
            rotation.z = -135f;
        }

        else if (dir.x < 0 && dir.y > 0) //Left up
        {
            rotation.z = -135f;
        }

        else if (dir.x < 0 && dir.y < 0) //Left down
        {
            scale.y = scale.y * -1;
            rotation.z = 135f;
        }

        transform.localScale = scale;
        transform.rotation = Quaternion.Euler(rotation); //Cant simply set the vector because cannot convert

    }

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        //Refference the script from the collided collider and deal damage using TakeDamage()
        if (col.CompareTag("Enemy"))
        {
            EnemyStats enemy = col.GetComponent<EnemyStats>();
            enemy.TakeDamage(GetCurrentDamage()); //Make sure to use currentDamage instead of weaponData.damage in case any damage multiplers in the future
            ReducePierce();
        }
        else if (col.CompareTag("Prop"))
        {
            if (col.gameObject.TryGetComponent(out BreakableProps breakable))
            {
                breakable.TakeDamage(GetCurrentDamage());
                ReducePierce();
            }
        }
    }

    void ReducePierce() //Destroy once the pierce reaches 0
    {
        currentPierce--;
        if (currentPierce <= 0)
        {
            Destroy(gameObject);
        }
    }

}
