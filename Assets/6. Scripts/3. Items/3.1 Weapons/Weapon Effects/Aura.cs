using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An aura is a damage-over-time effect that applies to a specific area in timed intervals
/// It is used to give the functionality of Garlic, and it can also be used to spawn holy water effct
/// </summary>

public class Aura : WeaponEffect
{
    // Словари для отслеживания целей и их кулдаунов
    Dictionary<EnemyStats, float> affectedTargets = new Dictionary<EnemyStats, float>();
    Dictionary<BreakableProps, float> affectedProps = new Dictionary<BreakableProps, float>();

    // Списки для временного хранения целей, которые должны быть удалены после завершения цикла Update
    List<EnemyStats> targetsToUnaffect = new List<EnemyStats>();
    List<BreakableProps> propsToUnaffect = new List<BreakableProps>();

    void Update()
    {
        // --- ОБРАБОТКА ВРАГОВ (ENEMYSTATS) ---
        // Создаем временный список ключей, чтобы безопасно изменять словарь во время итерации
        List<EnemyStats> enemiesToProcess = new List<EnemyStats>(affectedTargets.Keys);
        foreach (EnemyStats enemy in enemiesToProcess)
        {
            // Пропускаем, если объект уже невалиден или удален
            if (enemy == null || !affectedTargets.ContainsKey(enemy)) continue;

            affectedTargets[enemy] -= Time.deltaTime;
            if (affectedTargets[enemy] <= 0f)
            {
                if (targetsToUnaffect.Contains(enemy))
                {
                    // Цель вышла из триггера, удаляем её окончательно
                    RemoveEnemy(enemy);
                }
                else
                {
                    // Сбрасываем кулдаун, наносим урон и применяем эффекты
                    ProcessEnemyDamage(enemy);
                }
            }
        }

        // --- ОБРАБОТКА РАЗРУШАЕМЫХ ОБЪЕКТОВ (BREAKABLEPROPS) ---
        List<BreakableProps> propsToProcess = new List<BreakableProps>(affectedProps.Keys);
        foreach (BreakableProps prop in propsToProcess)
        {
            if (prop == null || !affectedProps.ContainsKey(prop)) continue;
            
            affectedProps[prop] -= Time.deltaTime;
            if (affectedProps[prop] <= 0f)
            {
                if (propsToUnaffect.Contains(prop))
                {
                    // Объект вышел из триггера, удаляем его окончательно
                    RemoveProp(prop);
                }
                else
                {
                    // Сбрасываем кулдаун, наносим урон и применяем эффекты
                    ProcessPropDamage(prop);
                }
            }
        }
    }

    private void ProcessEnemyDamage(EnemyStats enemy)
    {
        Weapon.Stats stats = weapon.GetStats();
        affectedTargets[enemy] = stats.cooldown * Owner.Stats.cooldown;
        
        enemy.TakeDamage(GetDamage(), transform.position, stats.knockback);
        weapon.ApplyBuffs(enemy);

        if (stats.hitEffect)
        {
            Destroy(Instantiate(stats.hitEffect, enemy.transform.position, Quaternion.identity), 5f);
        }
    }

    private void RemoveEnemy(EnemyStats enemy)
    {
        affectedTargets.Remove(enemy);
        targetsToUnaffect.Remove(enemy);
    }

    private void ProcessPropDamage(BreakableProps prop)
    {
        Weapon.Stats stats = weapon.GetStats();
        affectedProps[prop] = stats.cooldown * Owner.Stats.cooldown;
        
        prop.TakeDamage(GetDamage());

        if (stats.hitEffect)
        {
            Destroy(Instantiate(stats.hitEffect, prop.transform.position, Quaternion.identity), 5f);
        }
    }

    private void RemoveProp(BreakableProps prop)
    {
        affectedProps.Remove(prop);
        propsToUnaffect.Remove(prop);
    }
    
    // --- ОБРАБОТКА ТРИГГЕРОВ ---

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyStats es))
        {
            if (!affectedTargets.ContainsKey(es))
            {
                affectedTargets.Add(es, 0); // Урон начнется в следующем Update()
            }
            else if (targetsToUnaffect.Contains(es))
            {
                targetsToUnaffect.Remove(es);
            }
        }
        else if (other.TryGetComponent(out BreakableProps p))
        {
            if (!affectedProps.ContainsKey(p))
            {
                affectedProps.Add(p, 0); // Урон начнется в следующем Update()
            }
            else if (propsToUnaffect.Contains(p))
            {
                propsToUnaffect.Remove(p);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyStats es))
        {
            if (affectedTargets.ContainsKey(es))
            {
                targetsToUnaffect.Add(es);
            }
        }
        else if (other.TryGetComponent(out BreakableProps p))
        {
            if (affectedProps.ContainsKey(p))
            {
                propsToUnaffect.Add(p);
            }
        }    
    }
}
