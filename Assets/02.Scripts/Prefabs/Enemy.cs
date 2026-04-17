using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PoolObject, IDamageable
{
    private HealthSystem _healthSystem;
    private Rigidbody2D _rb;
    private EntityType _type = EntityType.Enemy;
    private string _enemyName;
    private EnemyData _enemyData;

    [SerializeField] private float _moveSpeed = 2f;

    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
        if (_healthSystem == null) _healthSystem = gameObject.AddComponent<HealthSystem>();
        
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        _healthSystem.OnDeath += HandleDeath;
    }

    public void Setup(EnemyData data)
    {
        if (data == null) return;

        _enemyData = data;
        _type = data.Type;
        _enemyName = data.Name;

        // 체력 초기화
        if (_healthSystem != null)
        {
            _healthSystem.InitHP(data.MaxHealth, data.MaxHealth);
        }

        // CanMove 플래그에 따른 이동 처리
        if (data.CanMove)
        {
            if (_rb != null) _rb.velocity = Vector2.left * _moveSpeed;
        }
        else
        {
            if (_rb != null) _rb.velocity = Vector2.zero;
        }

        // AI 로직
        if (TryGetComponent<EnemyAI>(out var ai))
        {
            ai.StartAI();
        }

        // 보스 전용 초기화 (예: UI 연결)
        if (_type == EntityType.Boss)
        {
            InitializeBossUI();
        }
    }

    private void InitializeBossUI()
    {
        // 보스인 경우 씬에서 보스 체력 바 UI를 찾습니다.
        var bossUI = FindObjectOfType<HealthStatusUI>();
        if (bossUI != null)
        {
            bossUI.HealthSystem = _healthSystem;
            bossUI.UpdateHPStatus();
            bossUI.ShowSlider();
        }
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath -= HandleDeath;
        }
    }

    public override void OnSpawn(Vector3 position, Quaternion rotation)
    {
        // 2D에서 충돌을 보장하기 위해 Z 위치를 0으로 고정
        Vector3 spawnPos = new Vector3(position.x, position.y, 0f);
        base.OnSpawn(spawnPos, rotation);
    }

    private void HandleDeath()
    {
        UpdateGameSystems();
        ProcessBossSpecificLogic();
        SpawnDeathRewards();
        
        ReturnToPool();
    }

    private void UpdateGameSystems()
    {
        if (SpawnManager.Instance != null) SpawnManager.Instance.UnregisterEnemy(this);
        if (StageManager.Instance != null) StageManager.Instance.OnEnemyDefeated();
    }

    private void ProcessBossSpecificLogic()
    {
        if (_type != EntityType.Boss) return;

        var bossUI = FindObjectOfType<HealthStatusUI>();
        if (bossUI != null) bossUI.HideSlider();
    }

    private void SpawnDeathRewards()
    {
        // 경험치 보상 이벤트 발행
        if (_enemyData != null)
        {
            EventBus<EnemyData>.Publish(GameEventType.OnRewardEarned, _enemyData);
        }

        SpawnCoins();
    }

    private void SpawnCoins()
    {
        if (PoolManager.Instance == null) return;

        // 적 종류에 따라 코인 개수 조절 (일반 1~3, 보스 10~20)
        int coinCount = (_type == EntityType.Boss) ? Random.Range(10, 21) : Random.Range(1, 4);

        for (int i = 0; i < coinCount; i++)
        {
            PoolManager.Instance.Spawn<Coin>(Tag.Coin, transform.position, Quaternion.identity);
        }
    }

    public void TakeDamage(float damage, Vector3 hitPosition)
    {
        _healthSystem.TakeDamage(damage);
    }

    private void ReturnToPool()
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.UnregisterEnemy(this);
        }

        if (PoolManager.Instance != null && !string.IsNullOrEmpty(_enemyName))
        {
            PoolManager.Instance.ReturnObject(_enemyName, this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        if (_rb != null) _rb.velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 필요한 경우 플레이어 또는 유닛과의 충돌 로직 추가
    }
}
