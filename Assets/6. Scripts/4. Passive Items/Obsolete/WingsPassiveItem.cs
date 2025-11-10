using UnityEngine;

[System.Obsolete("This will be replaced by PassiveData class")]
public class WingsPassiveItem : PassiveItem
{
    protected override void ApplyModifier()
    {
        player.CurrentMoveSpeed *= 1 + passiveItemData.Multipler / 100f; //Конвертирует выставленный в SO объекте множитель в проценты (в моем случае на 50% ускорение персонажа)
    }
    
}
