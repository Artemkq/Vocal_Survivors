using UnityEngine;

public class SwordVisual : MonoBehaviour
{
    private static readonly int AttackHash = Animator.StringToHash(IsAttack);

    [SerializeField] private Sword sword;

    private Animator _animator;
    private const string IsAttack = "Attack";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        sword.OnSwordSwing += Sword_OnSwordSwing;
    }

    private void Sword_OnSwordSwing(object sender, System.EventArgs e)
    {
        _animator.SetTrigger(AttackHash);
    }

    public void TriggerEndAttackAnimation()
    {
        sword.AttackColliderTurnOff();
    }

    private void OnDestroy()
    {
        sword.OnSwordSwing -= Sword_OnSwordSwing;
    }
}
