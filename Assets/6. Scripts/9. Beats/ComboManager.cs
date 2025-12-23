using UnityEngine;
using System; // Нужно для Action

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    // Событие, на которое подпишется UI
    // Передает текущее комбо и множитель
    public event Action<int, float> OnComboChanged;

    public float comboResetTime = 2f;
    public int CurrentCombo { get; private set; } = 0;
    public float ComboMultiplier { get; private set; } = 1f;
    private float _lastActionTime;

    void Awake()
    {
        Instance = this; // Гарантируем, что Instance создается сразу
    }

    void Update()
    {
        if (CurrentCombo > 0 && Time.time - _lastActionTime > comboResetTime)
        {
            ResetCombo();
        }
    }

    public void AddCombo()
    {
        CurrentCombo++;
        _lastActionTime = Time.time;
        ComboMultiplier = 1f + (CurrentCombo * 0.1f);

        // Вызываем событие
        OnComboChanged?.Invoke(CurrentCombo, ComboMultiplier);

        Debug.Log($"КОМБО X{CurrentCombo}! Множитель: {ComboMultiplier}");
    }

    public void ResetCombo()
    {
        CurrentCombo = 0;
        ComboMultiplier = 1f;

        // Вызываем событие при сбросе
        OnComboChanged?.Invoke(CurrentCombo, ComboMultiplier);

        Debug.Log("КОМБО СБРОШЕНО!");
    }
}
