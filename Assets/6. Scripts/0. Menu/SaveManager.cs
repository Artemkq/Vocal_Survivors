using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class SaveManager
{
    // Расширяем класс данных для вашей меты (Гараж, Персонажи)
    [System.Serializable]
    public class GameData
    {
        public float diamonds; // Валюта
        public List<string> unlockedItems = new List<string>(); // Купленные апгрейды в Гараже
        public int highscore; // Лучший счет
    }

    const string SAVE_FILE_NAME = "SaveData.json";
    static GameData lastLoadedGameData;

    public static GameData LastLoadedGameData
    {
        get
        {
            if (lastLoadedGameData == null) Load();
            return lastLoadedGameData;
        }
    }

    public static string GetSavePath() => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    public static void Save(GameData data = null)
    {
        if (data == null)
        {
            if (lastLoadedGameData == null) Load();
            data = lastLoadedGameData;
        }

        try
        {
            string json = JsonUtility.ToJson(data, true); // true сделает файл читаемым для человека
            File.WriteAllText(GetSavePath(), json);
            // В идеале в 2025 году это стоит делать через File.WriteAllTextAsync, 
            // но для начала хватит и этого, если вызывать Save() в конце уровня.
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка сохранения: " + e.Message);
        }
    }

    public static GameData Load()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                lastLoadedGameData = JsonUtility.FromJson<GameData>(json);
            }
            catch
            {
                lastLoadedGameData = new GameData();
            }
        }

        if (lastLoadedGameData == null) lastLoadedGameData = new GameData();
        return lastLoadedGameData;
    }
}
