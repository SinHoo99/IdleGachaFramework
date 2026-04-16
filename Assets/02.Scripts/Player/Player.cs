using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimation), typeof(PlayerCombat), typeof(HealthSystem))]
public class Player : Singleton<Player>, IDamageable
{
    private PlayerState _currentState = PlayerState.Idle;
    public PlayerState CurrentState => _currentState;

    private PlayerAnimation _animation;
    private PlayerCombat _combat;
    private HealthSystem _health;

    [Header("Settings")]
    [SerializeField] private float _idleCheckInterval = 0.1f;
    [SerializeField] private float _attackDuration = 0.5f;
    [SerializeField] private float _hitDuration = 0.3f;

    private float _idleCheckTimer = 0f;

    protected override void Awake()
    {
        isDontDestroy = false;
        base.Awake();
        
        _animation = GetComponent<PlayerAnimation>();
        if (_animation == null) _animation = gameObject.AddComponent<PlayerAnimation>();
        
        _combat = GetComponent<PlayerCombat>();
        if (_combat == null) _combat = gameObject.AddComponent<PlayerCombat>();

        _health = GetComponent<HealthSystem>();
        if (_health == null) _health = gameObject.AddComponent<HealthSystem>();
    }

    private void Start()
    {
        _health.OnDeath += HandleDie;
        ChangeState(PlayerState.Idle);
    }

    private void OnDestroy()
    {
        if (_health != null) _health.OnDeath -= HandleDie;
    }

    private void Update()
    {
        switch (_currentState)
        {
            case PlayerState.Idle:
                _idleCheckTimer += Time.deltaTime;
                if (_idleCheckTimer >= _idleCheckInterval)
                {
                    _idleCheckTimer = 0f;
                    if (_combat.DetectEnemy())
                    {
                        ChangeState(PlayerState.Attack);
                    }
                }
                break;
        }
    }

    public void ChangeState(PlayerState newState)
    {
        if (_currentState == newState && newState != PlayerState.Hit) return;

        ExitState(_currentState);
        _currentState = newState;
        EnterState(_currentState);
        
        _animation.UpdateAnimationState(_currentState, _combat.IsParrying);
    }

    private void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Attack:
                _combat.ResetShotCount(); // 전투 컴포넌트의 카운트 초기화
                StartCoroutine(AttackRoutine());
                break;
            case PlayerState.Hit:
                _combat.EndParry();
                StartCoroutine(HitRoutine());
                break;
            case PlayerState.Die:
                _combat.EndParry();
                _health.TakeDamage(_health.CurHP);
                break;
        }
    }

    private void ExitState(PlayerState state) { }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(_attackDuration);
        ChangeState(PlayerState.Idle);
    }

    // 애니메이션 이벤트 수신용 메서드
    public void ShootBullet()
    {
        if (_combat != null) 
        {
            _combat.PerformShoot(); // 실제 발사 및 카운트 체크는 Combat에서 처리
        }
    }

    private IEnumerator HitRoutine()
    {
        yield return new WaitForSeconds(_hitDuration);
        ChangeState(PlayerState.Idle);
    }

    private void HandleDie()
    {
        if (_currentState != PlayerState.Die)
        {
            ChangeState(PlayerState.Die);
        }
        StageManager.Instance.FailStage();
    }

    public void StartParry()
    {
        if (_currentState == PlayerState.Die) return;
        _combat.StartParry();
        _animation.UpdateAnimationState(_currentState, _combat.IsParrying);
    }

    public void OnParryAnimationEnd()
    {
        _combat.EndParry();
        _animation.UpdateAnimationState(_currentState, _combat.IsParrying);
    }

    public void TakeDamage(float damage, Vector3 hitPosition)
    {
        if (_combat.IsParrying)
        {
            Debug.Log("<color=green>[Player] 패링 성공!</color>");
            return;
        }

        if (_currentState == PlayerState.Die) return;

        _health.TakeDamage(damage);
        if (!_health.IsDead)
        {
            ChangeState(PlayerState.Hit);
        }
    }
}
