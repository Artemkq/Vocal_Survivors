using UnityEngine;

public class HealthPoition : Pickup, ICollectible
{
    public int healthToRestore;

    public void Collect()
    {
        PlayerStats player = FindAnyObjectByType<PlayerStats>();
        player.RestoreHealth(healthToRestore);
    }
}
