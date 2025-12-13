using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    // Переименовываем поля, чтобы избежать конфликтов
    private AudioSource _audioSource;
    private ParticleSystem _particleSystem;

    void Start()
    {
        // Присваиваем компоненты новым переменным
        _audioSource = GetComponent<AudioSource>();
        _particleSystem = GetComponent<ParticleSystem>();

        // Запускаем звук, если он есть
        if (_audioSource != null && _audioSource.clip != null)
        {
            _audioSource.Play();
            // Используем новые переменные в расчетах
            float duration = Mathf.Max(_audioSource.clip.length, _particleSystem.main.duration);
            Destroy(gameObject, duration);
        }
        else
        {
            // Если звука нет, уничтожаем объект по окончании частиц
            Destroy(gameObject, _particleSystem.main.duration);
        }
    }
}