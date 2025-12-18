using UnityEngine;

[CreateAssetMenu(fileName = "Treasure Chest Drop Profile", menuName = "2D Top-down Rogue-like/Treasure Chest Drop Profile")]

public class TreasureChestDropProfile : ScriptableObject
{
    [Header("General Settings")]
    public string profileName = "Drop Profile";
    [Tooltip("Влияние удачи на шанс выпадения предметов из сундука")]
    [Range(0, 1)] public float luckScaling = 0; 
    [Tooltip("Базовый шанс выпадения предметов из сундука (в процентах)")]
    [Range(0, 100)] public float baseDropChance; 
    [Tooltip("Длительность анимации открытия сундука")]
    public float animDuration;

    [Header("Fireworks")]
    [Tooltip("Включить фейерверк при открытии сундука?")]
    public bool hasFireworks = false;
    [Tooltip("Задержка перед запуском фейерверка (в секундах)")]
    [Range(0f, 100f)] public float fireworksDelay = 1f;

    [Header("Item Display Settings")]
    [Tooltip("Кол-во предметом, выпадающих из сундука")]
    public int noOfItems = 1;
    [Tooltip("Цвет лучей при выпадении предметов")]
    public Color[] beamColors = new Color[] { new Color(0, 0, 1, 0.6f) };

    [Tooltip("Задержка между появлением лучей (в секундах)")]
    [Range(0f, 100f)] public float delayTime = 0f;
    [Tooltip("Количество задержанных лучей")]
    public int delayedBeams = 0;

    [Tooltip("Использовать изогнутые лучи?")]
    public bool hasCurvedBeams;
    [Tooltip("Время спавна изогнутых лучей (в секундах)")]
    public float curveBeamsSpawnTime;

    [Header("Item Reveal Animation Settings")]
    [Tooltip("Задержка между появлением предметов (в секундах)")]
    public float itemRevealDelay = 0.3f;

    [Header("Diamonds")]
    [Tooltip("Максимальное количество кристалов, выпадающих из сундука")]
    public float maxDiamonds = 0; 
    [Tooltip("Минимальное количество кристалов, выпадающих из сундука")]
    public float minDiamonds = 0;

    [Header("Chest Sound Effects")] 
    [Tooltip("Звук открытия сундука")]
    public AudioClip openingSound;
}