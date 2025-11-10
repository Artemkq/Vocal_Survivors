using UnityEngine;

public class BobbingAnimation : MonoBehaviour
{
    public float frequency; // Speed of movement
    public float magnitude; // Range of movement
    public Vector3 direction; // Direction of movement
    Vector3 initialPosition;
    Pickup pickup;

    void Start()
    {
        pickup = GetComponent<Pickup>();
        
        //Save the starting position of the game object
        initialPosition = transform.position;
    }

    void Update()
    {
        if (pickup && !pickup.hasBeenCollected)
        {
            //Sine function for smooth bobbing effect
            transform.position = initialPosition + direction * Mathf.Sin(Time.time * frequency) * magnitude;
        }
    }
}
