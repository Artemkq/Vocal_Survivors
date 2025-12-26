using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class MusicLayerManager : MonoBehaviour
{
    public static MusicLayerManager Instance;
    public AudioMixer mixer;

    [Header("Настройки плавности")]
    [Tooltip("Скорость изменения громкости (чем выше, тем резче вступает инструмент)")]
    public float fadeSpeed = 50f;

    [Header("Источники звука (Stems)")]
    public AudioSource bassSource;
    public AudioSource drumsSource;
    public AudioSource otherSource;
    public AudioSource vocalsSource;

    // Словарь для хранения целевых значений громкости каждого слоя
    private Dictionary<string, float> _targetVolumes = new Dictionary<string, float>();
    private string[] _params = { "vocalsVol", "otherVol", "bassVol", "drumsVol" };

    void Awake()
    {
        Instance = this;
        // Инициализируем целевые значения (по умолчанию всё на нуле)
        foreach (var p in _params) _targetVolumes[p] = 0f;
    }

    void Start()
    {
        // Пример начальной настройки (барабаны выключены, остальное можно включить)
        SetLayer("vocalsVol", true);
        SetLayer("otherVol", true);
        SetLayer("bassVol", true);
        SetLayer("drumsVol", true); // ИЗМЕНЕНО: ставим true вместо false
        SetMasterVolume(0f);
    }

    void Update()
    {
        // Плавно двигаем текущую громкость в микшере к целевой
        foreach (var p in _params)
        {
            float currentVol;
            mixer.GetFloat(p, out currentVol);

            // Плавный переход
            float nextVol = Mathf.MoveTowards(currentVol, _targetVolumes[p], fadeSpeed * Time.deltaTime);
            mixer.SetFloat(p, nextVol);
        }
    }

    public void SetLayer(string parameterName, bool active)
    {
        // Теперь мы не меняем громкость сразу, а ставим "цель" для Update
        _targetVolumes[parameterName] = active ? 0f : -80f;
    }

    public void SetMasterVolume(float db)
    {
        mixer.SetFloat("MasterVolume", db);
    }

    // --- МЕТОДЫ УПРАВЛЕНИЯ ПАУЗОЙ ---
    public void PauseMusic()
    {
        bassSource.Pause(); drumsSource.Pause(); otherSource.Pause(); vocalsSource.Pause();
    }

    public void UnPauseMusic()
    {
        if (!bassSource.isPlaying)
        {
            bassSource.UnPause(); drumsSource.UnPause(); otherSource.UnPause(); vocalsSource.UnPause();
        }
    }

    public void StopMusic()
    {
        // Выключаем физическое воспроизведение всех источников
        if (bassSource != null) bassSource.Stop();
        if (drumsSource != null) drumsSource.Stop();
        if (otherSource != null) otherSource.Stop();
        if (vocalsSource != null) vocalsSource.Stop();

        // Сбрасываем целевые значения в "тишину", 
        // чтобы при следующем запуске музыка не "всплывала" из старых настроек
        foreach (var p in _params) _targetVolumes[p] = -80f;
    }
}
