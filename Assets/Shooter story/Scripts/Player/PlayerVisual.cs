using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private static readonly int RunningHash = Animator.StringToHash(IsRunning);
    private static readonly int DieHash = Animator.StringToHash(IsDie);

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private FlashBlink _flashBlink;

    private const string IsRunning = "IsRunning";
    private const string IsDie = "IsDie";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _flashBlink = GetComponent<FlashBlink>();
    }

    private void Start()
    {
        Player.Instance.OnPlayerDeath += Player_OnPlayerDeath;
    }

    private void Player_OnPlayerDeath(object sender, System.EventArgs e)
    {
        _animator.SetBool(DieHash, true);
        _flashBlink.StopBlinking();
    }

    private void Update()
    {
        _animator.SetBool(RunningHash, Player.Instance.IsRunning());

        if (Player.Instance.IsAlive())
            AdjustPlayerFacingDirection();
    }
    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 playerPosition = Player.Instance.GetPlayerScreenPosition();

        _spriteRenderer.flipX = mousePos.x < playerPosition.x;

        //Сверху сокращение того, что ниже
        //
        //if (mousePos.x < playerPosition.x)
        //{
        //    _spriteRenderer.flipX = true;
        //}
        //else
        //{
        //    _spriteRenderer.flipX = false;
        //}
    }

    private void OnDestroy()
    {
        Player.Instance.OnPlayerDeath -= Player_OnPlayerDeath;
    }
}