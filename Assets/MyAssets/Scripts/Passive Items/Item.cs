using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for both the Passive and the Weapon classes. It is primarily intended
/// to handle weapon evolution, as we want both weapons and passives to be evolve-able
/// </summary>

public abstract class Item : MonoBehaviour
{
    public int currentLevel = 1, maxLevel = 1;

    protected PlayerStats owner; 

    public virtual void Initialise(ItemData data)
    {
        maxLevel = data.maxLevel;
        owner = FindAnyObjectByType<PlayerStats>();
    }

    public virtual bool CanLevelUp()
    {
        return currentLevel <= maxLevel;
    }

    //Whenever an item levels up, attempt to make it evolve
    public virtual bool DoLevelUp()
    {
        //Weapon Evolution logic will go here later
        return true;
    }

    //What effects you receive on equipping an item
    public virtual void OnEquip() { }

    //What effects are removed on unequipping an item
    public virtual void OnUnequip() { }
}
