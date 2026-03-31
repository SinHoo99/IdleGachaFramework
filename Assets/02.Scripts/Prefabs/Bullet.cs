using UnityEngine;

public class Bullet : PoolObject
{
    private Rigidbody2D _rb;
    private Animator _animator;

    private string _ownerTag;
    private float _damage;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Use GetComponentInParent to find IDamageable in case the collider is on a child object
        var damageable = collision.GetComponentInParent<IDamageable>();
        
        if (damageable != null)
        {
            damageable.TakeDamage(_damage, transform.position);
            SpawnDamageText(_damage, transform.position);
            ReturnToPool();
        }
    }

    private void SpawnDamageText(float damage, Vector3 hitPosition)
    {
        if (PoolManager.Instance == null) return;

        var damageText = PoolManager.Instance.Spawn<DamageText>(Tag.DamageText, hitPosition, Quaternion.identity);
        if (damageText != null)
        {
            damageText.Setup(damage);
        }
    }

    /// <summary>
    /// Animation event receiver to return the bullet to the pool.
    /// </summary>
    public void BulletObjectreturn()
    {
        ReturnToPool();
    }

    /// <summary>
    /// Returns the bullet to the object pool.
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
    /// Initializes bullet properties when spawned from pool.
    /// </summary>
    public void Setup(Vector2 direction, string ownerTag, float bulletDamage)
    {
        _ownerTag = ownerTag;
        _damage = bulletDamage;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (_rb != null)
        {
            _rb.velocity = direction.normalized * 10f;
        }
    }

    public override void OnSpawn(Vector3 position, Quaternion rotation)
    {
        // Force Z position to 0 to ensure collision in 2D
        Vector3 spawnPos = new Vector3(position.x, position.y, 0f);
        base.OnSpawn(spawnPos, rotation);
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        if (_rb != null) _rb.velocity = Vector2.zero;
    }

}
