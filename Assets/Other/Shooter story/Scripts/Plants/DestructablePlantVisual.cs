using UnityEngine;

public class DestructablePlantVisual : MonoBehaviour
{

    [SerializeField] private DestructablePlant destructablePlant;
    [SerializeField] private GameObject bushDeathVFXPrefab;

    private void Start()
    {
        destructablePlant.OnDestructableTakeDamage += DestructablePlant_OnDestructableTakeDamage;
    }

    private void DestructablePlant_OnDestructableTakeDamage(object sender, System.EventArgs e)
    {
        ShowDeathVFX();
    }

    private void ShowDeathVFX()
    {
        Instantiate(bushDeathVFXPrefab, destructablePlant.transform.position, Quaternion.identity);
    }

    private void OnDestroy()
    {
        destructablePlant.OnDestructableTakeDamage -= DestructablePlant_OnDestructableTakeDamage;
    }

}
