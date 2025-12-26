using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UINavigationHelper : MonoBehaviour, IPointerClickHandler, ISubmitHandler, ICancelHandler
{
    public enum SelectionType { None, Character, Level }

    [Header("Forward Navigation")]
    public SelectionType selectionType = SelectionType.None;
    public Selectable targetElement;

    [Header("Backward Navigation")]
    [Tooltip("Куда вернуться (в меню), если мы уже на самом персонаже/уровне")]
    public Selectable returnToDefault;

    public void OnPointerClick(PointerEventData eventData) => TransferFocus();
    public void OnSubmit(BaseEventData eventData) => TransferFocus();

    public void OnCancel(BaseEventData eventData)
    {
        GameObject objectToFocus = null;

        if (selectionType == SelectionType.Character)
        {
            // Если мы на кнопке "Выбрать", возвращаемся к сохраненному Toggle персонажа
            if (SceneController.LastSelectedCharacterButton != null && SceneController.LastSelectedCharacterButton != this.gameObject)
            {
                objectToFocus = SceneController.LastSelectedCharacterButton;
            }
            else // Иначе выходим в меню
            {
                objectToFocus = returnToDefault != null ? returnToDefault.gameObject : null;
            }
        }
        else if (selectionType == SelectionType.Level)
        {
            // Если мы на кнопке "Старт", возвращаемся к сохраненному Toggle уровня
            if (SceneController.LastSelectedLevelButton != null && SceneController.LastSelectedLevelButton != this.gameObject)
            {
                objectToFocus = SceneController.LastSelectedLevelButton;
            }
            else // Иначе выходим в меню
            {
                objectToFocus = returnToDefault != null ? returnToDefault.gameObject : null;
            }
        }
        else if (returnToDefault != null)
        {
            objectToFocus = returnToDefault.gameObject;
        }

        if (objectToFocus != null) ApplyFocus(objectToFocus);
    }

    private void TransferFocus()
    {
        // Сохраняем текущий объект перед переходом
        if (selectionType == SelectionType.Character)
            SceneController.LastSelectedCharacterButton = this.gameObject;
        else if (selectionType == SelectionType.Level)
            SceneController.LastSelectedLevelButton = this.gameObject;

        if (targetElement != null)
        {
            ApplyFocus(targetElement.gameObject);
        }
    }

    private void ApplyFocus(GameObject obj)
    {
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(obj);

        var selectable = obj.GetComponent<Selectable>();
        if (selectable != null) selectable.OnSelect(null);
    }
}
