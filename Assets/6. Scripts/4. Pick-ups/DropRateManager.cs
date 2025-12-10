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

    void OnDestroy()
    {
        // *** КЛЮЧЕВАЯ ПРОВЕРКА: Если active == false, мы выходим и опыт не спавнится ***

        if (!active) return; //Prevents spawns from happening if inactive
        if (!gameObject.scene.isLoaded) //Stops the spawning error from appearing when stopping play mode
        {
            return;
        }

        float randomNumber = UnityEngine.Random.Range(0f, 100f);
        List<Drops> possibleDrops = new List<Drops>();

        foreach (Drops rate in drops)
        {
            if (randomNumber <= rate.dropRate && rate.itemPrefab != null)
            {
                possibleDrops.Add(rate);
            }
        }
        //Check if there are possible drops
        if (possibleDrops.Count > 0)
        {
            Drops drops = possibleDrops[UnityEngine.Random.Range(0, possibleDrops.Count)];

            if (drops.itemPrefab != null)
            {
                Instantiate(drops.itemPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Item prefab is missing for drop: " + drops.name);
            }
        }
    }
}
