using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FocusToSelectLevelButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    [Header("Select Level Button")]
    // Ссылка на кнопку "SELECT LEVEL" (назначьте через Inspector)
    public Button selectLevelButton;

    private void TransferFocus()
    {
        if (selectLevelButton != null)
        {
            // Сохраняем ТЕКУЩИЙ объект (кнопка уровня) в статическую переменную SceneController
            SceneController.LastSelectedLevelButton = this.gameObject;

            EventSystem.current.SetSelectedGameObject(null);
            // Устанавливаем фокус на целевую кнопку "SELECT LEVEL"
            EventSystem.current.SetSelectedGameObject(selectLevelButton.gameObject);
        }
    }

    public void OnPointerClick(PointerEventData eventData) => TransferFocus();
    public void OnSubmit(BaseEventData eventData) => TransferFocus();
}
