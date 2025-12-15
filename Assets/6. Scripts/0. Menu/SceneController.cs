using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.UI; // !!! Добавлено для доступа к компоненту Button !!!

public class SceneController : MonoBehaviour
{
    // --- Переменные для управления фокусом и интерактивностью ---
    // !!! Вернули публичную переменную firstSelectedButton !!!
    public GameObject firstSelectedButton;
    private GameObject lastSelectedGameObject;
    public static GameObject LastSelectedCharacterButton;
    public static GameObject LastSelectedLevelButton;


    [Header("For Title Screen Options")]
    public GameObject instructionsScreenPanel; // Перетащите сюда объект 'Instructions Screen'
    public GameObject closeInstructionsButton; // Перетащите сюда кнопку 'Close Instructions'
    public Button startButton;               // Перетащите сюда кнопку 'Start' (Компонент Button)
    public Button exitButton;                // Перетащите сюда кнопку 'Exit' (Компонент Button)
    public Button instructionsButton;        // Перетащите сюда кнопку 'Instructions Button' (Компонент Button)
    public GameObject exitConfirmationPanel; // Ссылка на панель (объект) ExitConfirmationPanel
    public GameObject firstSelectedExitButton; // Ссылка на кнопку "Нет" (NoButton), которая будет первой в фокусе
    // Дополнительная переменная для хранения кнопки, которая была активна перед открытием *любого* окна
    private GameObject buttonBeforePanelOpened;

    // !!! Вернули метод Start() для инициализации фокуса !!!
    void Start()
    {
        // Принудительно выбираем первый элемент при старте сцены и сохраняем его как последний выбранный
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        lastSelectedGameObject = firstSelectedButton;

        // Убедимся, что обе панели скрыты при старте
        if (instructionsScreenPanel != null) instructionsScreenPanel.SetActive(false);
        if (exitConfirmationPanel != null) exitConfirmationPanel.SetActive(false);
    }

    // Обновленный метод Update(), включая логику обработки Escape
    void Update()
    {
        // Логика восстановления фокуса (была и раньше)
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical") ||
                Input.GetButtonDown("Submit"))
            {
                if (lastSelectedGameObject != null)
                {
                    EventSystem.current.SetSelectedGameObject(lastSelectedGameObject);
                }
            }
        }
        else
        {
            lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        }

        // Обработка Escape: Сначала проверяем, активно ли окно подтверждения выхода
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (exitConfirmationPanel != null && exitConfirmationPanel.activeInHierarchy)
            {
                HideExitConfirmation(); // Закрыть окно подтверждения
            }
            else if (instructionsScreenPanel != null && instructionsScreenPanel.activeInHierarchy)
            {
                HideInstructions(); // Закрыть инструкции
            }
            // Можно добавить логику, чтобы при нажатии Escape на главном меню открывалось окно подтверждения выхода
            else { ShowExitConfirmation(); }
        }
    }

    // --- Функции управления сценами и выходом ---
    public void SceneChange(string name)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(name);
    }

    // !!! ИЗМЕНЕННАЯ ФУНКЦИЯ ExitGame() !!!
    // Теперь она не выходит сразу, а показывает окно подтверждения (если оно настроено)
    public void ExitGame()
    {
        // Вместо немедленного выхода, вызываем функцию показа окна подтверждения
        ShowExitConfirmation();
        Debug.Log("Попытка выхода из игры, ожидание подтверждения...");
    }

    // !!! НОВАЯ ФУНКЦИЯ ДЛЯ ПОДТВЕРЖДЕННОГО ВЫХОДА !!!
    public void ConfirmExitGame()
    {
        Application.Quit();
        Debug.Log("Выход из игры подтвержден...");

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
            // !!! НОВОЕ: Сохраняем кнопку перед открытием инструкций !!!
            buttonBeforePanelOpened = EventSystem.current.currentSelectedGameObject;

            instructionsScreenPanel.SetActive(true);
            SetMainButtonsInteractable(false);
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
            SetMainButtonsInteractable(true);
            EventSystem.current.SetSelectedGameObject(null);

            // !!! ИЗМЕНЕНО: Возвращаем фокус на сохраненную кнопку или на кнопку инструкций по умолчанию !!!
            if (buttonBeforePanelOpened != null)
            {
                EventSystem.current.SetSelectedGameObject(buttonBeforePanelOpened);
            }
            else if (instructionsButton != null)
            {
                EventSystem.current.SetSelectedGameObject(instructionsButton.gameObject);
            }
        }
    }

    // --- НОВЫЕ ФУНКЦИИ УПРАВЛЕНИЯ ОКНОМ ПОДТВЕРЖДЕНИЯ ---

    public void ShowExitConfirmation()
    {
        if (exitConfirmationPanel != null)
        {
            // !!! НОВОЕ: Сохраняем кнопку перед открытием окна выхода !!!
            // lastSelectedGameObject уже содержит нужный нам объект (кнопку Exit или другую)
            buttonBeforePanelOpened = EventSystem.current.currentSelectedGameObject;

            exitConfirmationPanel.SetActive(true);
            SetMainButtonsInteractable(false);

            EventSystem.current.SetSelectedGameObject(null);
            if (firstSelectedExitButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedExitButton);
            }
        }
    }

    public void HideExitConfirmation()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
            SetMainButtonsInteractable(true);

            // !!! ИЗМЕНЕНО: Возвращаем фокус на сохраненную кнопку (которая была до открытия окна) !!!
            EventSystem.current.SetSelectedGameObject(null);
            if (buttonBeforePanelOpened != null)
            {
                EventSystem.current.SetSelectedGameObject(buttonBeforePanelOpened);
            }
            else if (exitButton != null) // Fallback на случай, если что-то пошло не так
            {
                EventSystem.current.SetSelectedGameObject(exitButton.gameObject);
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
