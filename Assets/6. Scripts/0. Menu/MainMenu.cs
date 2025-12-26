// using UnityEngine;
// using UnityEngine.SceneManagement;
// using System.Collections;

// public class MainMenu : MonoBehaviour
// {
//     [Header("Настройки сцен")]
//     [SerializeField] private string gameplaySceneName = "GameplayScene"; // Напишите тут имя вашей игровой сцены

//     public void PlayGame()
//     {
//         // Запускаем асинхронную загрузку (не тормозит игру)
//         StartCoroutine(LoadSceneRoutine());
//     }

//     private IEnumerator LoadSceneRoutine()
//     {
//         // Здесь можно запустить анимацию "исчезновения" (Fade Out)
        
//         AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameplaySceneName);

//         // Пока сцена грузится, мы можем что-то делать (например, крутить иконку загрузки)
//         while (!asyncLoad.isDone)
//         {
//             yield return null;
//         }
//     }

//     public void ExitGame()
//     {
//         Debug.Log("Выход из игры...");
//         Application.Quit();
//     }
// }
