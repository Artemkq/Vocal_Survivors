using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI;

// Скрипт для немедленного переноса фокуса на другую кнопку
public class FocusToCharacterSelectButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    [Header("Select Button")]
    // Ссылка на кнопку "SELECT CHARACTER", которую нужно назначить через Inspector
    public Button selectCharacterButton;

    // Этот метод вызывается при клике мышью или нажатии Enter/Submit
    private void TransferFocus()
    {
        if (selectCharacterButton != null)
        {
            Debug.Log("Selected " + gameObject.name + ". Transferring focus to Select Button.");

            // !!! НОВАЯ ЛОГИКА !!!
            // Сохраняем ТЕКУЩИЙ объект (кнопка персонажа) в статическую переменную SceneController
            SceneController.LastSelectedCharacterButton = this.gameObject;

            // Сначала сбрасываем текущий выделенный объект
            EventSystem.current.SetSelectedGameObject(null);

            // Затем устанавливаем фокус на целевую кнопку "SELECT CHARACTER"
            EventSystem.current.SetSelectedGameObject(selectCharacterButton.gameObject);
        }
        else
        {
            Debug.LogWarning("Select Character Button is not assigned in the Inspector for " + gameObject.name);
        }
    }
public void OnPointerClick(PointerEventData eventData)
    {
        TransferFocus();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        TransferFocus();
    }
}
