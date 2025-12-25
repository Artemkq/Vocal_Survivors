using UnityEngine;

public class Pickup : Sortable
{
    public float lifespan = 0.5f;
    protected PlayerStats target; // Если у подбираемого предмета есть цель, он летит к ней
    protected float speed;        // Скорость полета предмета к игроку

    [Header("Bonuses")]
    public int experience;
    public int health;

    protected override void Start()
    {
        base.Start();
        // Логика инициализации позиции удалена, так как предмет теперь статичен до подбора
    }

    protected virtual void Update()
    {
        if (target)
        {
            // Движение в сторону игрока
            Vector2 distance = target.transform.position - transform.position;
            if (distance.sqrMagnitude > speed * speed * Time.deltaTime)
                transform.position += (Vector3)distance.normalized * speed * Time.deltaTime;
            else
                Destroy(gameObject);
        }
        // Блок else с анимацией Bobbing удален
    }

    public virtual bool Collect(PlayerStats target, float speed, float lifespan = 0f)
    {
        if (!this.target)
        {
            this.target = target;
            this.speed = speed;
            if (lifespan > 0f) this.lifespan = lifespan;
            Destroy(gameObject, Mathf.Max(0.01f, this.lifespan));
            return true;
        }
        return false;
    }

    protected virtual void OnDestroy()
    {
        if (!target) return;
        if (experience != 0) target.IncreaseExperience(experience);
        if (health != 0) target.RestoreHealth(health);
    }
}
