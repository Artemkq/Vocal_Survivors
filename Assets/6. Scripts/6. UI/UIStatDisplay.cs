using System.Text;
using System.Reflection;
using TMPro;

public class UIStatDisplay : UIPropertyDisplay
{
    public PlayerStats player; // The player that this stat display is rendering stats for.
    public CharacterData character; // Display the stats from a character's data instead.
    public bool displayCurrentHealth = false;


    public override object GetReadObject()
    {
        // Returns player stats in game scene, returns character stats in character select scene as there is no assigned 'player' variable.
        if (player) return player.Stats;
        else if (character) return character.stats;
        return new CharacterData.Stats();
    }

    public override void UpdateFields()
    {
        if (!player && !character) return;

        StringBuilder[] allStats = GetProperties(
            BindingFlags.Public | BindingFlags.Instance,
            "CharacterData+Stats"
        );

        // Get a reference to both Text objects to render stat names and stat values.
        if (!propertyNames) propertyNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (!propertyValues) propertyValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();


        // Add the current health to the stat box.
        if (displayCurrentHealth)
        {
            if (player != null) // <-- ��������� ��� ��������
            {
                allStats[0].Insert(0, "Health\n");
                allStats[1].Insert(0, player.CurrentHealth + "\n");
            }
            else
            {
                //Debug.LogWarning("Cannot display current health: Player object is missing!");
            }
        }

        // Updates the fields with the strings we built.
        if (propertyNames) propertyNames.text = allStats[0].ToString();
        if (propertyValues) propertyValues.text = allStats[1].ToString();
        propertyValues.fontSize = propertyNames.fontSize;
    }

    void Reset()
    {
        player = FindAnyObjectByType<PlayerStats>();
    }
}
