using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PoolObject, IDamageable
{
    private HealthSystem _healthSystem;
    private Rigidbody2D _rb;

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
        
        // Use HealthSystem's own MaxHP value for initialization
        if (_healthSystem != null)
        {
            _healthSystem.InitHP();
        }

        // Start moving left
        if (_rb != null)
        {
            _rb.velocity = Vector2.left * _moveSpeed;
        }
    }

    private void HandleDeath()
    {
        ReturnToPool();
    }

    public void TakeDamage(float damage, Vector3 hitPosition)
    {
        _healthSystem.TakeDamage(damage);
    }

    private void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnObject(Tag.Enemy, this);
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
