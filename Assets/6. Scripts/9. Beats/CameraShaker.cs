using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    [Header("Настройки тряски")]
    public float baseShakeIntensity = 0.1f;
    public float shakeDecay = 10f;

    private float _currentShakeIntensity;
    private Vector3 _shakeOffset; // Смещение, которое мы прибавим к позиции

    void Start()
    {
        if (BeatConductor.Instance != null)
            BeatConductor.Instance.OnBeat += TryShake;
    }

    private void OnDestroy()
    {
        if (BeatConductor.Instance != null)
            BeatConductor.Instance.OnBeat -= TryShake;
    }

    private void TryShake()
    {
        if (ComboManager.Instance != null && ComboManager.Instance.CurrentCombo > 0)
        {
            // Рассчитываем новую силу
            float newIntensity = baseShakeIntensity * ComboManager.Instance.ComboMultiplier;

            // Ограничиваем сверху, чтобы на х50 комбо камера не улетела
            float maxIntensity = 0.5f;

            // Вместо простого присваивания, выбираем максимальное из текущей и новой силы
            // Это предотвращает "дерганность", если бит наложился на затухание
            _currentShakeIntensity = Mathf.Clamp(newIntensity, 0, maxIntensity);
        }
    }

    void LateUpdate() // Используем LateUpdate для работы после систем движения
    {
        if (_currentShakeIntensity > 0)
        {
            // Считаем только смещение
            _shakeOffset = Random.insideUnitSphere * _currentShakeIntensity;

            // Применяем смещение к текущей позиции
            transform.localPosition += _shakeOffset;

            // Затухание
            _currentShakeIntensity = Mathf.Lerp(_currentShakeIntensity, 0f, Time.deltaTime * shakeDecay);
        }
    }
}
