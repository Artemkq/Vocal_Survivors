// Отвечает за управление звуковыми слоями через AudioMixer.

using UnityEngine;
using UnityEngine.Audio;

public class SoundLayerController : MonoBehaviour
{
    public AudioMixer mainMixer;

    // Метод для плавного включения слоя (значение от -80 до 0 децибел)
    public void SetLayerVolume(string parameterName, bool active)
    {
        float targetVolume = active ? 0f : -80f;
        // Плавно меняем громкость через Mixer
        mainMixer.SetFloat(parameterName, targetVolume);
    }
}
