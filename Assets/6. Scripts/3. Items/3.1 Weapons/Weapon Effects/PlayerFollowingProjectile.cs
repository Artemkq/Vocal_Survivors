using UnityEngine;

/// <summary>
/// Проектайл, который следует за позицией игрока во время анимации
/// Используется для ударов типа Sword Slash
/// </summary>
public class PlayerFollowingProjectile : Projectile
{
    private Transform playerTransform;
    private Vector3 spawnOffset; // Сохраняем смещение от позиции игрока при спавне

    protected override void Start()
    {
        // Получаем трансформ игрока
        playerTransform = owner.transform;
        
        // Сохраняем смещение удара от позиции игрока в момент спавна
        if (playerTransform != null)
        {
            spawnOffset = transform.position - playerTransform.position;
        }

        // Отключаем физику для кинематического движения
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }

        //Prevent the area from being 0, as if hides the projectile
        float area = weapon.GetArea();
        if (area <= 0) area = 1;

        transform.localScale = new Vector3(
            area * Mathf.Sign(transform.localScale.x),
            area * Mathf.Sign(transform.localScale.y), 1
        );

        //Set how much piercing this object has
        Weapon.Stats stats = weapon.GetStats();
        piercing = stats.pierce;

        //Destroy the projectile after its lifespan expires
        if (stats.duration > 0) Destroy(gameObject, stats.duration);

        //If the projectile is auto-aiming, automatically find a suitable enemy
        if (hasAutoAim) AcquireAutoAimFacing();
    }

    protected override void FixedUpdate()
    {
        // Если игрок существует, следуем за ним с сохраненным смещением
        if (playerTransform != null)
        {
            transform.position = playerTransform.position + spawnOffset;

            // Вращаем удар вместе с движением игрока
            Weapon.Stats stats = weapon.GetStats();
            if (rotationSpeed.z != 0)
            {
                transform.Rotate(rotationSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Если игрок исчез, уничтожаем удар
            Destroy(gameObject);
        }
    }
}
