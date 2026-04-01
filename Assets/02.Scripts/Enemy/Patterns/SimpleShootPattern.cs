using UnityEngine;

/// <summary>
/// A simple shooting pattern that fires a bullet towards the player.
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

        // In this game, player is usually to the left or at a fixed position
        // We can target the Player singleton directly
        if (Player.Instance == null) return;

        Vector2 direction = (Player.Instance.transform.position - owner.transform.position).normalized;
        
        // Use PoolManager to spawn a bullet
        // Assuming there's a projectile or enemy bullet tag in Define.cs
        var bullet = PoolManager.Instance.Spawn<Bullet>(Tag.Bullet, owner.transform.position, Quaternion.identity);
        if (bullet != null)
        {
            bullet.Setup(direction, owner.gameObject.tag, Damage);
        }
    }
}
