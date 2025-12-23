// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class ComboUIController : MonoBehaviour
// {
//     [SerializeField] private TextMeshProUGUI multiplierText; // Текст для "x1, x2..."
//     [SerializeField] private TextMeshProUGUI comboCountText; // Текст для "56 NOTES"
//     [SerializeField] private Image progressCircle;           // Шкала до след. множителя

//     // ДОБАВЛЯЕМ СЮДА ССЫЛКУ НА ВТОРУЮ ПАНЕЛЬ
//     [Header("Связанные панели")]
//     public UIPuncher otherPanelPuncher; // Ссылка на UIPuncher объекта ComboPanelRight

//     [Header("Настройки")]
//     public float punchScale = 1.2f;
//     public float lerpSpeed = 10f;

//     public Color colorX1 = Color.white;
//     public Color colorX2 = Color.yellow;
//     public Color colorX3 = Color.green;
//     public Color colorX4 = Color.magenta;

//     private void Start()
//     {
//         if (RhytmComboManager.Instance != null)
//         {
//             RhytmComboManager.Instance.OnComboChanged += UpdateDisplay;
//             UpdateDisplay(0, 1);
//         }
//     }

//     private void Update()
//     {
//         // Плавное возвращение размера к обычному
//         transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * lerpSpeed);
//     }

//     private void UpdateDisplay(int combo, int multiplier)
//     {
//         // Если комбо 0 — скрываем всё
//         if (combo <= 0)
//         {
//             multiplierText.text = "";
//             comboCountText.text = "";
//             if (progressCircle) progressCircle.fillAmount = 0;
//             return;
//         }

//         // 1. Определяем текущий цвет на основе множителя
//         Color currentColor = multiplier switch
//         {
//             2 => colorX2,
//             3 => colorX3,
//             4 => colorX4,
//             _ => colorX1
//         };

//         // 2. Применяем цвет к тексту и к шкале
//         multiplierText.text = $"x{multiplier}";
//         multiplierText.color = currentColor;

//         comboCountText.text = $"{combo} NOTES";

//         if (progressCircle != null)
//         {
//             // Применяем тот же цвет к шкале
//             progressCircle.color = currentColor;

//             if (multiplier >= 4)
//             {
//                 progressCircle.fillAmount = 1f;
//             }
//             else
//             {
//                 float progress = (combo % 10) / 10f;
//                 if (combo > 0 && combo % 10 == 0) progress = 1f;

//                 progressCircle.fillAmount = progress;
//             }
//         }

//         Pulse();
//     }

//     private void Pulse()
//     {
//         transform.localScale = Vector3.one * punchScale;

//         // ИНИЦИИРУЕМ ПУЛЬСАЦИЮ ВТОРОЙ ПАНЕЛИ
//         if (otherPanelPuncher != null)
//         {
//             otherPanelPuncher.Punch();
//         }
//     }
// }
