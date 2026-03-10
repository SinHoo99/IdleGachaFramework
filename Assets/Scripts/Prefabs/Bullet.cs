using UnityEngine;

public class Bullet : PoolObject
{
    private GameManager GM => GameManager.Instance;
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
        // Hit Boss
        if (collision.gameObject.layer == LayerMask.NameToLayer(Layer.Boss))
        {
            if (collision.TryGetComponent<Boss>(out var boss))
            {
                boss.TakeDamage(_damage);
                ReturnToPool();
            }
        }
    }

    /// <summary>
    /// Returns the bullet to the object pool.
    /// </summary>
    public void ReturnToPool()
    {
        if (GM != null && GM.ObjectPool != null)
        {
            GM.ObjectPool.ReturnObject(Tag.Bullet, this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Initializes bullet properties when spawned from pool.
    /// </summary>
    public void Initialize(Vector2 position, Vector2 direction, string ownerTag, float bulletDamage)
    {
        _ownerTag = ownerTag;
        _damage = bulletDamage;
        
        transform.position = position;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (_rb != null)
        {
            _rb.velocity = direction.normalized * 10f;
        }
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        if (_rb != null) _rb.velocity = Vector2.zero;
    }
}
