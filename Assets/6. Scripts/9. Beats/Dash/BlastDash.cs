// using UnityEngine; // Это исправит ошибки CS0246 и CS0103

// public class BlastDash : MonoBehaviour
// {
//     private PlayerMovement _pm;
//     private RhythmReactor _reactor;

//     [Header("Landing Effects")]
//     public GameObject clashEffect; // Префаб тарелок
//     public float vacuumRadius = 5f;

//     void Start()
//     {
//         // Находим компоненты на игроке
//         _pm = GetComponent<PlayerMovement>();
//         _reactor = GetComponent<RhythmReactor>();

//         if (_pm != null)
//         {
//             // Подписываемся на события из вашего скрипта PlayerMovement
//             _pm.OnDashStarted += HandleDashStart;
//             _pm.OnDashFinished += HandleDashEnd;
//         }
//     }

//     private void HandleDashStart()
//     {
//         // Делаем звук "глухим" через миксер (скрипт 17)
//         if (MusicLayerManager.Instance != null)
//         {
//             MusicLayerManager.Instance.mixer.SetFloat("MasterLPF", 500f);
//         }
//     }

//     private void HandleDashEnd()
//     {
//         // Возвращаем чистоту звука
//         if (MusicLayerManager.Instance != null)
//         {
//             MusicLayerManager.Instance.mixer.SetFloat("MasterLPF", 22000f);
//         }

//         // Спавним визуальный эффект приземления (Clash)
//         if (clashEffect != null)
//         {
//             Instantiate(clashEffect, transform.position, Quaternion.identity);
//         }

//         // Если это 10-й бит комбо (или выше), засасываем врагов
//         if (_reactor != null && _reactor.comboCount >= 10)
//         {
//             CreateVacuum();
//         }
//     }

//     private void CreateVacuum()
//     {
//         // Находим всех врагов в радиусе приземления
//         Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, vacuumRadius);
//         foreach (var t in targets)
//         {
//             EnemyStats es = t.GetComponent<EnemyStats>();
//             if (es != null)
//             {
//                 // Притягиваем врага к центру приземления
//                 // (Убедитесь, что у врага есть Rigidbody2D)
//                 Vector2 dir = (transform.position - es.transform.position).normalized;
//                 es.transform.position = Vector2.MoveTowards(es.transform.position, transform.position, 2f);
//             }
//         }
//     }

//     private void OnDestroy()
//     {
//         // Отписываемся от событий при уничтожении объекта, чтобы избежать ошибок
//         if (_pm != null)
//         {
//             _pm.OnDashStarted -= HandleDashStart;
//             _pm.OnDashFinished -= HandleDashEnd;
//         }
//     }
// }
