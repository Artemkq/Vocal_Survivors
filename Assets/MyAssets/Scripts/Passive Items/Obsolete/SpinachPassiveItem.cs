using UnityEngine;

[System.Obsolete("This will be replaced by PassiveData class")]
public class SpinachPassiveItem : PassiveItem
{
    protected override void ApplyModifier()
    {
        player.CurrentMight *= 1 + passiveItemData.Multipler / 100f;
    }
}

