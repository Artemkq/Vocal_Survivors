using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]

public class SkeletonVIsual : MonoBehaviour
{

    private static readonly int RunningHash = Animator.StringToHash(IsRunning);
    private static readonly int TakeHitHash = Animator.StringToHash(IsTakeHit);
    private static readonly int DieHash = Animator.StringToHash(IsDie);
    private static readonly int ChasingSpeedMultilpierHash = Animator.StringToHash(IsChasingSpeedMultilpier);
    private static readonly int AttackHash = Animator.StringToHash(IsAttack);

    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private EnemyEntity enemyEntity;
    [SerializeField] private GameObject enemyShadow;

    private Animator _animator;

    private const string IsRunning = "IsRunning";
    private const string IsTakeHit = "TakeHit";
    private const string IsDie = "IsDie";
    private const string IsChasingSpeedMultilpier = "ChasingSpeedMultiplier";
    private const string IsAttack = "Attack";

    SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        enemyAI.OnEnemyAttack += _enemyAI_OnEnemyAttack;
        enemyEntity.OnTakeHit += _enemyEntity_OnTakeHit;
        enemyEntity.OnDeath += _enemyEntity_OnDeath;
    }

    private void _enemyEntity_OnDeath(object sender, System.EventArgs e)
    {
        _animator.SetBool(DieHash, true);
        _spriteRenderer.sortingOrder = -1;
        enemyShadow.SetActive(false);
    }

    private void _enemyEntity_OnTakeHit(object sender, System.EventArgs e)
    {
        _animator.SetTrigger(TakeHitHash);
    }

    private void Update()
    {
        _animator.SetBool(RunningHash, enemyAI.IsRunning);
        _animator.SetFloat(ChasingSpeedMultilpierHash, enemyAI.GetRoamingAnimationSpeed());
    }

    public void TriggerAttackAnimationTurnOff()
    {
        enemyEntity.PolygonColliderTurnOff();
    }

    public void TriggerAttackAnimationTurnOn()
    {
        enemyEntity.PolygonColliderTurnOn();
    }

    private void _enemyAI_OnEnemyAttack(object sender, System.EventArgs e)
    {
        _animator.SetTrigger(AttackHash);
    }

    private void OnDestroy()
    {
        enemyAI.OnEnemyAttack -= _enemyAI_OnEnemyAttack;
        enemyEntity.OnTakeHit -= _enemyEntity_OnTakeHit;
        enemyEntity.OnDeath -= _enemyEntity_OnDeath;
    }
}
