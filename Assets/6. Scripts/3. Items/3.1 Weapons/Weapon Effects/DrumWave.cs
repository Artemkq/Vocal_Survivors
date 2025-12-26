using UnityEngine;

public class DrumWave : MonoBehaviour
{
    public float lifeTime = 0.5f;
    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null) ps = GetComponentInChildren<ParticleSystem>();
    }

    // Убрали параметр Color из метода
    public void SetupWave(float radius)
    {
        transform.localScale = new Vector3(radius, radius, 1);

        if (ps != null)
        {
            // Больше не меняем main.startColor, частицы используют цвет из префаба
            ps.Play();
        }

        Destroy(gameObject, lifeTime);
    }
}
