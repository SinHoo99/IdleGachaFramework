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
        var dataManager = DataManager.Instance;
        if (ObjectPool == null || dataManager == null) return;

        int unitPoolsCreated = 0;
        
        // Use pre-loaded UnitDatas from DataManager
        foreach (var unitData in dataManager.UnitDatas.Values)
        {
            if (unitData.Prefab != null)
            {
                string tag = unitData.ID; // Already a string
                ObjectPool.AddObjectPool(tag, unitData.Prefab, 20);
                
                // Initialize UnitID for all newly created inactive instances
                if (ObjectPool.PoolDictionary.TryGetValue(tag, out var list))
                {
                    foreach (var obj in list)
                    {
                        if (obj.TryGetComponent<Unit>(out var unit))
                        {
                            unit.SetupUnit(unitData.ID);
                        }
                    }
                }
                
                unitPoolsCreated++;
            }
            else
            {
                Debug.LogWarning($"[PoolManager] Skipping pool for {unitData.ID}: Prefab is null.");
            }
        }

        // Add bullet pool
        var bulletPrefab = GameManager.Instance.GetBullet();
        if (bulletPrefab != null)
        {
            ObjectPool.AddObjectPool(Tag.Bullet, bulletPrefab, 50);
            Debug.Log($"[PoolManager] Bullet pool created.");
        }
        else
        {
            Debug.LogWarning("[PoolManager] Bullet prefab not found in GameManager. Bullet pool NOT created.");
        }

        Debug.Log($"[PoolManager] Initialization complete. Total Unit pools created: {unitPoolsCreated}");
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
            // If the pool expanded dynamically, new objects might not have UnitID set before OnEnable.
            // But ObjectPool activates them before returning. 
            // Setting it here is a fallback.
            if (string.IsNullOrEmpty(unit.UnitID))
            {
                unit.SetupUnit(tag);
            }
            return fruit;
        }

        Debug.LogWarning($"[PoolManager] Spawned {tag} object does not have a Unit component.");
        return fruit;
    }
    #endregion
}
