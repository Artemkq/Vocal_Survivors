using UnityEngine;

public class DrumWave : Aura
{
    public float lifeTime = 0.4f;
    private ParticleSystem ps;

    void Awake()
    {
        // Ищем систему частиц на объекте или в дочерних
        ps = GetComponent<ParticleSystem>();
        if (ps == null) ps = GetComponentInChildren<ParticleSystem>();
    }

    public void SetupWave(float radius, Color color)
    {
        // Устанавливаем радиус коллайдера/ауры
        transform.localScale = new Vector3(radius, radius, 1);

        if (ps != null)
        {
            var main = ps.main;
            main.startColor = color; // Устанавливаем цвет частиц (красный или белый)
            ps.Play(); // Запускаем воспроизведение
        }
        else
        {
            Debug.LogWarning("ParticleSystem не найдена на префабе DrumWave!");
        }

        // Удаляем объект после завершения эффекта
        Destroy(gameObject, lifeTime);
    }
}
