using UnityEngine;

public class Pickup : MonoBehaviour, ICollectible
{
    protected bool hasBeenCollected = false;
    
    public virtual void Collect()
    {
        hasBeenCollected = true;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //If it gets too close to the player, destroy it. No need for any fance code
        if (col.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
