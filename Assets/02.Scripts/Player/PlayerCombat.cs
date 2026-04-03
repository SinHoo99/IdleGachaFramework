using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private GameObject _parryEffectPrefab;

    private bool _isParrying;
    public bool IsParrying => _isParrying;

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
        
        float range = _attackRange;
        if (PlayerDataManager.Instance?.NowPlayerData != null)
        {
            range = PlayerDataManager.Instance.NowPlayerData.AttackRange;
        }

        // 레이캐스트를 이용해 전방에 적이 있는지 확인합니다.
        RaycastHit2D hit = Physics2D.Raycast(_firePoint.position, Vector2.right, range, _enemyLayer);
        return hit.collider != null;
    }

    public void PerformShoot()
    {
        if (_firePoint == null) return;
        
        Vector2 direction = Vector2.right; // 2D 방치형 가로 방향
        if (PoolManager.Instance != null)
        {
            var bullet = PoolManager.Instance.Spawn<Bullet>(Tag.Bullet, _firePoint.position, Quaternion.identity);
            if (bullet != null)
            {
                float damage = 50f; // 기본값
                if (PlayerDataManager.Instance?.NowPlayerData != null)
                {
                    damage = PlayerDataManager.Instance.NowPlayerData.Damage;
                }
                bullet.Setup(direction, gameObject.tag, damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_firePoint == null) return;
        Gizmos.color = Color.red;

        float range = _attackRange;
        if (PlayerDataManager.Instance?.NowPlayerData != null)
        {
            range = PlayerDataManager.Instance.NowPlayerData.AttackRange;
        }

        // 사거리를 선으로 표시합니다.
        Gizmos.DrawLine(_firePoint.position, _firePoint.position + Vector3.right * range);
    }
}
