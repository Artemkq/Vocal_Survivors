using UnityEngine;
using UnityEngine.EventSystems; // Добавьте это using

public class UIFocusHelper : MonoBehaviour
{
    // Публичное поле, куда вы перетащите целевую кнопку в инспекторе
    public GameObject targetButton;

    // Функция, которую мы вызовем из события OnClick() другой кнопки
    public void SetFocusOnTarget()
    {
        // Устанавливает фокус на указанный объект targetButton
        EventSystem.current.SetSelectedGameObject(targetButton);
    }
}
