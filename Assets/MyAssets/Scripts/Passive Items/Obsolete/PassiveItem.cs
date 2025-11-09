using UnityEngine;

[System.Obsolete("This will be replaced by PassiveData class")]

public class PassiveItem : MonoBehaviour
{

    protected PlayerStats player;
    public PassiveItemScriptableObject passiveItemData;

    protected virtual void ApplyModifier ()
    {

    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>();
        ApplyModifier();
    }
}
