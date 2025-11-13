using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

public class CharacterSelector : MonoBehaviour
{
   
    public static CharacterSelector instance;
    public CharacterData characterData;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("EXTRA" + this + "DELETED");
            Destroy(gameObject);
        }
    }

    public static CharacterData GetData()
    {
        if (instance && instance.characterData)
            return instance.characterData;
        else
        {
            //Randomly pick a charecter if we are playing from the Editor
            #if UNITY_EDITOR
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            List<CharacterData> characters = new List<CharacterData>();
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

            //Pick a random character if we have found any characters
            if (characters.Count > 0) return characters[Random.Range(0, characters.Count)];
            #endif
        }
        return null;
    }

    public void SelectCharacter (CharacterData character)
    {
        characterData = character;
    }

    public void DestroySingleton()
    {
        instance = null;
        Destroy(gameObject);
    }
}
