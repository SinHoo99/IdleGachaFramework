using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PoolObject, IDamageable
{
    private HealthSystem _healthSystem;
    private Rigidbody2D _rb;
    private EntityType _type = EntityType.Enemy;
    private string _enemyName;

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

        _type = data.Type;
        _enemyName = data.Name;

        // Initialize Health
        if (_healthSystem != null)
        {
            _healthSystem.InitHP(data.MaxHealth, data.MaxHealth);
        }

        // Handle Movement based on CanMove flag
        if (data.CanMove)
        {
            if (_rb != null) _rb.velocity = Vector2.left * _moveSpeed;
        }
        else
        {
            if (_rb != null) _rb.velocity = Vector2.zero;
        }

        // Boss-specific initialization (e.g., UI connection)
        if (_type == EntityType.Boss)
        {
            InitializeBossUI();
        }
    }

    private void InitializeBossUI()
    {
        // Search for a Boss Health Bar UI in the scene if this is a boss
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
        // Force Z position to 0 to ensure collision in 2D
        Vector3 spawnPos = new Vector3(position.x, position.y, 0f);
        base.OnSpawn(spawnPos, rotation);
    }

    private void HandleDeath()
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.UnregisterEnemy(this);
        }

        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnEnemyDefeated();
        }

        if (_type == EntityType.Boss)
        {
            // Optional: Reward logic for bosses
            if (PlayerDataManager.Instance?.NowPlayerData != null)
                PlayerDataManager.Instance.NowPlayerData.PlayerCoin += 100; // Example reward

            // Hide Boss UI
            var bossUI = FindObjectOfType<HealthStatusUI>();
            if (bossUI != null) bossUI.HideSlider();
        }

        ReturnToPool();
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
        // Add collision logic with player or units if needed
    }
}
