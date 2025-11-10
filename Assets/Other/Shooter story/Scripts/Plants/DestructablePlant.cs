using System;
using UnityEngine;

public class DestructablePlant : MonoBehaviour
{

    public event EventHandler OnDestructableTakeDamage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Sword>())
        {
            OnDestructableTakeDamage?.Invoke(this, EventArgs.Empty);
            Destroy(gameObject);

            NavMeshSurfaceManagement.Instance.RebakeNavmeshSurface();
        }
    }

}
