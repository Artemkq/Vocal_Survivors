public class DiamondPickup : Pickup
{
    PlayerCollector collector;
    public int diamods = 1;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // Retrieve the PlayerCollector component from the player who picked up this object
        // Add diamonds to their total.
        if (target != null)
        {
            collector = target.GetComponentInChildren<PlayerCollector>();
            if (collector != null) collector.AddDiamonds(diamods);
        }
    }
}
