// Отвечает за пульсацию объекта в такт музыки

using UnityEngine;

public class BeatPulser : MonoBehaviour
{
    [Header("Настройки пульсации")]
    public float pulseSize = 1.15f;    // На сколько увеличится объект (1.15 = +15%)
    public float returnSpeed = 10f;    // Скорость возврата к обычному размеру

    private Vector3 _nativeScale;      // Исходный размер объекта

    void Start()
    {
        // Запоминаем исходный размер объекта при старте
        _nativeScale = transform.localScale;

        // Подписываемся на событие бита. 
        // Когда BeatConductor скажет "Бит!", сработает метод Pulse.
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat += Pulse;
        }
    }

    // Метод, который срабатывает строго в бит
    void Pulse()
    {
        // Резко увеличиваем объект
        transform.localScale = _nativeScale * pulseSize;
    }

    void Update()
    {
        // Плавно возвращаем размер к исходному каждый кадр
        transform.localScale = Vector3.Lerp(transform.localScale, _nativeScale, Time.deltaTime * returnSpeed);
    }

    void OnDestroy()
    {
        // Обязательно отписываемся от события при удалении объекта, 
        // чтобы не возникало ошибок (особенно важно для врагов)
        if (BeatConductor.Instance != null)
        {
            BeatConductor.Instance.OnBeat -= Pulse;
        }
    }
}
