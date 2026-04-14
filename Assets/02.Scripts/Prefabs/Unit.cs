using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Unit : PoolObject
{
    private GameManager GM => GameManager.Instance;

    public string UnitID { get; private set; } = string.Empty;

    [SerializeField] private Transform _firePoint;

    private Enemy _targetEnemy;
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
    }

    private void OnEnable()
    {
        UpdateTarget();

        if (_shootCoroutine != null) StopCoroutine(_shootCoroutine);
        _shootCoroutine = StartCoroutine(ShootCoroutine());
    }

    public void UpdateScale()
    {
        if (string.IsNullOrEmpty(UnitID)) return;

        float currentLevel = GetCurrentUnitLevel();
        float scaleMultiplier = Mathf.Min(2.0f, 1.0f + (currentLevel - 1) * 0.1f);
        _targetScale = _baseScale * scaleMultiplier;
        transform.localScale = _targetScale;
    }

    private int GetCurrentUnitLevel()
    {
        if (PlayerDataManager.Instance?.NowPlayerData?.Inventory != null && 
            PlayerDataManager.Instance.NowPlayerData.Inventory.TryGetValue(UnitID, out var collectedData))
        {
            return collectedData.Amount;
        }
        return 1;
    }

    private void OnDisable()
    {
        if (_shootCoroutine != null)
        {
            StopCoroutine(_shootCoroutine);
            _shootCoroutine = null;
        }
    }

    private void UpdateTarget()
    {
        if (_targetEnemy != null && _targetEnemy.gameObject.activeInHierarchy) return;

        if (SpawnManager.Instance != null && SpawnManager.Instance.ActiveEnemies.Count > 0)
        {
            _targetEnemy = SpawnManager.Instance.ActiveEnemies[0];
        }
        else
        {
            _targetEnemy = null;
        }
    }

    #region Shooting Logic
    private IEnumerator ShootCoroutine()
    {
        while (true)
        {
            UpdateTarget();
            float attackSpeed = GetAttackSpeed();
            float randomVariance = Random.Range(-0.3f, 0.3f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, attackSpeed + randomVariance));
            ShootBullet();
        }
    }

    public void ShootBullet() // public으로 변경하여 애니메이션 이벤트 대응 가능하게 함
    {
        UpdateTarget();
        if (_targetEnemy == null || !_targetEnemy.gameObject.activeInHierarchy) return;

        Vector2 direction = (_targetEnemy.transform.position - _firePoint.position).normalized;
        
        if (PoolManager.Instance != null)
        {
            var bullet = PoolManager.Instance.Spawn<Bullet>(Tag.Bullet, _firePoint.position, Quaternion.identity);
            if (bullet != null)
            {
                bullet.Setup(direction, gameObject.tag, GetBulletDamage());
            }
        }
        
        PlayLimitedSFX();
    }

    private float GetBulletDamage()
    {
        var data = DataManager.Instance.GetUnitData(UnitID);
        if (data == null) return 0f;

        return data.Damage * 0.1f * GetCurrentUnitLevel();
    }

    private float GetAttackSpeed()
    {
        var data = DataManager.Instance.GetUnitData(UnitID);
        return data != null ? data.AttackSpeed : 1.0f;
    }
    #endregion

    #region Visual Effects
    public void UpgradeEffect()
    {
        transform.DOKill();
        
        float currentLevel = GetCurrentUnitLevel();
        float scaleMultiplier = Mathf.Min(2.0f, 1.0f + (currentLevel - 1) * 0.1f);
        _targetScale = _baseScale * scaleMultiplier;

        transform.DOScale(_targetScale * 1.2f, 0.1f)
            .OnComplete(() => transform.DOScale(_targetScale, 0.1f));
        
        if (GM != null) GM.PlaySFX(SFX.Upgrade);
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
