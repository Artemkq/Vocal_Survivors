using UnityEngine;

public class HealthPoition : Pickup
{
    public int healthToRestore;

    public override void Collect()
    {
        if (hasBeenCollected)
        {
            return;
        }
        else
        {
            base.Collect();
        }
        
        PlayerStats player = FindAnyObjectByType<PlayerStats>();
        player.RestoreHealth(healthToRestore);
    }
}
