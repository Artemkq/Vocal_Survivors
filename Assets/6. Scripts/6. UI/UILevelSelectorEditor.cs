using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[CustomEditor(typeof(UILevelSelector))]

public class UILevelSelectorEditor : Editor
{
    UILevelSelector selector;

    void OnEnable()
    {
        // Point to the UILevelSelector when it's in the inspector so its variables can be accessed.
        selector = target as UILevelSelector;
    }

    public override void OnInspectorGUI()
    {
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
    }

    // Function that finds all Scene files in our project, and assigns them to the levels list.
    public void PopulateLevelsList()
    {
        // Record the changes made to the UILevelSelector component as undoable and clears any null scenes (i.e. deleted/missing scenes) from the lists.
        Undo.RecordObject(selector, "Create New SceneData structs");
        SceneAsset[] maps = UILevelSelector.GetAllMaps();

        // Record a list of scenes that are already in.
        selector.levels.RemoveAll(levels => levels.scene == null);

        foreach (SceneAsset map in maps)
        {
            // If the current scene we are checking isn't in the Level list.
            // We exclude all scenes that are already in to avoid overwriting the user's settings.
            if (!selector.levels.Any(sceneData => sceneData.scene == map))
            {
                // Extract information from the map name using regex.
                Match m = Regex.Match(map.name, UILevelSelector.MAP_NAME_FORMAT, RegexOptions.IgnoreCase);
                string mapLabel = "Level", mapName = "New Map";
                if (m.Success)
                {
                    if (m.Groups.Count > 1) mapLabel = m.Groups[1].Value;
                    if (m.Groups.Count > 2) mapName = m.Groups[2].Value;
                }


                // Create a new SceneData object, initialise it with default variables, and add it to the levels list.
                selector.levels.Add(new UILevelSelector.SceneData
                {
                    scene = map,
                    label = mapLabel,
                    displayName = mapName
                });
            }
        }
    }

    // With a Toggle Template assigned, this function will create the UI toggles
    // that we can use to select the levels in-game.

    public void CreateLevelSelectToggles()
    {
        // If the toggle template is not assigned, leave a warning and abort.
        if (!selector.toggleTemplate)
        {
            Debug.LogWarning("Failed to create the Toggles for selecting levels. Please assign the Toggle Template.");
            return;
        }

        // Loop through all the children of the parent of the toggle template,
        // and deleting everything under it except the template.
        for (int i = selector.toggleTemplate.transform.parent.childCount - 1; i >= 0; i--)
        {
            Toggle tog = selector.toggleTemplate.transform.parent.GetChild(i).GetComponent<Toggle>();
            if (tog == selector.toggleTemplate) continue;
            Undo.DestroyObjectImmediate(tog.gameObject); // Record the action so we can undo.
        }


        // Record the changes made to the UILevelSelector component as undoable and clears the toggle list.
        Undo.RecordObject(selector, "Updates to UILevelSelector.");
        selector.selectableToggles.Clear();

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
            tog.gameObject.name = selector.levels[i].scene.name;

            // Finding the level name, number, description and image to assign.
            Transform levelName = tog.transform.Find(selector.LevelImagePath).Find("Name Holder").Find(selector.LevelNamePath);
            if (levelName && levelName.TryGetComponent(out TextMeshProUGUI lvlName))
            {
                lvlName.text = selector.levels[i].displayName;
            }

            Transform levelNumber = tog.transform.Find(selector.LevelImagePath).Find(selector.LevelNumberPath);
            if (levelNumber && levelNumber.TryGetComponent(out TextMeshProUGUI lvlNumber))
            {
                lvlNumber.text = selector.levels[i].label;
            }

            Transform levelDescription = tog.transform.Find(selector.LevelDescriptionPath);
            if (levelDescription && levelDescription.TryGetComponent(out TextMeshProUGUI lvlDescription))
            {
                lvlDescription.text = selector.levels[i].description;
            }

            Transform levelImage = tog.transform.Find(selector.LevelImagePath);
            if (levelImage && levelImage.TryGetComponent(out Image lvlImage))
                lvlImage.sprite = selector.levels[i].icon;

            selector.selectableToggles.Add(tog);

            // Remove all select events and add our own event that checks which character toggle was clicked.
            for (int j = 0; j < tog.onValueChanged.GetPersistentEventCount(); j++)
            {
                if (tog.onValueChanged.GetPersistentMethodName(j) == "Select")
                {
                    UnityEventTools.RemovePersistentListener(tog.onValueChanged, j);
                }
            }
            UnityEventTools.AddIntPersistentListener(tog.onValueChanged, selector.Select, i);
        }
        // Registers the changes to be saved when done.
        EditorUtility.SetDirty(selector);
    }
}