using UnityEngine;
using UnityEngine.Audio;

public class MusicLayerManager : MonoBehaviour
{
    public static MusicLayerManager Instance;
    public AudioMixer mixer;

    [Header("Источники звука (Stems)")]
    public AudioSource bassSource;
    public AudioSource drumsSource;
    public AudioSource otherSource;
    public AudioSource vocalsSource;

    void Awake() => Instance = this;

    void Start()
    {
        SetLayer("vocalsVol", true);
        SetLayer("otherVol", true);
        SetLayer("bassVol", true);
        SetLayer("drumsVol", true);
        SetMasterVolume(0f);
    }

    // ... (Метод Update остается тем же) ...
    void Update()
    {
        if (VocalAnalyzer.Instance != null)
        {
            bool isScreaming = VocalAnalyzer.Instance.CurrentLoudness > 0.5f;
            float currentVocVol;
            mixer.GetFloat("vocalsVol", out currentVocVol);
            float nextVol = Mathf.Lerp(currentVocVol, isScreaming ? 0f : -80f, Time.deltaTime * 5f);
            mixer.SetFloat("vocalsVol", nextVol);
        }
    }


    public void SetLayer(string parameterName, bool active)
    {
        float targetVol = active ? 0f : -80f;
        mixer.SetFloat(parameterName, targetVol);
    }

    public void SetMasterVolume(float db)
    {
        mixer.SetFloat("MasterVolume", db);
    }

    // --- НОВЫЕ МЕТОДЫ УПРАВЛЕНИЯ ПАУЗОЙ ---
    public void PauseMusic()
    {
        // Используем метод Pause() для остановки воспроизведения
        bassSource.Pause();
        drumsSource.Pause();
        otherSource.Pause();
        vocalsSource.Pause();
    }

    public void UnPauseMusic()
    {
        // Используем метод UnPause() для возобновления
        if (!bassSource.isPlaying) bassSource.UnPause();
        if (!drumsSource.isPlaying) drumsSource.UnPause();
        if (!otherSource.isPlaying) otherSource.UnPause();
        if (!vocalsSource.isPlaying) vocalsSource.UnPause();
    }

    public void StopMusic()
    {
        // Используем метод Stop() для полной остановки (например, при GameOver)
        bassSource.Stop();
        drumsSource.Stop();
        otherSource.Stop();
        vocalsSource.Stop();
    }
}
