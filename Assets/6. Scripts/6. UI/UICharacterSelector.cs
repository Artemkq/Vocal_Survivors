using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UICharacterSelector : MonoBehaviour
{
    public CharacterData defaultCharacter;
    public static CharacterData selected;
    public UIStatDisplay statsUI;

    public static UICharacterSelector instance;

    [Header("Template")]
    public Toggle toggleTemplate;
    public string characterNamePath = "Character Name";
    public string weaponIconPath = "Weapon Icon";
    public string characterIconPath = "Character Icon";
    public List<Toggle> selectableToggles = new List<Toggle>();

    [Header("DescriptionBox")]
    public TextMeshProUGUI characterFullName;
    public TextMeshProUGUI characterDescription;
    public Image selectedCharacterIcon;
    public Image selectedCharacterWeapon;

    void Start()
    {
        //If there is a default character assigned, select it on scene load
        if (defaultCharacter) Select(defaultCharacter);
    }

    public static CharacterData[] GetAllCharacterDataAssets()
    {
        List<CharacterData> characters = new List<CharacterData>();

        //Populate the list with all CharacterData assets (Editor only)
#if UNITY_EDITOR
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            if (assetPath.EndsWith(".asset"))
            {
                CharacterData characterData = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
                if (characterData != null)
                {
                    characters.Add(characterData);
                }
            }
        }
#else
        Debug.LogWarning("This function cannot be called on builds");
#endif
        return characters.ToArray();
    }

    public static CharacterData GetData()
    {
        //Use the selected character chosen in the Select() function
        if (selected)
            return selected;
        else
        {
            //Randomly pick a character if we are playing from the Editor
            CharacterData[] characters = GetAllCharacterDataAssets();
            if (characters.Length > 0) return characters[Random.Range(0, characters.Length)];
        }
        return null;
    }

    public void Select(CharacterData character)
    {
        //Update stat fields in character select screen
        selected = statsUI.character = character;
        statsUI.UpdateFields();

        //Update description box contents
        characterFullName.text = character.FullName;
        characterDescription.text = character.CharacterDescription;
        selectedCharacterIcon.sprite = character.Icon;
        selectedCharacterWeapon.sprite = character.StartingWeapon.icon;
    }
}



//void Awake()
//{
//    if (instance == null)
//    {
//        instance = this;
//        DontDestroyOnLoad(gameObject);
//    }
//    else
//    {
//        Debug.LogWarning("EXTRA" + this + "DELETED");
//        Destroy(gameObject);
//    }
//}

//public void DestroySingleton()
//{
//    instance = null;
//    Destroy(gameObject);
//}

//    public static CharacterData GetData()
//    {
//        if (instance && instance.characterData)
//            return instance.characterData;
//        else
//        {
//            //Randomly pick a charecter if we are playing from the Editor
//#if UNITY_EDITOR
//            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
//            List<CharacterData> characters = new List<CharacterData>();
//            foreach (string assetPath in allAssetPaths)
//            {
//                if (assetPath.EndsWith(".asset"))
//                {
//                    CharacterData characterData = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
//                    if (characterData != null)
//                    {
//                        characters.Add(characterData);
//                    }
//                }
//            }

//            //Pick a random character if we have found any characters
//            if (characters.Count > 0) return characters[Random.Range(0, characters.Count)];
//#endif
//        }
//        return null;
//    }

//public void SelectCharacter(CharacterData character)
//{
//    characterData = character;
//}




