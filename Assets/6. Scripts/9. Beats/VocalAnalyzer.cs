using UnityEngine;

public class VocalAnalyzer : MonoBehaviour
{
    public static VocalAnalyzer Instance;

    [Header("Настройки микрофона")]
    public float sensitivity = 100f; // Чувствительность
    public float vocalThreshold = 0.05f; // Порог шума

    private AudioClip _micClip;
    public float CurrentLoudness { get; private set; }

    void Awake() { Instance = this; }

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            Debug.Log("Микрофон найден: " + Microphone.devices[0]); // Покажет имя микрофона
            _micClip = Microphone.Start(null, true, 1, 44100);
        }
        else
        {
            Debug.LogError("Микрофон НЕ найден! Подключите устройство.");
        }
    }

    void Update()
    {
        // Проверка: если зажат левый Shift — имитируем крик на максимум
        if (Input.GetKey(KeyCode.LeftShift))
        {
            CurrentLoudness = 1.5f; // "Виртуальный крик"
        }
        else
        {
            // Если микрофон есть — берем данные с него, если нет — будет 0
            if (Microphone.devices.Length > 0)
            {
                CurrentLoudness = GetLoudness() * sensitivity;
            }
            else
            {
                CurrentLoudness = 0;
            }
        }

        // Ограничиваем порог шума
        if (CurrentLoudness < vocalThreshold) CurrentLoudness = 0;
    }

    float GetLoudness()
    {
        float[] waveData = new float[128];
        int micPos = Microphone.GetPosition(null) - 128;
        if (micPos < 0) return 0;
        _micClip.GetData(waveData, micPos);
        float sum = 0;
        foreach (var s in waveData) sum += Mathf.Abs(s);
        return sum / 128;
    }
}
