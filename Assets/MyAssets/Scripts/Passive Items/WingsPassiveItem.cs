using UnityEngine;

public class WingsPassiveItem : PassiveItem
{
    protected override void ApplyModifier()
    {
        player.currentMoveSpeed *= 1 + passiveItemData.Multipler / 100f; //Конвертирует выставленный в SO объекте множитель в проценты (в моем случае на 50% ускорение персонажа)
    }
    
}
