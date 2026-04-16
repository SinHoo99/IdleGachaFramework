using UnityEngine;

public class Bullet : PoolObject
{
    private Rigidbody2D _rb;
    private Animator _animator;

    private string _ownerTag;
    private float _damage;
    
    // 사거리 제한을 위한 변수들
    private Vector2 _startPosition;
    private float _maxRange;
    private bool _hasRangeLimit;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private bool _isHit = false;

    private void Update()
    {
        // 사거리 제한이 있는 경우 이동 거리 체크
        if (_hasRangeLimit)
        {
            float distanceTraveled = Vector2.Distance(_startPosition, transform.position);
            if (distanceTraveled >= _maxRange)
            {
                ReturnToPool();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isHit) return;

        // 콜라이더가 자식 오브젝트에 있는 경우를 대비해 GetComponentInParent를 사용하여 IDamageable을 찾습니다.
        var damageable = collision.GetComponentInParent<IDamageable>();
        
        if (damageable != null)
        {
            _isHit = true; // 즉시 피격된 것으로 표시
            Debug.Log($"[Bullet] 대상 피격: {collision.gameObject.name} (위치: {transform.position}). 총알 인스턴스 ID: {gameObject.GetInstanceID()}");
            damageable.TakeDamage(_damage, transform.position);
            SpawnDamageText(_damage, transform.position);
            SpawnHitEffect(transform.position);
            ReturnToPool();
        }
    }

    private void SpawnHitEffect(Vector3 hitPosition)
    {
        if (PoolManager.Instance == null) return;

        // 피격 이펙트가 몬스터 뒤에 가려지지 않도록 Z축 오프셋 적용
        Vector3 effectPos = new Vector3(hitPosition.x, hitPosition.y, hitPosition.z - 0.3f);
        PoolManager.Instance.Spawn<PoolObject>(Tag.HitEffect, effectPos, Quaternion.identity);
    }

    private void SpawnDamageText(float damage, Vector3 hitPosition)
    {
        if (PoolManager.Instance == null) return;

        // 데미지 텍스트가 다른 오브젝트보다 앞에 보이도록 Z축 오프셋 적용
        Vector3 textPos = new Vector3(hitPosition.x, hitPosition.y, hitPosition.z - 0.5f);
        var damageText = PoolManager.Instance.Spawn<DamageText>(Tag.DamageText, textPos, Quaternion.identity);
        if (damageText != null)
        {
            damageText.Setup(damage);
        }
    }

    /// <summary>
    /// 총알을 풀로 반납하기 위한 애니메이션 이벤트 수신기입니다.
    /// </summary>
    public void BulletObjectreturn()
    {
        ReturnToPool();
    }

    /// <summary>
    /// 총알을 오브젝트 풀로 반납합니다.
    /// </summary>
    public void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnObject(Tag.Bullet, this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 풀에서 생성될 때 총알 속성을 초기화합니다.
    /// </summary>
    public void Setup(Vector2 direction, string ownerTag, float bulletDamage, float maxRange = -1f)
    {
        _ownerTag = ownerTag;
        _damage = bulletDamage;
        _startPosition = transform.position;
        
        // 사거리 제한 설정 (maxRange가 0보다 클 때만 활성화)
        if (maxRange > 0)
        {
            _maxRange = maxRange;
            _hasRangeLimit = true;
        }
        else
        {
            _hasRangeLimit = false;
        }
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (_rb != null)
        {
            _rb.velocity = direction.normalized * 10f;
        }
    }

    public override void OnSpawn(Vector3 position, Quaternion rotation)
    {
        _isHit = false; // 생성 시 피격 플래그 리셋
        // 2D에서 충돌을 보장하기 위해 Z 위치를 0으로 고정
        Vector3 spawnPos = new Vector3(position.x, position.y, 0f);
        base.OnSpawn(spawnPos, rotation);
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        if (_rb != null) _rb.velocity = Vector2.zero;
        _hasRangeLimit = false; // 플래그 초기화
    }
}
