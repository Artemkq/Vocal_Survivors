using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag; // Назови как угодно, например "Zombie"
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                // Создаем объекты и сразу делаем их дочерними к пулу, чтобы не мусорить в иерархии
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            // Используем имя префаба как ключ, это надежнее
            string poolKey = pool.prefab.name.Replace("(Clone)", "").Trim();
            poolDictionary.Add(poolKey, objectPool);
        }
    }

    public GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // Убираем "(Clone)" из имени, если оно там есть, для точного совпадения
        string key = prefab.name.Replace("(Clone)", "").Trim();

        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogError($"ОШИБКА: В пуле нет объектов с именем {key}. Проверь настройки PoolManager!");
            return null;
        }

        // Берем объект
        GameObject obj = poolDictionary[key].Dequeue();
        poolDictionary[key].Enqueue(obj);

        // Если объект уже активен (значит, мы израсходовали весь запас), 
        // он просто телепортируется. Если нет — включаем.
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        if (!obj.activeInHierarchy)
        {
            obj.SetActive(true);
        }

        return obj;
    }
}
