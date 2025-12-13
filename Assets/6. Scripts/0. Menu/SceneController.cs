using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; 
using UnityEditor; 
using UnityEngine.UI; // !!! Добавлено для доступа к компоненту Button !!!

[System.Obsolete("This is an obsolete class, and will be replaced by UILevelSelect")]
public class SceneController : MonoBehaviour
{
    // --- Переменные для управления фокусом и интерактивностью ---
    // !!! Вернули публичную переменную firstSelectedButton !!!
    public GameObject firstSelectedButton; 
    private GameObject lastSelectedGameObject;

    [Header("UI Objects References")]
    public GameObject instructionsScreenPanel; // Перетащите сюда объект 'Instructions Screen'
    public GameObject closeInstructionsButton; // Перетащите сюда кнопку 'Close Instructions'
    public Button startButton;               // Перетащите сюда кнопку 'Start' (Компонент Button)
    public Button exitButton;                // Перетащите сюда кнопку 'Exit' (Компонент Button)
    public Button instructionsButton;        // Перетащите сюда кнопку 'Instructions Button' (Компонент Button)

    // !!! Вернули метод Start() для инициализации фокуса !!!
    void Start()
    {
        // Принудительно выбираем первый элемент при старте сцены и сохраняем его как последний выбранный
        EventSystem.current.SetSelectedGameObject(null); 
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        lastSelectedGameObject = firstSelectedButton; 
    }

    // Метод Update() для восстановления фокуса (логика немного изменена, чтобы полагаться на Start)
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical") ||
                Input.GetButtonDown("Submit"))
            {
                if (lastSelectedGameObject != null)
                {
                    EventSystem.current.SetSelectedGameObject(lastSelectedGameObject);
                }
                // else блок удален, так как Start() гарантирует инициализацию
            }
        }
        else
        {
            lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        }
    }

    // --- Функции управления сценами и выходом ---
    public void SceneChange(string name)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(name);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Выход из игры...");

        if (Application.isEditor)
        {
            EditorApplication.isPlaying = false;
        }
    }
    
    // --- Функции управления экраном инструкций и блокировкой кнопок ---

    public void ShowInstructions()
    {
        if (instructionsScreenPanel != null)
        {
            instructionsScreenPanel.SetActive(true);
            SetMainButtonsInteractable(false); // Блокируем основные кнопки

            // Принудительно устанавливаем фокус на кнопку закрытия
            EventSystem.current.SetSelectedGameObject(null);
            if (closeInstructionsButton != null)
            {
                EventSystem.current.SetSelectedGameObject(closeInstructionsButton);
            }
        }
    }

    public void HideInstructions()
    {
        if (instructionsScreenPanel != null)
        {
            instructionsScreenPanel.SetActive(false);
            SetMainButtonsInteractable(true); // Разблокируем основные кнопки

            // Возвращаем фокус на кнопку, которая открыла инструкции
            EventSystem.current.SetSelectedGameObject(null);
            if (instructionsButton != null)
            {
                EventSystem.current.SetSelectedGameObject(instructionsButton.gameObject);
            }
        }
    }

    private void SetMainButtonsInteractable(bool state)
    {
        if (startButton != null) startButton.interactable = state;
        if (exitButton != null) exitButton.interactable = state;
        if (instructionsButton != null) instructionsButton.interactable = state;
    }
}
