using System;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private GameManager GM => GameManager.Instance;
    protected ObjectPool ObjectPool => ObjectPool.Instance;

    #region Object Pool Initialization Logic
    /// <summary>
    /// Initializes object pools based on game data.
    /// </summary>
    public void AddObjectPool()
    {
        if (ObjectPool == null || DataManager.Instance == null) return;

        // Automatically add pools for all FruitsID enums defined in data
        foreach (FruitsID id in Enum.GetValues(typeof(FruitsID)))
        {
            if (id == FruitsID.None) continue;

            var fruitData = GameManager.Instance.GetFruitsData(id);
            if (fruitData != null && fruitData.Prefab != null)
            {
                ObjectPool.AddObjectPool(id.ToString(), fruitData.Prefab, 20);
            }
        }

        // Add bullet pool
        if (GameManager.Instance.GetBullet() != null)
        {
            ObjectPool.AddObjectPool(Tag.Bullet, GameManager.Instance.GetBullet(), 50);
        }
    }

    /// <summary>
    /// Spawns a unit prefab from the object pool.
    /// </summary>
    public PoolObject CreateUnitPrefabs(string tag)
    {
        if (ObjectPool == null) return null;

        PoolObject fruit = ObjectPool.SpawnFromPool(tag);
        if (fruit == null)
        {
            Debug.LogError($"[PoolManager] Failed to spawn {tag} from object pool.");
            return null;
        }

        // Ensure it's a Unit component (optional verification)
        if (fruit.TryGetComponent<Unit>(out var unit))
        {
            return fruit;
        }

        Debug.LogWarning($"[PoolManager] Spawned {tag} object does not have a Unit component.");
        return fruit;
    }
    #endregion
}
