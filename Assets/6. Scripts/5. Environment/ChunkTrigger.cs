using UnityEngine;

public class ChunkTrigger : MonoBehaviour
{
    private MapController mc;
    public GameObject targetMap; // Ссылка на корневой объект чанка

    void Start()
    {
        mc = Object.FindAnyObjectByType<MapController>();
        
        // Если таргет не назначен вручную, берем родителя, 
        // так как триггер обычно находится внутри чанка
        if (targetMap == null)
        {
            targetMap = transform.parent.gameObject;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // Меняем текущий чанк только если он действительно изменился
            if (mc.currentChunk != targetMap)
            {
                mc.currentChunk = targetMap;
                // Debug.Log($"Игрок перешел на чанк: {targetMap.name}");
            }
        }
    }

    // Метод Exit удален специально, чтобы избежать "мертвых зон" 
    // при переходе между триггерами чанков.
}
