using System.Collections;
using UnityEngine;

public class Coin : PoolObject
{
    [Header("Settings")]
    [SerializeField] private float _popForce = 5f;
    [SerializeField] private float _waitDuration = 0.7f;
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _acceleration = 20f;
    [SerializeField] private int _coinValue = 10;

    private Rigidbody2D _rb;
    private bool _isFollowing = false;
    private float _currentSpeed;
    private Transform _playerTransform;

    // GC 최적화를 위한 WaitForSeconds 캐싱
    private static readonly WaitForSeconds _waitDurationSeconds = new WaitForSeconds(0.7f);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
        }
    }

    public override void OnSpawn(Vector3 position, Quaternion rotation)
    {
        base.OnSpawn(position, rotation);
        ResetState();
        
        // 사방으로 튀어오르는 힘 적용
        Vector2 randomDir = new Vector2(Random.Range(-0.5f, 0.5f), 1f).normalized;
        _rb.AddForce(randomDir * _popForce, ForceMode2D.Impulse);

        StartCoroutine(FollowPlayerRoutine());
    }

    private void ResetState()
    {
        _isFollowing = false;
        _currentSpeed = _moveSpeed;
        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
            _rb.isKinematic = false;
        }
    }

    private IEnumerator FollowPlayerRoutine()
    {
        yield return _waitDurationSeconds;

        if (Player.Instance != null)
        {
            _rb.velocity = Vector2.zero;
            _rb.isKinematic = true; 
            _playerTransform = Player.Instance.transform;
            _isFollowing = true;
        }
    }

    private void Update()
    {
        if (!_isFollowing || _playerTransform == null) return;

        MoveTowardsPlayer();
        CheckAbsorption();
    }

    private void MoveTowardsPlayer()
    {
        _currentSpeed += _acceleration * Time.deltaTime;
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        transform.position += direction * (_currentSpeed * Time.deltaTime);
    }

    private void CheckAbsorption()
    {
        // 제곱근 연산을 피하기 위해 sqrMagnitude 사용 (성능 최적화)
        float distanceSqr = (_playerTransform.position - transform.position).sqrMagnitude;
        if (distanceSqr < 0.25f) // 0.5f * 0.5f = 0.25f
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.GainCoin(_coinValue);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        _isFollowing = false;
        _rb.isKinematic = false;
        
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnObject(Tag.Coin, this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SetValue(int value)
    {
        _coinValue = value;
    }
}
