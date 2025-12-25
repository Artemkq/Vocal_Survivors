using UnityEngine;

public class DrummerWeapon : Weapon
{
    [Header("Drummer Settings")]
    public float perfectAreaMultiplier = 1.5f;

    protected override bool Attack(int attackCount = 1)
    {
        if (!CanAttack()) return false;

        Aura waveObj = Instantiate(currentStats.auraPrefab, owner.transform.position, Quaternion.identity);
        waveObj.weapon = this;
        waveObj.owner = owner;

        DrumWave drumWave = waveObj.GetComponent<DrumWave>();
        if (drumWave != null)
        {
            // Проверяем: либо кнопка нажата ПРЯМО СЕЙЧАС, 
            // либо она была нажата чуть ранее в этом же окне бита.
            bool isPerfect = BeatConductor.Instance.WasPressedThisWindow ||
                             (BeatConductor.Instance.IsInBeatWindow && Input.GetKey(KeyCode.Space));

            if (isPerfect)
            {
                drumWave.SetupWave(GetArea() * perfectAreaMultiplier, Color.red);
            }
            else
            {
                drumWave.SetupWave(GetArea(), Color.white);
            }
        }

        ActivateCooldown();
        return true;
    }
}
