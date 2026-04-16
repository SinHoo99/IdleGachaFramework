using UnityEngine;

/// <summary>
/// 플레이어를 향해 총알을 발사하는 간단한 사격 패턴입니다.
/// </summary>
[CreateAssetMenu(fileName = "SimpleShootPattern", menuName = "Enemy/Patterns/SimpleShoot")]
public class SimpleShootPattern : EnemyAttackPattern
{
    [Header("Shoot Settings")]
    public float Damage = 10f;
    public float BulletSpeed = 10f;

    public override void Execute(Enemy owner)
    {
        if (owner == null) return;

        // 이 게임에서 플레이어는 보통 왼쪽이나 고정된 위치에 있습니다.
        // 플레이어 싱글톤을 직접 타겟팅할 수 있습니다.
        if (Player.Instance == null) return;

        Vector2 direction = (Player.Instance.transform.position - owner.transform.position).normalized;
        
        // PoolManager를 사용하여 총알을 생성합니다.
        // Define.cs에 발사체 또는 적 총알 태그가 있다고 가정합니다.
        var bullet = PoolManager.Instance.Spawn<Bullet>(Tag.Bullet, owner.transform.position, Quaternion.identity);
        if (bullet != null)
        {
            bullet.Setup(direction, owner.gameObject.tag, Damage);
        }
    }
}
