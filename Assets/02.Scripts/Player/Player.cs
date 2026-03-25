using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;

    #region Shooting Logic
    public Bullet CreateBullet(string tag, Vector2 position, Vector2 direction, string ownerTag)
    {
        if (ObjectPool.Instance == null) return null;

        var bullet = ObjectPool.Instance.Spawn<Bullet>(tag, position, Quaternion.identity);
        if (bullet != null)
        {
            float damage = UnityEngine.Random.Range(10f, 20f); // Example damage, replace with actual logic
            bullet.Setup(direction, ownerTag, damage);
            return bullet;
        }
        return null;
    }

    public void ShootBullet()
    {
        Vector2 direction = new Vector2(1, 0); // Example direction, replace with actual aiming logic
        CreateBullet(Tag.Bullet, _firePoint.position, direction, gameObject.tag);
    }
    #endregion
}