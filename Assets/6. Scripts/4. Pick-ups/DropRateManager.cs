using System.Collections.Generic;
using UnityEngine;

public class DropRateManager : MonoBehaviour
{
    [System.Serializable]
    public class Drops
    {
        public string name;
        public GameObject itemPrefab;
        public float dropRate;
    }

    public bool active = false;
    public List<Drops> drops;

    // ПЕРЕИМЕНОВАЛИ ИЗ OnDestroy В ПУБЛИЧНЫЙ МЕТОД
    public void GenerateDrops()
    {
        // Если флаг active выключен (например, враг ушел за экран), ничего не спавним
        if (!active) return;

        float randomNumber = Random.Range(0f, 100f);

        // Оптимизация: вместо создания нового списка просто ищем подходящий дроп
        Drops selectedDrop = null;
        foreach (Drops rate in drops)
        {
            if (randomNumber <= rate.dropRate && rate.itemPrefab != null)
            {
                selectedDrop = rate;
                break; // Берем первый подошедший или уберите break, если логика сложнее
            }
        }

        if (selectedDrop != null && selectedDrop.itemPrefab != null)
        {
            Instantiate(selectedDrop.itemPrefab, transform.position, Quaternion.identity);
        }
    }

    // Оставляем пустой OnDestroy на случай, если вы удалите объект вручную из редактора
    void OnDestroy() { }
}
