using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[CustomEditor(typeof(UILevelSelector))]

// Имя класса было UILevelSelectorEd, я сохранил его, но обычно используют UILevelSelectorEditor
public class UILevelSelectorEd : Editor
{
    UILevelSelector selector;

    void OnEnable()
    {
        // Point to the UILevelSelector when it's in the inspector so its variables can be accessed.
        selector = target as UILevelSelector;
    }

    public override void OnInspectorGUI()
    {
        // ... (GUI code remains the same) ...
        // Create a button in the inspector with the name, that creates the level structs/templates when clicked.
        base.OnInspectorGUI();

        // If a Toggle Template isn't set, show a warning that the button will not completely work.
        if (!selector.toggleTemplate)
            EditorGUILayout.HelpBox(
                "You need to assign a Toggle Template for the button below to work properly.",
                MessageType.Warning
            );

        if (GUILayout.Button("Find and Populate Levels"))
        {
            PopulateLevelsList();
            CreateLevelSelectToggles();
        }

        // --- НОВАЯ ЧАСТЬ: Отображение текущего списка сцен с помощью SceneAsset в инспекторе ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Assigned Levels (Editor View)", EditorStyles.boldLabel);

        // Позволяет пользователю изменять/удалять элементы списка в инспекторе.
        SerializedObject serializedObject = new SerializedObject(selector);
        SerializedProperty levelsProperty = serializedObject.FindProperty("levels");
        
        serializedObject.Update();

        for (int i = 0; i < selector.levels.Count; i++)
        {
            UILevelSelector.SceneData levelData = selector.levels[i];
            
            EditorGUILayout.BeginHorizontal();
            
            // 1. Загружаем SceneAsset на основе сохраненного имени (Runtime safe)
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(GetScenePathFromName(levelData.sceneName));
            
            // 2. Отображаем ObjectField для выбора SceneAsset
            sceneAsset = (SceneAsset)EditorGUILayout.ObjectField($"Level {i}: {levelData.displayName}", sceneAsset, typeof(SceneAsset), false);

            // 3. Обновляем имя сцены, если пользователь что-то поменял в инспекторе
            if (sceneAsset != null)
            {
                levelData.sceneName = sceneAsset.name;
            }
            else if (string.IsNullOrEmpty(levelData.sceneName))
            {
                 // Если объект удален из поля, а имени нет, то поле пусто
            }

            // Кнопка удаления элемента списка
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                Undo.RecordObject(selector, "Remove Level Data");
                selector.levels.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
        // --- КОНЕЦ НОВОЙ ЧАСТИ ---
    }
    
    // Вспомогательная функция для поиска пути к ассету сцены по имени
    private string GetScenePathFromName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;
        string[] assets = AssetDatabase.FindAssets(sceneName + " t:SceneAsset");
        if (assets.Length > 0)
        {
            foreach (var asset in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(asset);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == sceneName)
                {
                    return path;
                }
            }
        }
        return null;
    }


    // Function that finds all Scene files in our project, and assigns them to the levels list.
    public void PopulateLevelsList()
    {
        // Record the changes made to the UILevelSelector component as undoable and clears any null scenes (i.e. deleted/missing scenes) from the lists.
        Undo.RecordObject(selector, "Create New SceneData structs");

        // Вызываем новый метод, который мы обернули в UNITY_EDITOR в предыдущем шаге.
        // Если вы не переименовывали SceneData в RuntimeSceneData, эта строка сработает.
        SceneAsset[] maps = UILevelSelector.GetAllMaps(); 

        // Record a list of scenes that are already in.
        // Мы ищем по имени сцены, так как поля scene больше нет.
        selector.levels.RemoveAll(levelData => string.IsNullOrEmpty(levelData.sceneName)); 

        foreach (SceneAsset map in maps)
        {
            // If the current scene we are checking isn't in the Level list.
            // Мы сравниваем имена сцен.
            if (!selector.levels.Any(sceneData => sceneData.sceneName == map.name))
            {
                // Extract information from the map name using regex.
                Match m = Regex.Match(map.name, UILevelSelector.MAP_NAME_FORMAT, RegexOptions.IgnoreCase);
                string mapLabel = "Level", mapName = "New Map";
                if (m.Success)
                {
                    if (m.Groups.Count > 1) mapLabel = m.Groups[1].Value;
                    if (m.Groups.Count > 2) mapName = m.Groups[2].Value;
                }

                // Create a new RuntimeSceneData object, initialise it with default variables, and add it to the levels list.
                selector.levels.Add(new UILevelSelector.SceneData // Используем новое имя класса
                {
                    sceneName = map.name, // Используем новое поле sceneName
                    label = mapLabel,
                    displayName = mapName
                });
            }
        }
        EditorUtility.SetDirty(selector);
    }

    // ... (Метод CreateLevelSelectToggles() - нужно поменять только одну строку) ...
    public void CreateLevelSelectToggles()
    {
        // ... (code above remains same) ...

        // For every level struct in the level selector, we create a toggle for them in the level selector.
        for (int i = 0; i < selector.levels.Count; i++)
        {
            Toggle tog;
            if (i == 0)
            {
                tog = selector.toggleTemplate;
                Undo.RecordObject(tog, "Modifying the template.");
            }
            else
            {
                tog = Instantiate(selector.toggleTemplate, selector.toggleTemplate.transform.parent); // Create a toggle of the current character as a child of the original
                Undo.RegisterCreatedObjectUndo(tog.gameObject, "Created a new toggle.");
            }
            // tog.gameObject.name = selector.levels[i].scene.name; // <-- Старая строка
            tog.gameObject.name = selector.levels[i].sceneName; // <-- Новая строка (используем имя сцены)

            // ... (rest of the method remains same) ...

            // Registers the changes to be saved when done.
            EditorUtility.SetDirty(selector);
        }
    }
}
