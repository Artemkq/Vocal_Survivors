using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D playerCollector;
    public float pullSpeed;

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>();
        playerCollector = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        playerCollector.radius = player.CurrentMagnet;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //Check if the other game object has the ICollectible interface
        if (col.gameObject.TryGetComponent(out ICollectible collectible))
        {
            //Pulling animation
            
            //Gets the Rigidbody2D component on the item
            Rigidbody2D rb = col.gameObject.GetComponent<Rigidbody2D>();
            
            //Vector2 pointing from the item to the player
            Vector2 forceDirection = (transform.position - col.transform.position).normalized;
            
            //Aplies force to the item in the forceDirection with pullSpeed
            rb.AddForce(forceDirection * pullSpeed);
            
            //If it does, call the collect method
            collectible.Collect();
        }
    }
}
