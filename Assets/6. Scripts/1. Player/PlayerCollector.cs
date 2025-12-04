using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]

public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D detector;
    public float pullSpeed;

    public delegate void OnDiamondCollected();
    public OnDiamondCollected onDiamondCollected;

    float diamonds;

    void Start()
    {
        player = GetComponentInParent<PlayerStats>();
        diamonds = 0;
    }

    public void SetRadius(float r)
    {
        if (!detector) detector = GetComponent<CircleCollider2D>();
        detector.radius = r;
    }

    public float GetDiamonds() { return diamonds; }
    
    //Updated diamonds Display and information
    public float AddDiamonds (float amount)
    {
        diamonds += amount;
        onDiamondCollected();
        return diamonds;
    }

    //Saves the collected coins to the save file
    public void SaveDiamondsToStash()
    {
        SaveManager.LastLoadedGameData.diamonds += diamonds;
        diamonds = 0;
        SaveManager.Save();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //Check if the other GameObject is a Pickup 
        if (col.TryGetComponent(out Pickup p))
        {
            p.Collect(player, pullSpeed);
        }
    }
}
