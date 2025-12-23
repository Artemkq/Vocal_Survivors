// using UnityEngine;
// using TMPro;

// public class ScoreManager : MonoBehaviour
// {
//     public static ScoreManager Instance;

//     [Header("Настройки очков")]
//     public int pointsPerHit = 50;

//     [Header("UI элементы")]
//     [SerializeField] private TextMeshProUGUI scoreText;

//     public long CurrentScore { get; private set; } = 0;

//     void Awake()
//     {
//         Instance = this;
//     }

//     void Start()
//     {
//         // При старте UI будет пустым
//         UpdateScoreUI();
//     }

//     public void AddPoints()
//     {
//         if (RhytmComboManager.Instance == null) return;

//         int multiplier = RhytmComboManager.Instance.CurrentMultiplier;
//         int pointsToAdd = pointsPerHit * multiplier;

//         CurrentScore += pointsToAdd;

//         UpdateScoreUI();
//     }

//     private void UpdateScoreUI()
//     {
//         if (scoreText != null)
//         {
//             // Если очков 0 (начало игры), текст пустой. 
//             // Как только появилось хоть 1 очко — показываем число.
//             if (CurrentScore <= 0)
//             {
//                 scoreText.text = "";
//             }
//             else
//             {
//                 scoreText.text = CurrentScore.ToString("N0");
//             }
//         }
//     }

//     // Метод для полного сброса (если нужно при рестарте уровня)
//     public void ResetScore()
//     {
//         CurrentScore = 0;
//         UpdateScoreUI();
//     }
// }
