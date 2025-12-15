using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR // Добавляем эту строку
using UnityEditor; // Оборачиваем импорт редактора
#endif // Добавляем эту строку

public class UILevelSelector : MonoBehaviour
{
    public UISceneDataDisplay statsUI;

    public static int selectedLevel = -1;
    public static SceneData currentLevel;
    public List<SceneData> levels = new List<SceneData>();

    [Header("Template")]
    public Toggle toggleTemplate;
    public string LevelNamePath = "Level Name";
    public string LevelNumberPath = "Level Number";
    public string LevelDescriptionPath = "Level Description";
    public string LevelImagePath = "Level Image";
    public List<Toggle> selectableToggles = new List<Toggle>();

    // The level modifiers will be applied to players and enemies using a buff. 
    // The buff data is stored in this static variable.
    public static BuffData globalBuff;

    // Whenever a global Buff is applied, we will check whether the buff has any
    // effect on the player or enemies and record them here. If there isn't, we don't 
    // apply the buff to save overhead.
    public static bool globalBuffAffectsPlayer = false, globalBuffAffectsEnemies = false;


    // This is the regex which is used to identify which maps are level maps.
    public const string MAP_NAME_FORMAT = "^(Level .*?) ?- ?(.*)$";

    [System.Serializable]
    public class SceneData
    {
        // public SceneAsset scene; // <-- Эту строку нужно удалить или закомментировать
        public string sceneName; // <-- Добавляем строку для хранения имени сцены (безопасно для билда)

        [Header("UI Display")]
        public string displayName;
        public string label;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Modifiers")]
        public CharacterData.Stats playerModifier;
        public EnemyStats.Stats enemyModifier;
        [Min(-1)] public float timeLimit = 0f, clockSpeed = 1f;
        [TextArea] public string extraNotes = "--";
    }

    // Оборачиваем весь метод в UNITY_EDITOR
#if UNITY_EDITOR
    public static SceneAsset[] GetAllMaps()
    {
        List<SceneAsset> maps = new List<SceneAsset>();

        // Populate the list with all Scenes starting with "Level -" (Editor only).
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            if (assetPath.EndsWith(".unity"))
            {
                SceneAsset map = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                if (map != null && Regex.IsMatch(map.name, MAP_NAME_FORMAT))
                {
                    maps.Add(map);
                }
            }
        }
        maps.Reverse();
        return maps.ToArray();
    }
#endif // Закрываем директиву

    // For normal scene changes.          
    public void SceneChange(string name)
    {
        SceneManager.LoadScene(name);
        Time.timeScale = 1;
    }

    // To load a level from the level select screen.

    public void LoadSelectedLevel()
    {
        if (selectedLevel >= 0)
        {
            // Используем новое поле sceneName
            SceneManager.LoadScene(levels[selectedLevel].sceneName);
            currentLevel = levels[selectedLevel];
            selectedLevel = -1;
            Time.timeScale = 1f;
        }
        else
        {
            Debug.LogWarning("No level was selected!");
        }
    }

    void Start()
    {
        // Выбираем первый уровень (индекс 0) по умолчанию при старте сцены
        if (levels.Count > 0 && selectableToggles.Count > 0)
        {
            // Устанавливаем переключатель (Toggle) первого уровня в состояние "Включено"
            // Это визуально выделит его в интерфейсе и вызовет Select(0) через Event System (если настроено)
            selectableToggles[0].isOn = true;
            // Также явно вызываем Select(0) на случай, если Toggle не настроен на вызов этого метода
            Select(0);
        }
    }

    // Selects a scene that will be loaded with LoadSelectedLevel().
    // Also creates the buff that will be applied on that level, and checks if
    // the modifier variables are empty (which are used by PlayerStats and EnemyStats). 1 reference
    public void Select(int sceneIndex)
    {
        selectedLevel = sceneIndex;
        statsUI.UpdateFields();
        globalBuff = GenerateGlobalBuffData();
        globalBuffAffectsPlayer = globalBuff && IsModifierEmpty(globalBuff.variations[0].playerModifier);
        globalBuffAffectsEnemies = globalBuff && IsModifierEmpty(globalBuff.variations[0].enemyModifier);
    }

    // Generate a BuffData object to wrap around the playerModifer and enemyModifier variables. 1 reference
    public BuffData GenerateGlobalBuffData()
    {
        BuffData bd = ScriptableObject.CreateInstance<BuffData>();
        bd.name = "Global Level Buff";
        bd.variations[0].damagePerSecond = 0;
        bd.variations[0].duration = 0;
        bd.variations[0].playerModifier = levels[selectedLevel].playerModifier;
        bd.variations[0].enemyModifier = levels[selectedLevel].enemyModifier;
        return bd;
    }

    // Used to check if the playerModifier or enemyModifier of the global buff is empty. 2 references
    private static bool IsModifierEmpty(object obj)
    {
        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields();
        float sum = 0;
        foreach (FieldInfo f in fields)
        {
            object val = f.GetValue(obj);
            if (val is int) sum += (int)val;
            else if (val is float) sum += (float)val;
        }
        return Mathf.Approximately(sum, 0);
    }
}