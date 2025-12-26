// using UnityEngine;

// public class RhythmReactor : MonoBehaviour
// {
//     [Header("Настройки драйва")]
//     [Tooltip("Через сколько секунд тишины выключать барабаны")]
//     public float timeToSustain = 1.0f;

//     private MusicLayerManager _mlm;
//     private BeatConductor _bc;

//     private float _stopTime = -1f;
//     private bool _areDrumsActive = false;

//     void Start()
//     {
//         _mlm = MusicLayerManager.Instance;
//         _bc = BeatConductor.Instance;

//         if (_mlm == null || _bc == null)
//         {
//             Debug.LogError("MusicLayerManager или BeatConductor не найдены!");
//             enabled = false;
//             return;
//         }

//         // В начале выключаем барабаны
//         _mlm.SetLayer("drumsVol", false);
//     }

//     void Update()
//     {
//         // 1. Регистрация нажатия в бит
//         // Теперь мы проверяем флаг WasPressedThisWindow, который BeatConductor ставит при попадании
//         if (_bc.WasPressedThisWindow && !_areDrumsActive)
//         {
//             ActivateDrums();
//         }

//         // Если игрок нажал пробел, продлеваем время звучания
//         if (Input.GetKeyDown(KeyCode.Space) && _bc.IsInBeatWindow)
//         {
//             _stopTime = Time.time + timeToSustain;
//         }

//         // 2. Логика выключения по таймеру
//         if (_areDrumsActive)
//         {
//             if (Time.time >= _stopTime)
//             {
//                 DeactivateDrums();
//             }
//         }
//     }

//     private void ActivateDrums()
//     {
//         _mlm.SetLayer("drumsVol", true);
//         _areDrumsActive = true;
//         _stopTime = Time.time + timeToSustain;
//         // Debug.Log("Drums ON");
//     }

//     private void DeactivateDrums()
//     {
//         _mlm.SetLayer("drumsVol", false);
//         _areDrumsActive = false;
//         // Debug.Log("Drums OFF");
//     }
// }
