using UnityEngine;
using UnityEngine.AI;
using ShooterStory.Utils;
using System;

public class EnemyAI : MonoBehaviour
{

    [SerializeField] private State startingState = State.Roaming;
    [SerializeField] private float roamingDistanceMax = 7f;
    [SerializeField] private float roamingDistanceMin = 3f;
    [SerializeField] private float roamingTimerMax = 2f;

    [SerializeField] private bool isChasingEnemy = true;
    [SerializeField] private float chasingDistance = 10f;
    [SerializeField] private float chasingSpeedMultiplier = 2f;

    [SerializeField] private bool isAttackingEnemy = true;
    [SerializeField] private float attackingDistance = 4f;
    [SerializeField] private float attackRate = 2f;

    [SerializeField] private float timeBeforeDestroy = 10.0f;

    private float _nextAttackTime = 0f;

    private NavMeshAgent _navMeshAgent;
    private State _currentState;
    private float _roamingTimer;
    private Vector3 _roamPosition;
    private Vector3 _startingPosition;

    private float _roamingSpeed;
    private float _chasingSpeed;

    private float _nextCheckDirectionTime = 0f;
    private float _checkDirectionDuration = 0.1f;
    private Vector3 _lastPosition;

    public event EventHandler OnEnemyAttack;

    public bool IsRunning => _navMeshAgent.velocity != Vector3.zero;

        //������ ���������� ����, ��� ����
        //
        //{
        //    get
        //    {
        //        if (_navMeshAgent.velocity == Vector3.zero)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //}

    private enum State
    {
        Idle,
        Roaming,
        Chasing,
        Attacking,
        Death
    }

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updateUpAxis = false;
        _currentState = startingState;

        _roamingSpeed = _navMeshAgent.speed;
        _chasingSpeed = _navMeshAgent.speed * chasingSpeedMultiplier;
    }

    private void Update()
    {
        StateHandler();
        MovementDirectHandler();
    }

    public void SetDeathState()
    {
        _navMeshAgent.ResetPath();
        _currentState = State.Death;
    }

    private void StateHandler()
    {

        switch (_currentState)
        {

            case State.Roaming:
                _roamingTimer -= Time.deltaTime;
                if (_roamingTimer < 0)
                {
                    Roaming();
                    _roamingTimer = roamingTimerMax;
                }
                CheckCurrentState();
                break;

            case State.Chasing:
                ChasingTarget();
                CheckCurrentState();
                break;

            case State.Attacking:
                AttackingTarget();
                CheckCurrentState();
                break;

            case State.Death:
                Destroy(gameObject, timeBeforeDestroy);
                break;

            default:
            case State.Idle:
                break;
        }
    }

    private void ChasingTarget()
    {
        _navMeshAgent.SetDestination(Player.Instance.transform.position);
    }

    public float GetRoamingAnimationSpeed()
    {
        return _navMeshAgent.speed / _roamingSpeed;
    }

    private void CheckCurrentState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);
        State newState = State.Roaming;

        if (isChasingEnemy)
        {
            if (distanceToPlayer <= chasingDistance)
            {
                newState = State.Chasing;
            }
        }

        if (isAttackingEnemy)
        {
            if (distanceToPlayer <= attackingDistance)
            {
                newState = Player.Instance.IsAlive() ? State.Attacking : State.Roaming;
            }
        }
                //������ ���������� ����, ��� ����
                //
                //{
                //    if (Player.Instance.IsAlive())
                //        newState = State.Attacking;
                //    else
                //        newState = State.Roaming;
                //}
            

        if (newState != _currentState)
        {
            if (newState == State.Chasing)
            {
                _navMeshAgent.ResetPath();
                _navMeshAgent.speed = _chasingSpeed;
            }
            else if (newState == State.Roaming)
            {
                _roamingTimer = 0f;
                _navMeshAgent.speed = _roamingSpeed;
            }
            else if (newState == State.Attacking)
            {
                _navMeshAgent.ResetPath();
            }
            _currentState = newState;
        }
    }



    private void AttackingTarget()
    {
        if (Time.time > _nextAttackTime)
        {
            OnEnemyAttack?.Invoke(this, EventArgs.Empty);
            _nextAttackTime = Time.time + attackRate;
        }
    }

    private void MovementDirectHandler()
    {
        if (Time.time > _nextCheckDirectionTime)
        {
            if (IsRunning)
            {
                ChangeFacingDirection(_lastPosition, transform.position);
            }
            else if (_currentState == State.Attacking)
            {
                ChangeFacingDirection(transform.position, Player.Instance.transform.position);
            }
            _lastPosition = transform.position;
            _nextCheckDirectionTime = Time.time + _checkDirectionDuration;
        }
    }


    private void Roaming()
    {
        _startingPosition = transform.position;
        _roamPosition = GetRoamingPosition();
        _navMeshAgent.SetDestination(_roamPosition);
    }
    private Vector3 GetRoamingPosition()
    {
        return _startingPosition + Utils.GetRandomDir() * UnityEngine.Random.Range(roamingDistanceMin, roamingDistanceMax);
    }

    private void ChangeFacingDirection(Vector3 sourcePosition, Vector3 targetPosition)
    {
        transform.rotation = sourcePosition.x > targetPosition.x ? Quaternion.Euler(0, -180, 0) : Quaternion.Euler(0, 0, 0);
    }

    //������ ���������� ����, ��� ����
    //
    //if (sourcePosition.x > targetPosition.x)
    //{
    //    transform.rotation = Quaternion.Euler(0, -180, 0);
    //}
    //else
    //{
    //    transform.rotation = Quaternion.Euler(0, 0, 0);
    //}
}
