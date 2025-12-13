using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Define the different states of the game
    public enum GameState
    {
        Gameplay,
        Paused,
        GameOver,
        LevelUp,
        TreasureChest
    }

    // Store the current state of the game
    public GameState currentState;
    // Store the previous state of the game
    public GameState previousState;

    [Header("Damage Text Settings")]
    public Canvas damageTextCanvas;
    public float textFontSize = 40;
    public TMP_FontAsset textFont;
    public Camera referenceCamera;

    [Header("Screens")]
    public GameObject pauseScreen;
    public GameObject resultScreen;
    public GameObject levelUpScreen;
    int stackedLevelUps = 0; //If we try to StartLevelUp() multiple times

    [Header("Current Stat Displays")]
    public TMP_Text currentHealthDisplay;
    public TMP_Text currentRecoveryDisplay;
    public TMP_Text currentMoveSpeedDisplay;
    public TMP_Text currentMightDisplay;
    public TMP_Text currentProjectileSpeedDisplay;
    public TMP_Text currentMagnetDisplay;

    [Header("Results Screen Displays")]
    public Image chosenCharacterImage;
    public TMP_Text chosenCharacterName;
    public TMP_Text levelReachedDisplay;
    public TMP_Text timeSurvivedDisplay;

    const float DEFAULT_TIME_LIMIT = 1800f;
    const float DEFAULT_CLOCK_SPEED = 1f;
    float ClockSpeed => UILevelSelector.currentLevel?.clockSpeed ?? DEFAULT_CLOCK_SPEED;
    float TimeLimit => UILevelSelector.currentLevel?.timeLimit ?? DEFAULT_TIME_LIMIT;

    [Header("Stopwatch")]
    public float timeLimit; //The time limit in seconds
    float stopwatchTime; //The current time elapsed since the stopwatch started
    public TMP_Text stopwatchDisplay;

    bool levelEnded = false; //Has the time limit been reached?
    public GameObject evilWizardPrefab; //Spawns after time limit has been reached

    PlayerStats[] players; //Track all players

    public bool isGameOver { get { return currentState == GameState.GameOver; } }
    public bool choosingUpgrade { get { return currentState == GameState.LevelUp; } }

    // ДОБАВЛЕНО СВОЙСТВО:
    public bool isPaused { get { return currentState == GameState.Paused; } }

    public float GetElapsedTime() { return stopwatchTime; }

    //Sums up the curse stat of all players and returns the value
    public static float GetCumulativeCurse()
    {
        if (!instance) return 1;

        float totalCurse = 0;
        foreach (PlayerStats p in instance.players)
        {
            totalCurse += p.Actual.curse;
        }
        return Mathf.Max(1, 1 + totalCurse);
    }

    //Sums up the levels of all players and returns the value
    public static int GetCumulativeLevels()
    {
        if (!instance) return 1;

        int totalLevel = 0;
        foreach (PlayerStats p in instance.players)
        {
            totalLevel += p.level;
        }
        return Mathf.Max(1, totalLevel);

    }

    void Awake()
    {
        players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

        //Set the levels Time Limit
        timeLimit = TimeLimit;

        //Warning check to see if there is another singleton of this kind in the game
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("EXTRA" + this + "DELETED");
            Destroy(gameObject);
        }
        DisableScreens();
    }

    void Start()
    {
        // Ограничиваем игру 60 кадрами в секунду
        Application.targetFrameRate = 60;

        // Отключаем вертикальную синхронизацию, чтобы targetFrameRate работал стабильно
        QualitySettings.vSyncCount = 0;

    }

    void Update()
    {
        // Define the behaviour for each state

        switch (currentState)
        {
            case GameState.Gameplay:
                // Code for the gameplay state
                CheckForPauseAndResume();
                UpdateStopwatch();
                break;

            case GameState.Paused:
                // Code for the paused state
                CheckForPauseAndResume();
                break;

            case GameState.GameOver:

            case GameState.LevelUp:
                break;

            default:
                Debug.LogWarning("STATE DOES NOT EXIST");
                break;
        }
    }

    IEnumerator GenerateFloatingTextCoroutine(string text, Transform target, float duration = 1f, float speed = 50f)
    {
        //Start generating the floating text
        GameObject textObj = new GameObject("Damage Floating Text");
        RectTransform rect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI tmPro = textObj.AddComponent<TextMeshProUGUI>();
        tmPro.text = text;
        tmPro.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmPro.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmPro.fontSize = textFontSize;
        if (textFont) tmPro.font = textFont;
        rect.position = referenceCamera.WorldToScreenPoint(target.position);

        //Makes sure this is destroyd after the duratin finishes
        Destroy(textObj, duration);

        //Parent the generated text object to the canvas
        textObj.transform.SetParent(instance.damageTextCanvas.transform);
        textObj.transform.SetSiblingIndex(0);

        //Pan the text upwards and fade it away over time
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0;
        float yOffset = 0;
        Vector3 lastKnownPosition = target.position;

        while (t < duration)
        {
            //If the rect object is missing for whatever reason, terminate this loop
            if (!rect) break;

            //Fade the text to the right alpha value
            tmPro.color = new Color(tmPro.color.r, tmPro.color.g, tmPro.color.b, 1 - t / duration);

            //If target exitsts, then save its position
            if (target)
                lastKnownPosition = target.position;

            //Pan the text upwards
            yOffset += speed * Time.deltaTime;
            rect.position = referenceCamera.WorldToScreenPoint(lastKnownPosition + new Vector3(0, yOffset));

            //Wait for a frame and update the time
            yield return w;
            t += Time.deltaTime;
        }
    }

    public static void GenerateFloatingText(string text, Transform target, float duration = 1f, float speed = 1f)
    {
        //If the canvas is not set, end the function so we dont generane any floationg text
        if (!instance.damageTextCanvas) return;

        //Find a relevant camera that we can use to convert the world positions to a screen position
        if (!instance.referenceCamera) instance.referenceCamera = Camera.main;

        instance.StartCoroutine(instance.GenerateFloatingTextCoroutine(text, target, duration, speed));
    }

    // Define the method to change the state of the game
    public void ChangeState(GameState newState)
    {
        previousState = currentState;
        currentState = newState;
    }

    public void PauseGame()
    {
        if (currentState != GameState.Paused)
        {
            ChangeState(GameState.Paused);
            Time.timeScale = 0f; //Stop the game
            pauseScreen.SetActive(true);
            Debug.Log("Game is paused");
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(previousState);
            Time.timeScale = 1f; //Resume the game
            pauseScreen.SetActive(false);
            Debug.Log("Game is resumed");
        }
    }

    // Define the method to check for pause and resume input
    void CheckForPauseAndResume()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void DisableScreens()
    {
        pauseScreen.SetActive(false);
        resultScreen.SetActive(false);
        levelUpScreen.SetActive(false);
    }

    public void GameOver()
    {
        timeSurvivedDisplay.text = stopwatchDisplay.text;

        //Set the Game Over variables here
        ChangeState(GameState.GameOver);
        Time.timeScale = 0f; //Stop the game entirely
        DisplayResults();

        // Save all the diamonds of all the players to the save file.
        foreach (PlayerStats p in players)
        {
            p.GetComponentInChildren<PlayerCollector>().SaveDiamondsToStash();
        }

        // Add all players' coins to their save file, since the game has ended.
        foreach (PlayerStats p in players)
        {
            if (p.TryGetComponent(out PlayerCollector c))
            {
                c.SaveDiamondsToStash();
            }
        }
    }

    void DisplayResults()
    {
        resultScreen.SetActive(true);
    }

    public void AssignChosenCharacterUI(CharacterData chosenCharacterData)
    {
        chosenCharacterImage.sprite = chosenCharacterData.Icon;
        chosenCharacterName.text = chosenCharacterData.name;
    }

    public void AssignLevelReachedUI(int levelReachedData)
    {
        levelReachedDisplay.text = levelReachedData.ToString();
    }

    public Vector2 GetRandomPlayerLocation()
    {
        int chosenPlayer = Random.Range(0, players.Length);
        return new Vector2(players[chosenPlayer].transform.position.x,
                           players[chosenPlayer].transform.position.y);
    }

    void UpdateStopwatch()
    {
        stopwatchTime += Time.deltaTime * ClockSpeed;
        UpdateStopwatchDisplay();

        if (stopwatchTime >= timeLimit && !levelEnded)
        {
            levelEnded = true;

            //Set the enemy/event spawner GameObject inactive to stop enemies from spawning and kill the remaining enemies onscreen
            FindAnyObjectByType<WaveManager>().gameObject.SetActive(false);
            foreach (EnemyStats e in FindObjectsByType<EnemyStats>(FindObjectsSortMode.None))
                e.SendMessage("Kill");

            //Spawn the EvilWizard off-camera
            Vector2 evilWizardOffset = Random.insideUnitCircle * 50f;
            Vector2 spawnPosition = GetRandomPlayerLocation() + evilWizardOffset;
            Instantiate(evilWizardPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void UpdateStopwatchDisplay()
    {
        //Calculate the number of minutes and seconds that have elapsed
        int minutes = Mathf.FloorToInt(stopwatchTime / 60);
        int seconds = Mathf.FloorToInt(stopwatchTime % 60);

        //Update the stopwatch text to display the elapsed time
        stopwatchDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StartLevelUp()
    {
        ChangeState(GameState.LevelUp);

        //If the level up screen is already active, record it
        if (levelUpScreen.activeSelf) stackedLevelUps++;
        else
        {
            Time.timeScale = 0f; //Pause the game for now
            levelUpScreen.SetActive(true);

            foreach (PlayerStats p in players)
                p.SendMessage("RemoveAndApplyUpgrades");
        }
    }

    public void EndLevelUp()
    {
        Time.timeScale = 1f; //Resume the game
        levelUpScreen.SetActive(false);
        ChangeState(GameState.Gameplay);

        if (stackedLevelUps > 0)
        {
            stackedLevelUps--;
            StartLevelUp();
        }
    }
}
