using System;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private GameManager GM => GameManager.Instance;
    protected ObjectPool ObjectPool => ObjectPool.Instance;

    [Header("Global Prefabs")]
    [SerializeField] private PoolObject _bulletPrefab;
    [SerializeField] private PoolObject _damageTextPrefab;
    [SerializeField] private PoolObject _enemyPrefab;

    #region Object Pool Initialization Logic
    /// <summary>
    /// Initializes object pools based on game data.
    /// </summary>
    public void AddObjectPool()
    {
        var dataManager = DataManager.Instance;
        if (ObjectPool == null || dataManager == null) return;

        int unitPoolsCreated = 0;
        int enemyPoolsCreated = 0;
        
        // 1. Initialize Unit Pools
        foreach (var unitData in dataManager.UnitDatas.Values)
        {
            if (unitData.Prefab != null)
            {
                string tag = unitData.ID; 
                ObjectPool.AddObjectPool(tag, unitData.Prefab, 1);
                
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
        }

        // 2. Initialize Enemy Pools (One for each unique enemy type)
        foreach (var enemyData in dataManager.EnemyDatas.Values)
        {
            if (enemyData.Prefab != null)
            {
                // Use the Name from CSV as the pool tag
                string tag = enemyData.Name;
                
                // Avoid creating duplicate pools for the same enemy name across stages
                if (!ObjectPool.PoolDictionary.ContainsKey(tag))
                {
                    // Pre-spawn a reasonable amount (e.g., 5 per type, or more for common enemies)
                    int initialSize = (enemyData.Type == EntityType.Boss) ? 1 : 5;
                    ObjectPool.AddObjectPool(tag, enemyData.Prefab, initialSize);
                    enemyPoolsCreated++;
                }
            }
        }

        // 3. Global Pools (Bullets, etc.)
        if (_bulletPrefab != null)
        {
            ObjectPool.AddObjectPool(Tag.Bullet, _bulletPrefab, 50);
        }

        if (_damageTextPrefab != null)
        {
            ObjectPool.AddObjectPool(Tag.DamageText, _damageTextPrefab, 20);
        }

        Debug.Log($"[PoolManager] Initialization complete. Units: {unitPoolsCreated}, Enemies: {enemyPoolsCreated}");
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

    #region Generic Pooling Interface
    /// <summary>
    /// Spawns an object from the pool using the primary ObjectPool instance.
    /// </summary>
    public T Spawn<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
    {
        if (ObjectPool == null) return null;
        return ObjectPool.Spawn<T>(tag, position, rotation);
    }

    /// <summary>
    /// Returns an object to the pool using the primary ObjectPool instance.
    /// </summary>
    public void ReturnObject(string tag, PoolObject obj)
    {
        if (ObjectPool == null)
        {
            obj.gameObject.SetActive(false);
            return;
        }
        ObjectPool.ReturnObject(tag, obj);
    }
    #endregion
}
