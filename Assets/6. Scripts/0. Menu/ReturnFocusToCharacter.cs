using UnityEngine;
using UnityEngine.EventSystems;

// Скрипт для обработки Escape на кнопке "Select Character"
public class ReturnFocusToCharacter : MonoBehaviour, ICancelHandler
{
    // Этот метод вызывается, когда активный UI-элемент получает событие Cancel (по умолчанию Escape или B на геймпаде)
    public void OnCancel(BaseEventData eventData)
    {
        Debug.Log("Escape/Cancel pressed on Select Character button. Returning focus.");

        // Проверяем, сохранили ли мы ранее кнопку персонажа
        if (SceneController.LastSelectedCharacterButton != null)
        {
            // Устанавливаем фокус обратно на кнопку персонажа
            EventSystem.current.SetSelectedGameObject(SceneController.LastSelectedCharacterButton);
            
            // Опционально: очищаем ссылку после использования, если нужно
            // SceneController.LastSelectedCharacterButton = null; 
        }
    }
}