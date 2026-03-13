using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Unit : PoolObject
{
    private GameManager GM => GameManager.Instance;

    public string UnitID { get; private set; } = string.Empty;

    [SerializeField] private Transform _firePoint;

    private Boss _boss;
    private Coroutine _shootCoroutine;
    private Vector3 _baseScale;
    private Vector3 _targetScale;

    private float _lastSFXTime = 0f;
    private float _sfxCooldown = 0.2f;

    private void Awake()
    {
        _baseScale = transform.localScale;
        _targetScale = _baseScale;
    }

    public void SetupUnit(string id)
    {
        UnitID = id;
        UpdateScale();
    }

    private void OnEnable()
    {
        InitializeBossReference();
      //  UpdateScale();

        if (string.IsNullOrEmpty(UnitID))
        {
            // Initial warning, but setup can happen right after.
            // If it stays empty, ShootCoroutine will fail safely.
        }

        if (_shootCoroutine != null) StopCoroutine(_shootCoroutine);
        //_shootCoroutine = StartCoroutine(ShootCoroutine());
    }

    public void UpdateScale()
    {
        if (string.IsNullOrEmpty(UnitID)) return;

        float currentLevel = 1;
        if (PlayerDataManager.Instance?.NowPlayerData?.Inventory != null && 
            PlayerDataManager.Instance.NowPlayerData.Inventory.TryGetValue(UnitID, out var collectedData))
        {
            currentLevel = collectedData.Amount;
        }
        
        float scaleMultiplier = Mathf.Min(2.0f, 1.0f + (currentLevel - 1) * 0.1f);
        _targetScale = _baseScale * scaleMultiplier;
        transform.localScale = _targetScale;
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
        if (_boss == null && SpawnManager.Instance != null)
        {
            _boss = SpawnManager.Instance.GetCurrentBoss();
        }

        if (_boss == null)
        {
            _boss = FindFirstObjectByType<Boss>();
        }
    }

    #region Shooting Logic
    public PoolObject CreateBullet(string tag, Vector2 position, Vector2 direction, string ownerTag)
    {
        if (ObjectPool.Instance == null) return null;

        PoolObject bulletObj = ObjectPool.Instance.SpawnFromPool(tag);
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
        
        //PlayShootEffects();
        PlayLimitedSFX();
    }

    private float GetBulletDamage()
    {
        var data = DataManager.Instance.GetUnitData(UnitID);
        if (data == null) return 0f;

        int amount = 1;
        if (PlayerDataManager.Instance?.NowPlayerData?.Inventory != null && 
            PlayerDataManager.Instance.NowPlayerData.Inventory.TryGetValue(UnitID, out var collectedData))
        {
            amount = collectedData.Amount;
        }

        return data.Damage * 0.1f * amount;
    }

    public void UpgradeEffect()
    {
        // Visual feedback
        transform.DOKill();
        
        // Calculate permanent scale based on level (max 200% scale at Lv. 10+)
        float currentLevel = 1;
        if (PlayerDataManager.Instance?.NowPlayerData?.Inventory != null && 
            PlayerDataManager.Instance.NowPlayerData.Inventory.TryGetValue(UnitID, out var collectedData))
        {
            currentLevel = collectedData.Amount;
        }
        
        float scaleMultiplier = Mathf.Min(2.0f, 1.0f + (currentLevel - 1) * 0.1f);
        _targetScale = _baseScale * scaleMultiplier;

        // Pulse effect and then settle to new target scale
        transform.DOScale(_targetScale * 1.2f, 0.1f)
            .OnComplete(() => transform.DOScale(_targetScale, 0.1f));
        
        if (GM != null) GM.PlaySFX(SFX.Upgrade);
    }

    private float GetAttackSpeed()
    {
        var data = DataManager.Instance.GetUnitData(UnitID);
        return data != null ? data.AttackSpeed : 1.0f;
    }
    #endregion

    #region Visual Effects
    private void PlayShootEffects()
    {
        // Player recoil effect
        transform.DOKill();
        transform.DOScale(new Vector3(_targetScale.x * 0.95f, _targetScale.y * 1.05f, _targetScale.z), 0.05f)
            .OnComplete(() => transform.DOScale(_targetScale, 0.05f));

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
