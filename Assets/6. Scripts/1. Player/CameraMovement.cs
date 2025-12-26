using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    [Header("Настройки плавности")]
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.125f;

    // Используем FixedUpdate для камеры, если игрок на физике, 
    // НО в 2025 году лучше оставить LateUpdate с коррекцией:
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // Используем Time.smoothDeltaTime для компенсации дерганий
        // Это делает движение вязким, но стабильным
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * (Time.smoothDeltaTime * 60f));
    }
}
