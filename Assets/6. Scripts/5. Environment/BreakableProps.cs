using UnityEngine;

public class BreakableProps : MonoBehaviour
{
// Добавляем публичную переменную, которую можно менять в Unity Inspector
    public bool isImmortal = false; 

    public float health;
    
    public void TakeDamage (float dmg)
    {
        // Добавляем проверку: если объект бессмертен, 
        // мы просто выходим из функции и не наносим урон
        if (isImmortal)
        {
            return; 
        }

        health -= dmg;

        if(health <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        Destroy(gameObject);
    }
}
