using System;
using System.Collections;
using UnityEngine;

public class Player : Singleton<Player>
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private PlayerState _currentState = PlayerState.Idle;

    [Header("Attack Settings")]
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _parryWindow = 0.2f; // 패링 유효 시간
    
    private Animator _animator;
    private bool _isParrying; // 독자적인 패링 상태 플래그

    public PlayerState CurrentState => _currentState;

    protected override void Awake()
    {
        isDontDestroy = false; // 씬 전환 시 파괴되도록 명시적으로 설정
        base.Awake(); // Singleton의 Awake 호출 (중복 검사 및 DontDestroyOnLoad 여부 결정)
        
        _animator = GetComponent<Animator>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        ChangeState(PlayerState.Idle);
    }

    private void Update()
    {
        // Handle state-specific logic
        switch (_currentState)
        {
            case PlayerState.Idle:
                HandleIdle();
                break;
            case PlayerState.Attack:
                break;
            case PlayerState.Hit:
                break;
            case PlayerState.Die:
                break;
        }
    }

    /// <summary>
    /// UI 버튼 등 외부에서 호출할 패링 시작 메서드
    /// </summary>
    public void OnParry()
    {
        if (_currentState == PlayerState.Die) return;
        StartCoroutine(PerformParry());
    }

    public void ChangeState(PlayerState newState)
    {
        if (_currentState == newState) return;

        // Exit current state logic
        ExitState(_currentState);

        _currentState = newState;

        // Enter new state logic
        EnterState(_currentState);
    }

    private void EnterState(PlayerState state)
    {
        // 1. Sync Animation first
        UpdateAnimationState();

        // 2. State Specific Logic
        switch (state)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.Attack:
                StartCoroutine(AttackRoutine());
                break;
            case PlayerState.Hit:
                StartCoroutine(HitRoutine());
                break;
            case PlayerState.Die:
                HandleDie();
                break;
        }
    }

    private void ExitState(PlayerState state)
    {
        // Cleanup when leaving a state
    }

    #region Animation Logic

    private void UpdateAnimationState()
    {
        SetAnimBool("isIdle", _currentState == PlayerState.Idle);
        SetAnimBool("isAttacking", _currentState == PlayerState.Attack);
        SetAnimBool("isHit", _currentState == PlayerState.Hit);
        SetAnimBool("isDie", _currentState == PlayerState.Die);
        SetAnimBool("isParrying", _isParrying); // Use the independent flag
    }

    private void SetAnimBool(string paramName, bool value)
    {
        if (_animator == null) return;
        
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            if (param.name == paramName)
            {
                _animator.SetBool(paramName, value);
                break;
            }
        }
    }

    #endregion

    #region State Behaviors

    private void HandleIdle()
    {
        if (_firePoint == null) return;

        // Check for enemies within attack range using OverlapCircle
        Collider2D enemy = Physics2D.OverlapCircle(_firePoint.position, _attackRange, _enemyLayer);

        if (enemy != null)
        {
            ChangeState(PlayerState.Attack);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_firePoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_firePoint.position, _attackRange);
    }

    private IEnumerator AttackRoutine()
    {
        Debug.Log("[Player] Started Attacking");
        ShootBullet();
        
        yield return new WaitForSeconds(0.5f);
        
        ChangeState(PlayerState.Idle);
    }

    private IEnumerator PerformParry()
    {
        if (_isParrying) yield break; // 중복 실행 방지

        Debug.Log("<color=cyan>[Player] Parry Window Open</color>");
        _isParrying = true;
        UpdateAnimationState(); // Sync animation

        yield return new WaitForSeconds(_parryWindow);

        _isParrying = false;
        UpdateAnimationState(); // Sync animation
        Debug.Log("<color=cyan>[Player] Parry Window Closed</color>");
    }

    private IEnumerator HitRoutine()
    {
        Debug.Log("[Player] Hit State!");
        yield return new WaitForSeconds(0.3f);
        ChangeState(PlayerState.Idle);
    }

    private void HandleDie()
    {
        Debug.Log("[Player] Player Died");
        StageManager.Instance.FailStage();
    }

    #endregion

    #region Shooting Logic
    public Bullet CreateBullet(string tag, Vector2 position, Vector2 direction, string ownerTag)
    {
        if (ObjectPool.Instance == null) return null;

        var bullet = ObjectPool.Instance.Spawn<Bullet>(tag, position, Quaternion.identity);
        if (bullet != null)
        {
            float damage = UnityEngine.Random.Range(10f, 20f);
            bullet.Setup(direction, ownerTag, damage);
            return bullet;
        }
        return null;
    }

    public void ShootBullet()
    {
        if (_firePoint == null) return;
        Vector2 direction = new Vector2(1, 0);
        CreateBullet(Tag.Bullet, _firePoint.position, direction, gameObject.tag);
    }
    #endregion

    public void OnDamaged(float damage)
    {
        // 1. Check Parry First (Independent of current state)
        if (_isParrying)
        {
            Debug.Log("<color=green>[Player] PARRY SUCCESS!</color>");
            // 추가적인 패링 성공 피드백 (SFX, VFX 등)을 여기에 넣을 수 있습니다.
            return;
        }

        if (_currentState == PlayerState.Die) return;

        Debug.Log($"[Player] Took {damage} damage");
        ChangeState(PlayerState.Hit);
    }
}
