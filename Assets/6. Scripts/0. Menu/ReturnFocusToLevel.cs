using UnityEngine;
using UnityEngine.EventSystems;

public class ReturnFocusToLevel : MonoBehaviour, ICancelHandler
{
    // Вызывается при нажатии Escape/Cancel на кнопке "SELECT LEVEL"
    public void OnCancel(BaseEventData eventData)
    {
        if (SceneController.LastSelectedLevelButton != null)
        {
            // Устанавливаем фокус обратно на сохраненную кнопку уровня
            EventSystem.current.SetSelectedGameObject(SceneController.LastSelectedLevelButton);
            // SceneController.LastSelectedLevelButton = null; // Опционально: очистить ссылку после использования
        }
    }
}
