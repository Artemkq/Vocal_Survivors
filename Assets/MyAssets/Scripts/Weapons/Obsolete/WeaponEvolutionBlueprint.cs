using UnityEngine;

[System.Obsolete("This will be replaced by WeaponData class")]

[CreateAssetMenu (fileName = "WeaponEvolutionBlueprint", menuName = "ScriptableObjects/WeaponEvolutionBlueprint")]

public class WeaponEvolutionBlueprint : ScriptableObject
{
    public WeaponScriptableObject baseWeaponData;
    public PassiveItemScriptableObject catalystPassiveItemData;
    public WeaponScriptableObject evolvedWeaponData;
    public GameObject evolvedWeapon;
}
