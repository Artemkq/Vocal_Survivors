using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Define the different states of the game
    public enum GameState
    {
        Gameplay,
        Paused,
        GameOver
    }

    // Store the current state of the game
    public GameState currentState;
    // Store the previous state of the game
    public GameState previousState;

    [Header("Screens")]
    public GameObject pauseScreen;
    public GameObject resultScreen;

    [Header("Current Stat Displays")]
    public Text currentHealthDisplay;
    public Text currentRecoveryDisplay;
    public Text currentMoveSpeedDisplay;
    public Text currentMightDisplay;
    public Text currentProjectileSpeedDisplay;
    public Text currentMagnetDisplay;

    [Header("Results Screen Displays")]
    public Image chosenCharacterImage;
    public Text chosenCharacterName;
    public Text levelReachedDisplay;
    public List<Image> chosenWeaponsUI = new List<Image>(6);
    public List<Image> chosenPassiveItemsUI = new List<Image>(6);

    //Flag to check if the game over
    public bool isGameOver = false;

    void Awake()
    {
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


    void Update()
    {
        // Define the behaviour for each state

        switch (currentState)
        {
            case GameState.Gameplay:
                // Code for the gameplay state
                CheckForPauseAndResume();
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

            default:
                Debug.LogWarning("STATE DOES NOT EXIST");
                break;
        }
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
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
    }

    void DisplayResults()
    {
        resultScreen.SetActive(true);
    }

    public void AssignChosenCharacterUI(CharacterScriptableObject chosenCharacterData)
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
            Debug.Log("Chosen weapons and passive items data lists have different lengths");
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
}
