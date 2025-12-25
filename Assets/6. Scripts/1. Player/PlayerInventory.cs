using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Item item;

        public void Assign(Item assignedItem)
        {
            item = assignedItem;
            if (item is Weapon)
            {
                Weapon w = item as Weapon;
            }
            else
            {
                Passive p = item as Passive;
            }
            Debug.Log(string.Format("Надето оружие: {0}", item.name));
        }

        public void Clear()
        {
            item = null;
        }

        public bool IsEmpty() { return item == null; }
    }

    public List<Slot> weaponSlots = new List<Slot>(6);
    public List<Slot> passiveSlots = new List<Slot>(6);
    public UIInventoryIconsDisplay weaponUI, passiveUI;

    [Header("UI Elements")]
    public List<WeaponData> availableWeapons = new List<WeaponData>(); //List of upgrade options for weapons
    public List<PassiveData> availablePassives = new List<PassiveData>(); //List of upgrade options for passive items
    public UIUpgradeWindow upgradeWindow;

    PlayerStats player;

    void Start()
    {
        player = GetComponent<PlayerStats>();
    }

    //Checks if the inventory has an item of a certaint type
    public bool Has(ItemData type) { return Get(type); }

    public Item Get(ItemData type)
    {
        if (type is WeaponData) return Get(type as WeaponData);
        else if (type is PassiveData) return Get(type as PassiveData);
        return null;
    }

    //Find a passive of a certain type in the inventory
    public Passive Get(PassiveData type)
    {
        foreach (Slot s in passiveSlots)
        {
            Passive p = s.item as Passive;
            if (p && p.data == type)
                return p;
        }
        return null;
    }

    //Find a weapon of a certain type in the inventory
    public Weapon Get(WeaponData type)
    {
        foreach (Slot s in weaponSlots)
        {
            Weapon w = s.item as Weapon;
            if (w && w.data == type)
                return w;
        }
        return null;
    }

    //Removes a weapon of a particular type, as specified by <data>
    public bool Remove(WeaponData data, bool removeUpgradeAvailability = false)
    {
        //Remove this weapon from the upgrade pool
        if (removeUpgradeAvailability) availableWeapons.Remove(data);

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            Weapon w = weaponSlots[i].item as Weapon;
            if (w.data == data)
            {
                weaponSlots[i].Clear();
                w.OnUnequip();
                Destroy(w.gameObject);
                return true;
            }
        }
        return false;
    }

    //Removes a passive of a particular type, as specified by <data>
    public bool Remove(PassiveData data, bool removeUpgradeAvailability = false)
    {
        //Remove this passive from the upgrade pool
        if (removeUpgradeAvailability) availablePassives.Remove(data);

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            Passive p = weaponSlots[i].item as Passive;
            if (p.data == data)
            {
                weaponSlots[i].Clear();
                p.OnUnequip();
                Destroy(p.gameObject);
                return true;
            }
        }
        return false;
    }

    //If an ItemData is passed, determine what type it is and call the respective overload
    //We also have an optional boolean to remove this item from the upgrade list

    public bool Remove(ItemData data, bool removeUpgradeAvailability = false)
    {
        if (data is PassiveData) return Remove(data as PassiveData, removeUpgradeAvailability);
        else if (data is WeaponData) return Remove(data as WeaponData, removeUpgradeAvailability);
        return false;
    }

    //Finds an empty slot and adds a weapon of a certain type, returns
    //the slot number that the irem was put in

    public int Add(WeaponData data, bool updateUI = true)
    {
        int slotNum = -1;

        //Try to find an empty slot
        for (int i = 0; i < weaponSlots.Capacity; i++)
        {
            if (weaponSlots[i].IsEmpty())
            {
                slotNum = i;
                break;
            }
        }

        //If there is no empty slot, exit
        if (slotNum < 0) return slotNum;

        //Otherwise create the weapon in the slot
        //Get the type of the weapon we want to spawn
        Type weaponType = Type.GetType(data.behaviour);

        if (weaponType != null)
        {
            //Spawn the weapon GameObject
            GameObject go = new GameObject(data.baseStats.name + " Controller");
            Weapon spawnedWeapon = (Weapon)go.AddComponent(weaponType);
            spawnedWeapon.transform.SetParent(transform); //Set the weapon to be a child of the player
            spawnedWeapon.transform.localPosition = Vector2.zero;
            spawnedWeapon.Initialise(data);
            spawnedWeapon.OnEquip();

            //Assign the weapon to the slot
            weaponSlots[slotNum].Assign(spawnedWeapon);
            if(updateUI) weaponUI.Refresh();

            //Close the level up UI if it is on
            if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
                GameManager.instance.EndLevelUp();

            return slotNum;
        }
        else
        {
            Debug.LogWarning(string.Format("Invalid weapon type specified for {0}", data.name));
        }
        return -1;
    }

    //Finds an empty slot and adds a passive of a certain type, returns
    //the slot number that the item was put in

    public int Add(PassiveData data, bool updateUI = true)
    {
        int slotNum = -1;

        //Try to find an empty slot
        for (int i = 0; i < passiveSlots.Capacity; i++)
        {
            if (passiveSlots[i].IsEmpty())
            {
                slotNum = i;
                break;
            }
        }

        //If there is no empty slot, exit
        if (slotNum < 0) return slotNum;

        //Otherwise create the passive in the slot
        //Get the type of the passive we want to spawn
        GameObject go = new GameObject(data.baseStats.name + " Passive");
        Passive p = go.AddComponent<Passive>();
        p.Initialise(data);
        p.transform.SetParent(transform); //Set the weapon to be a child of the player
        p.transform.localPosition = Vector2.zero;

        //Assign the passive to the slot
        passiveSlots[slotNum].Assign(p);
        if(updateUI) passiveUI.Refresh();

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
        player.RecalculateStats();

        return slotNum;
    }

    //If we dont know what item is being added, this function will determine that
    public int Add(ItemData data, bool updateUI = true)
    {
        if (data is WeaponData) return Add(data as WeaponData, updateUI);
        else if (data is PassiveData) return Add(data as PassiveData, updateUI);
        return -1;
    }

    //Overload so that we can use both ItemData or Item to level up an
    //item in the inventory
    public bool LevelUp(ItemData data, bool updateUI = true)
    {
        Item item = Get(data);
        if (item) return LevelUp(item, updateUI);
        return false;
    }

    //Levels up a selected weapon in the player inventory
    public bool LevelUp(Item item, bool updateUI = true)
    {
        //Tries to level up the item
        if (!item.DoLevelUp())
        {
            Debug.LogWarning(string.Format(
                "Failed to level up {0}.",
                item.name
                ));
            return false;
        }

        //Update the UI after the weapon has leveledup
        if (updateUI)
        {
            weaponUI.Refresh();
            passiveUI.Refresh();
        }

        //Close the level up screen afterwards
        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
        //If it is a passive, recalculate player stats
        if (item is Passive) player.RecalculateStats();
        return true;
    }

    //Determines what upgrade options should apeear
    void ApplyUpgradeOptions()
    {
        //<availableUpgrades> is an empty list will be filtered from
        //<allUpgrades>, which is the list of ALL upgrades in PlayerInventory
        //Not all upgrades can be applied, as some may have already been
        //maxed out the player, or the player may not have enough inventory slots
        List<ItemData> availableUpgrades = new List<ItemData>();
        List<ItemData> allUpgrades = new List<ItemData>(availableWeapons);
        allUpgrades.AddRange(availablePassives);

        //We need to know how many weapon/passive slots are left
        int weaponSlotsLeft = GetSlotsLeft(weaponSlots);
        int passiveSlotsLeft = GetSlotsLeft(passiveSlots);

        //Filters through the available weapons and passives and add those
        //that can possibly be an option
        foreach (ItemData data in allUpgrades)
        {
            //If a weapon of this type exists, allow for the upgrade if the
            //level of the weapon is not already maxed out
            Item obj = Get(data);
            if (obj)
            {
                if (obj.currentLevel <= data.maxLevel) availableUpgrades.Add(data);
            }
            else
            {
                //If we dont have this item in the inventory yet, check if
                //we still have enough slots to take new items
                if (data is WeaponData && weaponSlotsLeft > 0) availableUpgrades.Add(data);
                else if (data is PassiveData && passiveSlotsLeft > 0) availableUpgrades.Add(data);
            }
        }

        //Show the UI upgrade windown if we still have available upgrades left
        int availUpgradeCount = availableUpgrades.Count;
        if (availUpgradeCount > 0)
        {
            bool getExtraItem = 1f - 1f / player.Stats.luck > UnityEngine.Random.value;
            if (getExtraItem || availUpgradeCount < 4) upgradeWindow.SetUpgrades(this, availableUpgrades, 4);
            else upgradeWindow.SetUpgrades(this, availableUpgrades, 3, "Increase your Luck stat for a chance to get 4 items!");
        }
        else if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
    }

    public void RemoveAndApplyUpgrades()
    {
        ApplyUpgradeOptions();
    }

    // Get all the slots from the player of a certain type, 
    // either Weapon or Passive. If you get all slots of Item, 
    // it will return all Weapons and Passives.

    public Slot[] GetSlots<T>() where T : Item
    {
        // Check which set of slots to return.
        // If you get Items, it will give you both weapon and passive slots.
        switch (typeof(T).ToString())
        {
            case "Passive":
                return passiveSlots.ToArray();
            case "Weapon":
                return weaponSlots.ToArray();
            case "Item":
                List<Slot> s = new List<Slot>(passiveSlots);
                s.AddRange(weaponSlots);
                return s.ToArray();
        }
        // If you have other subclasses of Item, you will need to add extra cases above to
        // prevent this message from triggering. This message is here to help developers pinpoint 
        // the part of the code they need to update.
        Debug.LogWarning("Generic type provided to GetSlots() call does not have a coded behaviour.");
        return null;
    }

    // Version of GetSlots() that works with ItemData instead of Item.
    public Slot[] GetSlotsFor<T>() where T : ItemData
    {
        if (typeof(T) == typeof(PassiveData))
        {
            return passiveSlots.ToArray();
        }
        else if (typeof(T) == typeof(WeaponData))
        {
            return weaponSlots.ToArray();
        }
        else if (typeof(T) == typeof(ItemData))
        {
            List<Slot> s = new List<Slot>(passiveSlots);
            s.AddRange(weaponSlots);
            return s.ToArray();
        }
        // If you have other subclasses of Item, you will need to add extra cases above to
        // prevent this message from triggering. This message is here to help developers pinpoint
        // the part of the code they need to update.
        Debug.LogWarning("Generic type provided to GetSlotsFor() call does not have a coded behaviour.");
        return null;
    }

    //Checks a list of slots to see if there are any empty item slots in this list
    int GetSlotsLeft(List<Slot> slots)
    {
        int count = 0;
        foreach (Slot s in slots)
        {
            if (s.IsEmpty()) count++;
        }
        return count;
    }

    public int GetSlotsLeft<T>() where T : Item { return GetSlotsLeft(new List<Slot>(GetSlots<T>())); }
    public int GetSlotsLeftFor<T>() where T : ItemData { return GetSlotsLeft(new List<Slot>(GetSlotsFor<T>())); }

    public T[] GetAvailable<T>() where T : ItemData
    {
        if (typeof(T) == typeof(PassiveData))
        {
            return availablePassives.ToArray() as T[];
        }
        else if (typeof(T) == typeof(WeaponData))
        {
            return availableWeapons.ToArray() as T[];
        }
        else if (typeof(T) == typeof(ItemData))
        {
            List<ItemData> list = new List<ItemData>(availablePassives);
            list.AddRange(availableWeapons);
            return list.ToArray() as T[];
        }

        Debug.LogWarning("Generic type provided to GetAvailable() call does not have a coded behaviour.");
        return null;
    }


    // Get all available items (weapons or passives) that we still do not have yet. 1 reference
    public T[] GetUnowned<T>() where T : ItemData
    {
        // Get all available items.
        var available = GetAvailable<T>();

        if (available == null || available.Length == 0)
            return new T[0]; // Return empty array if null or empty

        List<T> list = new List<T>(available);

        // Check all of our slots, and remove all items in the list that are found in the slots.
        var slots = GetSlotsFor<T>();
        if (slots != null)
        {
            foreach (Slot s in slots)
            {
                if (s?.item?.data != null && list.Contains(s.item.data as T))
                    list.Remove(s.item.data as T);
            }
        }
        return list.ToArray();
    }

    public T[] GetEvolvables<T>() where T : Item
    {
        // Check all the slots, and find all the items in the slot that 
        // are capable of evolving.
        List<T> result = new List<T>();
        foreach (Slot s in GetSlots<T>())
            if (s.item is T t && t.CanEvolve(0).Length > 0) result.Add(t);
        return result.ToArray();
    }


    public T[] GetUpgradables<T>() where T : Item
    {
        // Check all the slots, and find all the items in the slot that 
        // are still capable of levelling up.
        List<T> result = new List<T>();
        foreach (Slot s in GetSlots<T>())
            if (s.item is T t && t.CanLevelUp()) result.Add(t);
        return result.ToArray();
    }
}