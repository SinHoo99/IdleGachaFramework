using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Unit : PoolObject
{
    private GameManager GM => GameManager.Instance;

    [SerializeField] private FruitsID _fruitsID;
    [SerializeField] private Transform _firePoint;

    private Boss _boss;
    private Coroutine _shootCoroutine;

    private float _lastSFXTime = 0f;
    private float _sfxCooldown = 0.2f;

    private void Awake()
    {
        AssignFruitID();
    }

    private void OnEnable()
    {
        InitializeBossReference();

        if (_fruitsID == FruitsID.None)
        {
            Debug.LogWarning($"[Unit] FruitID for {gameObject.name} is not assigned.");
            return;
        }

        if (_shootCoroutine != null) StopCoroutine(_shootCoroutine);
        _shootCoroutine = StartCoroutine(ShootCoroutine());
    }

    private void OnDisable()
    {
        if (_shootCoroutine != null)
        {
            StopCoroutine(_shootCoroutine);
            _shootCoroutine = null;
        }
    }

    private void InitializeBossReference()
    {
        if (_boss == null && GM != null && GM.SpawnManager != null)
        {
            _boss = GM.SpawnManager.GetCurrentBoss();
        }

        if (_boss == null)
        {
            _boss = FindFirstObjectByType<Boss>();
        }
    }

    #region Fruit ID Assignment
    public void AssignFruitID()
    {
        if (_fruitsID != FruitsID.None) return;

        string prefabName = gameObject.name.Replace("(Clone)", "").Trim();
        
        if (GM?.DataManager?.FruitDatas == null) return;

        foreach (var fruitData in GM.DataManager.FruitDatas.Values)
        {
            if (fruitData.Name.Trim() == prefabName)
            {
                _fruitsID = fruitData.ID;
                return;
            }
        }

        Debug.LogError($"[Unit] Failed to automatically assign FruitID for {gameObject.name}.");
    }
    #endregion

    #region Shooting Logic
    public PoolObject CreateBullet(string tag, Vector2 position, Vector2 direction, string ownerTag)
    {
        if (GM?.ObjectPool == null) return null;

        PoolObject bulletObj = GM.ObjectPool.SpawnFromPool(tag);
        if (bulletObj != null && bulletObj.TryGetComponent<Bullet>(out var bullet))
        {
            float damage = GetBulletDamage();
            bullet.Initialize(position, direction, ownerTag, damage);
            return bulletObj;
        }
        return null;
    }

    private IEnumerator ShootCoroutine()
    {
        while (true)
        {
            float attackSpeed = GetAttackSpeed();
            float randomVariance = Random.Range(-0.3f, 0.3f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, attackSpeed + randomVariance));
            ShootBullet();
        }
    }

    private void ShootBullet()
    {
        if (_boss == null) return;

        // Check if boss is active and visible
        if (_boss.TryGetComponent<SpriteRenderer>(out var bossSprite) && (!bossSprite.enabled || !bossSprite.gameObject.activeInHierarchy))
        {
            return;
        }

        Vector2 direction = (_boss.transform.position - _firePoint.position).normalized;
        CreateBullet(Tag.Bullet, _firePoint.position, direction, gameObject.tag);
        
        PlayShootEffects();
        PlayLimitedSFX();
    }

    private float GetBulletDamage()
    {
        var data = GM.GetFruitsData(_fruitsID);
        return data != null ? data.Damage * 0.1f : 0f;
    }

    private float GetAttackSpeed()
    {
        var data = GM.GetFruitsData(_fruitsID);
        return data != null ? data.AttackSpeed : 1.0f;
    }
    #endregion

    #region Visual Effects
    private void PlayShootEffects()
    {
        // Player recoil effect
        transform.DOKill();
        transform.DOScale(new Vector3(0.95f, 1.05f, 1f), 0.05f).OnComplete(() => transform.DOScale(Vector3.one, 0.05f));

        if (_firePoint != null)
        {
            _firePoint.DOKill();
            _firePoint.DOShakePosition(0.2f, 0.1f);
            _firePoint.DOScale(1.2f, 0.05f).OnComplete(() => _firePoint.DOScale(1f, 0.05f));
        }
    }

    private void PlayLimitedSFX()
    {
        if (Time.time - _lastSFXTime < _sfxCooldown) return;
        _lastSFXTime = Time.time;

        if (GM != null) GM.PlaySFX(SFX.Shoot);
    }
    #endregion

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        transform.DOKill();
        if (_firePoint != null) _firePoint.DOKill();
    }
}
