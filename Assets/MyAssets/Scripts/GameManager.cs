using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Define the different states of the game
    public enum GameState
    {
        Gameplay,
        Paused,
        GameOver,
        LevelUp
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
    public List<Image> chosenWeaponsUI = new List<Image>(6);
    public List<Image> chosenPassiveItemsUI = new List<Image>(6);

    [Header("Stopwatch")]
    public float timeLimit; //The time limit in seconds
    float stopwatchTime; //The current time elapsed since the stopwatch started
    public TMP_Text stopwatchDisplay;

    //Flag to check if the game over
    public bool isGameOver = false;

    //Flag to check if the player is choosing their upgrades
    public bool choosingUpgrade;

    //Reference to the player's game object
    public GameObject playerObject;

    void Awake()
    {
        //Warning check to see if there is another singleton of this kind in the game
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            //Debug.LogWarning("EXTRA" + this + "DELETED");
            Destroy(gameObject);
        }
        DisableScreens();
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
                // Code for the gameover state
                if (!isGameOver)
                {
                    isGameOver = true;
                    Time.timeScale = 0f; //Stop the game entirely
                    //Debug.Log("GAME IS OVER");
                    DisplayResults();
                }
                break;

            case GameState.LevelUp:
                if (!choosingUpgrade)
                {
                    choosingUpgrade = true;
                    Time.timeScale = 0f; //Pause the game for now
                    //Debug.Log("UPGRADES SHOWN");
                    levelUpScreen.SetActive(true);
                }
                break;

            default:
                //Debug.LogWarning("STATE DOES NOT EXIST");
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

        // Проверка цели из предыдущего исправления
        Vector3 currentWorldPosition = Vector3.zero;
        if (target != null)
        {
            currentWorldPosition = target.position;
            rect.position = referenceCamera.WorldToScreenPoint(currentWorldPosition);
        }
        else
        {
            Destroy(textObj);
            yield break;
        }

        // !!! УДАЛИТЕ ЭТУ СТРОКУ, ОНА ВЫЗЫВАЕТ ПРОБЛЕМУ !!!
        // Destroy(textObj, duration); 

        //Parent the generated text object to the canvas
        textObj.transform.SetParent(instance.damageTextCanvas.transform);

        //Pan the text upwards and fade it away over time
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0;
        float yOffset = 0;

        while (t < duration)
        {
            //Wait for a frame and update the time
            yield return w;
            t += Time.deltaTime;

            // Если цель все еще существует, обновляем позицию, иначе используем последнюю известную
            if (target != null)
            {
                currentWorldPosition = target.position;
            }

            // --- СТРОКА 188 ВАШЕГО СКРИПТА ---
            // Проверяем, что textObj (и rect) еще не был уничтожен внешним вызовом (хотя мы его удалили выше, проверка не помешает)
            if (rect != null)
            {
                //Fade the text to the right alpha value
                tmPro.color = new Color(tmPro.color.r, tmPro.color.g, tmPro.color.b, 1 - t / duration);

                //Pan the text upwards
                yOffset += speed * Time.deltaTime;

                // Используем сохраненную (или обновленную) позицию мира для расчета позиции на экране
                rect.position = referenceCamera.WorldToScreenPoint(currentWorldPosition + new Vector3(0, yOffset));
            }
            else
            {
                // Если объект все-таки уничтожен (например, при выходе из игры), выходим из цикла
                break;
            }
        }

        // --- ДОБАВЬТЕ ЭТОТ КОД В КОНЕЦ ---
        // Когда цикл завершен и анимация окончена, уничтожаем объект
        if (textObj != null)
        {
            Destroy(textObj);
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
        currentState = newState;
    }

    public void PauseGame()
    {
        if (currentState != GameState.Paused)
        {
            previousState = currentState;
            ChangeState(GameState.Paused);
            Time.timeScale = 0f; //Stop the game
            pauseScreen.SetActive(true);
            //Debug.Log("Game is paused");
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(previousState);
            Time.timeScale = 1f; //Resume the game
            pauseScreen.SetActive(false);
            //Debug.Log("Game is resumed");
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
        ChangeState(GameState.GameOver);
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

    public void AssignChosenWeaponsAndPassiveItemsUI(List<Image> chosenWeaponsData, List<Image> chosenPassiveItemsData)
    {
        if (chosenWeaponsData.Count != chosenWeaponsUI.Count || chosenPassiveItemsData.Count != chosenPassiveItemsUI.Count)
        {
            //Debug.Log("Chosen weapons and passive items data lists have different lengths");
            return;
        }

        //Assign chosen weapons data to chosenWeaponsUI
        for (int i = 0; i < chosenWeaponsUI.Count; i++)
        {
            //Check that the sprite of the corresponding element in chosenWeaponsData is not null
            if (chosenWeaponsData[i].sprite)
            {
                //Enable the corresponding element in chosenWeaponsUI and set its sprite to the corresponding sprite in chosenWeaponsData
                chosenWeaponsUI[i].enabled = true;
                chosenWeaponsUI[i].sprite = chosenWeaponsData[i].sprite;
            }
            else
            {
                //If the sprite is null, disable the corresponding element in chosenWeaponsUI
                chosenWeaponsUI[i].enabled = false;
            }
        }

        //Assign chosen Passive Items data to chosenPassiveItemsUI
        for (int i = 0; i < chosenPassiveItemsUI.Count; i++)
        {
            //Check that the sprite of the corresponding element in ChosenPassiveItemsData is not null
            if (chosenPassiveItemsData[i].sprite)
            {
                //Enable the corresponding element in chosenPassiveItemsUI and set its sprite to the corresponding sprite in ChosenPassiveItemsData
                chosenPassiveItemsUI[i].enabled = true;
                chosenPassiveItemsUI[i].sprite = chosenPassiveItemsData[i].sprite;
            }
            else
            {
                //If the sprite is null, disable the corresponding element in chosenPassiveItemsUI
                chosenPassiveItemsUI[i].enabled = false;
            }
        }
    }

    void UpdateStopwatch()
    {
        stopwatchTime += Time.deltaTime;

        UpdateStopwatchDisplay();

        if (stopwatchTime >= timeLimit)
        {
            playerObject.SendMessage("Kill");
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
        playerObject.SendMessage("RemoveAndApplyUpgrades");
    }

    public void EndLevelUp()
    {
        choosingUpgrade = false;
        Time.timeScale = 1f; //Resume the game
        levelUpScreen.SetActive(false);
        ChangeState(GameState.Gameplay);
    }
}
