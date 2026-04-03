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

    private float _idleCheckTimer = 0f;
    private const float IDLE_CHECK_INTERVAL = 0.1f;

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
                if (_idleCheckTimer >= IDLE_CHECK_INTERVAL)
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
                StartCoroutine(AttackRoutine());
                break;
            case PlayerState.Hit:
                _combat.EndParry();
                StartCoroutine(HitRoutine());
                break;
            case PlayerState.Die:
                _combat.EndParry();
                _health.TakeDamage(_health.CurHP); // Ensure health is 0
                break;
        }
    }

    private void ExitState(PlayerState state) { }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        ChangeState(PlayerState.Idle);
    }

    // 애니메이션 이벤트 수신용 메서드 (단 한 번만 실행됨)
    public void ShootBullet()
    {
        if (_combat != null) _combat.PerformShoot();
    }

    private IEnumerator HitRoutine()
    {
        yield return new WaitForSeconds(0.3f);
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
            Debug.Log("<color=green>[Player] PARRY SUCCESS!</color>");
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
