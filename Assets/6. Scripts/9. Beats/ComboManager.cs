using UnityEngine;
using System;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;
    public event Action<int, int> OnComboChanged;

    public int CurrentCombo { get; private set; } = 0;
    public int CurrentMultiplier { get; private set; } = 1;

    private bool _hasActedThisBeat = false; // Совершил ли игрок действие в текущем бите

    void Awake() => Instance = this;

    void Start()
    {
        // Подписываемся на событие бита, чтобы проверять пропуски
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat += HandleBeatUpdate;
        }
    }

    public void AddCombo()
    {
        _hasActedThisBeat = true; // Фиксируем успех
        CurrentCombo++;

        int newMultiplier = 1;
        if (CurrentCombo >= 30) newMultiplier = 4;
        else if (CurrentCombo >= 20) newMultiplier = 3;
        else if (CurrentCombo >= 10) newMultiplier = 2;

        CurrentMultiplier = newMultiplier;
        OnComboChanged?.Invoke(CurrentCombo, CurrentMultiplier);
    }

    public void ResetCombo()
    {
        if (CurrentCombo == 0) return;

        CurrentCombo = 0;
        CurrentMultiplier = 1;
        OnComboChanged?.Invoke(CurrentCombo, CurrentMultiplier);
        Debug.Log("COMBO RESET");
    }

    private void HandleBeatUpdate()
    {
        // Не сбрасываем, если музыка только началась (первые 0.5 сек)
        if (BeatConductor.Instance.musicSource.time < 0.5f) return;

        if (!_hasActedThisBeat && CurrentCombo > 0)
        {
            ResetCombo();
        }
        _hasActedThisBeat = false;
    }

    private void OnDestroy()
    {
        if (BeatConductor.Instance != null)
            BeatConductor.Instance.OnBeat -= HandleBeatUpdate;
    }
}
