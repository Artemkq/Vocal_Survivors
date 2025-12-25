using UnityEngine;

public class RhythmReactor : MonoBehaviour
{
    [Header("Настройки драйва")]
    [Tooltip("На сколько битов вперед продлевается звучание при попадании")]
    public int beatsToSustain = 2;

    private MusicLayerManager _mlm;
    private BeatConductor _bc;

    private int _targetBeatToStop = -1;
    private bool _areDrumsActive = false;

    void Start()
    {
        _mlm = MusicLayerManager.Instance;
        _bc = BeatConductor.Instance;

        if (_mlm == null || _bc == null)
        {
            Debug.LogError("MusicLayerManager или BeatConductor не найдены!");
            enabled = false;
        }
        _mlm.SetLayer("drumsVol", false);
    }

    void Update()
    {
        // 1. Регистрация нажатия
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckBeatHit();
        }

        // 2. Логика выключения: проверяем текущий бит из Conductor
        if (_areDrumsActive)
        {
            // Если текущий бит в песне достиг или превысил намеченную цель
            if ((int)_bc.BeatPosition >= _targetBeatToStop)
            {
                _mlm.SetLayer("drumsVol", false);
                _areDrumsActive = false;
                Debug.Log("Drums Stopped on Beat");
            }
        }
    }

    private void CheckBeatHit()
    {
        if (_bc.IsInBeatWindow)
        {
            // Если попали, планируем остановку через X битов от ТЕКУЩЕГО момента
            _targetBeatToStop = (int)_bc.BeatPosition + beatsToSustain;

            if (!_areDrumsActive)
            {
                _mlm.SetLayer("drumsVol", true);
                _areDrumsActive = true;
                Debug.Log("Drums Started!");
            }

            Debug.Log($"Hit! Drums will play until beat: {_targetBeatToStop}");
        }
        else
        {
            // Штраф за промах: выключаем барабаны на следующем же бите
            _targetBeatToStop = (int)_bc.BeatPosition + 1;
            Debug.Log("Miss! Drums will cut early.");
        }
    }
}
