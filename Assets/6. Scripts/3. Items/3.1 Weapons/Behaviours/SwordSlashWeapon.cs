using UnityEngine;

public class SwordSlashWeapon : ProjectileWeapon
{
    int currentSpawnCount; //How many times the whip has been attacking in this iteration
    float currentSpawnYOffset; //If there are more that 2 sword slash, we will start offsetting it upwards

    protected override bool Attack(int attackCount = 1)
    {
        //If no projectile prefab is assigned, leave a warning message
        if (!currentStats.projectilePrefab)
        {
            Debug.LogWarning(string.Format("Projectile prefab has not been set for {0}, name"));
            ActivateCooldown(true);
            return false;
        }

        //If there is no projectile assigned, set the weapon on cooldown
        if (!CanAttack()) return false;

        // Если это первый вызов атаки, сбрасываем счетчик
        if (currentCooldown <= 0)
        {
            currentSpawnCount = 0;
            currentSpawnYOffset = 0f;
        }

        //Otherwise, calculate the angle and offset of our spawned projectile
        //Then, if <currentSpawnCount> is even (i.e. more than 1 projectile,
        //we flip the direction of the spawn
        float spawnDir = Mathf.Sign(movement.lastMovedVector.x) * (currentSpawnCount % 2 != 0 ? -1 : 1);
        Vector2 spawnOffset = new Vector2(
            spawnDir * Random.Range(currentStats.spawnVariance.xMin, currentStats.spawnVariance.xMax),
            currentSpawnYOffset
        );

        // --- ИЗМЕНЕНИЕ ЗДЕСЬ ---
        // Используем базовую позицию спавна (центр спрайта) из PlayerMovement
        Vector3 spawnBasePosition = movement.ProjectileSpawnPoint;
        Vector3 finalSpawnPosition = spawnBasePosition + (Vector3)spawnOffset;

        // And spawn a copy of the projectile
        Projectile prefab = Instantiate(
            currentStats.projectilePrefab,
            finalSpawnPosition, // Используем новую, скорректированную позицию
            Quaternion.identity
        );

        prefab.owner = owner; //Set ourselves to be the owner

        //Flip the projectile sprite
        if (spawnDir < 0)
        {
            prefab.transform.localScale = new Vector3(
                -Mathf.Abs(prefab.transform.localScale.x),
                prefab.transform.localScale.y,
                prefab.transform.localScale.z
            );
            Debug.Log(spawnDir + " | " + prefab.transform.localScale);
        }

        //Assign the stats
        prefab.weapon = this;
        ActivateCooldown(true);
        attackCount--;

        //Determine where the next projectile should spawn
        currentSpawnCount++;
        if (currentSpawnCount > 1 && currentSpawnCount % 2 == 0)
            currentSpawnYOffset += 1;

        //Do we perform another attack?
        if (attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = ((WeaponData)data).baseStats.projectileInterval;
        }

        return true;
    }
}
