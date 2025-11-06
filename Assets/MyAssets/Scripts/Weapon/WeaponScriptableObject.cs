using UnityEngine;

[CreateAssetMenu(fileName = "WeaponScriptableObject", menuName = "ScriptableObjects/Weapon")]

public class WeaponScriptableObject : ScriptableObject
{
    [SerializeField] GameObject prefab;
    public GameObject Prefab { get => prefab; private set => prefab = value; }

    //Base stats for weapons
    [SerializeField] float damage;
    public float Damage { get => damage; private set => damage = value; }


    [SerializeField] float speed;
    public float Speed { get => speed; private set => speed = value; }

    [SerializeField] float cooldownDuration;
    public float CooldownDuration { get => cooldownDuration; private set => cooldownDuration = value; }

    [SerializeField] int pierce;
    public int Pierce { get => pierce; private set => pierce = value; }

    [SerializeField] int level; //Not mean to be modified in the game [Only in Editor]
    public int Level { get => level; private set => level = value; }

    [SerializeField] GameObject nextLevelPrefab; //The prefab of the next level i.e. what the object becomes when it levels up. Not to be confused with the prefab to be spawned at the next level
    public GameObject NextLevelPrefab { get => nextLevelPrefab; private set => nextLevelPrefab = value; }

    [SerializeField] new string name;
    public string Name { get => name; private set => name = value; }

    [SerializeField] string desctription; //What is the description of this weapon? [If this weapon is and upgrade, place the description of the upgrades
    public string Desctription { get => desctription; private set => desctription = value; }

    [SerializeField] Sprite icon; //Not mean to be modified in the game [Only in Editor]
    public Sprite Icon { get => icon; private set => icon = value; }
}
