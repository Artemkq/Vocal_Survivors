using UnityEngine;

[System.Obsolete("This will be replaced by PassiveData class")]

[CreateAssetMenu(fileName ="PassiveItemScriptableObject", menuName = "ScriptableObjects/Passive Item")]

public class PassiveItemScriptableObject : ScriptableObject
{
    [SerializeField] float multipler;
    
    public float Multipler { get => multipler; private set => multipler = value; }

    [SerializeField] int level; //Not meant to be modified in the game [Only in Editor]
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
