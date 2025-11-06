using UnityEngine;

public class BobbingAnimation : MonoBehaviour
{
    public float frequency; // Speed of movement
    public float magnitude; // Range of movement
    public Vector3 direction; // Direction of movement
    Vector3 initialPosition;

    void Start()
    {
        //Save the starting position of the game object
        initialPosition = transform.position;
    }

    void Update()
    {
        //Sine function for smooth bobbing effect
        transform.position = initialPosition + direction * Mathf.Sin(Time.time * frequency) * magnitude;    
    }
}
