using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    [System.Serializable]
    public struct ShakeSettings
    {
        public float intensity; // Сила тряски
        public float decay;     // Скорость затухания (чем меньше, тем дольше тряска)
    }

    [Header("Настройки порогов")]
    public ShakeSettings settings10 = new ShakeSettings { intensity = 0.1f, decay = 20f };
    public ShakeSettings settings25 = new ShakeSettings { intensity = 0.2f, decay = 15f };
    public ShakeSettings settings50 = new ShakeSettings { intensity = 0.4f, decay = 10f };
    public ShakeSettings settings100 = new ShakeSettings { intensity = 0.7f, decay = 7f };

    private float _currentIntensity;
    private float _currentDecay;
    private Vector3 _originalLocalPos;

    void Start()
    {
        _originalLocalPos = transform.localPosition;
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
        if (ComboManager.Instance == null) return;

        int combo = ComboManager.Instance.CurrentCombo;
        ShakeSettings currentSet = new ShakeSettings { intensity = 0, decay = 0 };

        // Тряска при переходе на новые уровни множителя (GH3 Style)
        if (combo == 10) currentSet = settings10;      // Переход на x2
        else if (combo == 20) currentSet = settings25; // Переход на x3
        else if (combo == 30) currentSet = settings50; // Переход на x4
        else if (combo >= 80 && (combo - 30) % 50 == 0) // Каждые 50 после выхода на макс
        {
            currentSet = settings100;
        }

        if (currentSet.intensity > 0)
        {
            _currentIntensity = currentSet.intensity;
            _currentDecay = currentSet.decay;
        }
    }

    void LateUpdate()
    {
        if (_currentIntensity > 0)
        {
            Vector2 shakePoint = Random.insideUnitCircle * _currentIntensity;
            transform.localPosition = _originalLocalPos + new Vector3(shakePoint.x, shakePoint.y, 0);

            // Затухание теперь использует динамический _currentDecay
            _currentIntensity = Mathf.Lerp(_currentIntensity, 0f, Time.deltaTime * _currentDecay);
        }
        else
        {
            transform.localPosition = _originalLocalPos;
        }
    }
}
