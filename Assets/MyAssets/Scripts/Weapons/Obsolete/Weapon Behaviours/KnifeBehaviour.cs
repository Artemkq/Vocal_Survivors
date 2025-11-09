using UnityEngine;

[System.Obsolete("This will be replaced by WeaponData class")]
public class KnifeBehaviour : ProjectileWeaponBehavior
{

    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        transform.position += direction * currentSpeed * Time.deltaTime;
    }
}
