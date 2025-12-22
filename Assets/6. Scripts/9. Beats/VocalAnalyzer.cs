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
        if (Microphone.devices.Length > 0) {
            _micClip = Microphone.Start(null, true, 1, 44100);
        } else {
            Debug.LogWarning("Микрофон не найден!");
        }
    }

    void Update()
    {
        CurrentLoudness = GetLoudness() * sensitivity;
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
