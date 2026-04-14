using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _defaultDamage = 50f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private GameObject _parryEffectPrefab;
    [SerializeField] private bool _showDebugRay = true;

    private bool _isParrying;
    public bool IsParrying => _isParrying;

    private int _currentShotCount = 0;

    public void ResetShotCount()
    {
        _currentShotCount = 0;
    }

    public void StartParry()
    {
        if (_isParrying) return;
        
        _isParrying = true;
        if (_parryEffectPrefab != null) _parryEffectPrefab.SetActive(true);
        Debug.Log("<color=cyan>[PlayerCombat] Parry Started</color>");
    }

    public void EndParry()
    {
        if (!_isParrying) return;

        _isParrying = false;
        if (_parryEffectPrefab != null) _parryEffectPrefab.SetActive(false);
        Debug.Log("<color=cyan>[PlayerCombat] Parry Ended</color>");
    }

    public bool DetectEnemy()
    {
        if (_firePoint == null) return false;
        
        float range = 5f; 
        if (PlayerDataManager.HasInstance && PlayerDataManager.Instance.NowPlayerData != null)
        {
            range = PlayerDataManager.Instance.NowPlayerData.AttackRange;
        }

        if (_showDebugRay)
        {
            Debug.DrawRay(_firePoint.position, Vector2.right * range, Color.yellow);
        }

        RaycastHit2D hit = Physics2D.Raycast(_firePoint.position, Vector2.right, range, _enemyLayer);
        return hit.collider != null;
    }

    public void PerformShoot()
    {
        if (_firePoint == null) return;
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.NowPlayerData == null) return;

        int maxShots = PlayerDataManager.Instance.NowPlayerData.MultiShotCount;

        if (_currentShotCount < maxShots)
        {
            Vector2 direction = Vector2.right;
            if (PoolManager.Instance != null)
            {
                var bullet = PoolManager.Instance.Spawn<Bullet>(Tag.Bullet, _firePoint.position, Quaternion.identity);
                if (bullet != null)
                {
                    float damage = PlayerDataManager.Instance.NowPlayerData.Damage;
                    float range = PlayerDataManager.Instance.NowPlayerData.AttackRange;
                    bullet.Setup(direction, gameObject.tag, damage, range);
                    
                    _currentShotCount++;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_firePoint == null) return;
        Gizmos.color = Color.red;

        float range = 5f;
        if (PlayerDataManager.HasInstance && PlayerDataManager.Instance.NowPlayerData != null)
        {
            range = PlayerDataManager.Instance.NowPlayerData.AttackRange;
        }

        Gizmos.DrawLine(_firePoint.position, _firePoint.position + Vector3.right * range);
    }
}
