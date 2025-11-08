using UnityEngine;

[System.Obsolete("This will be replaced by WeaponData class")]
public class GarlicController : WeaponController
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack()
    {
        base.Attack();
        GameObject spawnedGarlic = Instantiate(weaponData.Prefab);
        spawnedGarlic.transform.position = transform.position; //Assigned the position to be same as this object which is parented to the player
        spawnedGarlic.transform.parent = transform; //So that is spawns below this object
    }

}
