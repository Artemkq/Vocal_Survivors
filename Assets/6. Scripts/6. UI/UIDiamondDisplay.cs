using TMPro;
using UnityEngine;

/// <summary>
/// Component that is attached to GameObjects to make it display the player's coins. 
/// Either in-game, or the total number of coins the player has, depending on whether 
/// the collector variable is set.
/// </summary>

public class UIDiamondDisplay : MonoBehaviour
{
    TextMeshProUGUI displayTarget;
    public PlayerCollector collector;

    void Start()
    {
        displayTarget = GetComponentInChildren<TextMeshProUGUI>();
        UpdateDisplay();
        if (collector != null) collector.onDiamondCollected += UpdateDisplay;
    }

    public void UpdateDisplay()
    {
        // If a collector is assigned, we will display the number of coins the collector has.
        if (collector != null)
        {
            displayTarget.text = Mathf.RoundToInt(collector.GetDiamonds()).ToString();
        }
        else
        {
            // If not, we will get the current number of coins that are saved.
            float diamonds = SaveManager.LastLoadedGameData.diamonds;
            displayTarget.text = Mathf.RoundToInt(diamonds).ToString();
        }
    }
}