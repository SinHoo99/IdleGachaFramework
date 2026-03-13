using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private GameObject boss;
    public GameObject Boss => boss;

    [SerializeField] private float minSpawnDistance = -2f;
    [SerializeField] private float maxSpawnDistance = 0f;

    private Boss _currentBoss;

    private Dictionary<string, Unit> _activeUnits = new();

    private void OnEnable()
    {
        EventBus.Subscribe(GameEventType.OnDataReset, HandleDataReset);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(GameEventType.OnDataReset, HandleDataReset);
    }

    private void HandleDataReset()
    {
        Debug.Log("[SpawnManager] Received OnDataReset event. Clearing all visuals...");
        StopAllCoroutines();
        ReturnAllUnitToPool();
        
        // Clear active units tracking
        _activeUnits.Clear();

        // Immediately update prefab data to reflect that there are no active units on field
        // This prevents old, out-of-range positions from being loaded later.
        if (PrefabDataManager.Instance != null)
        {
            PrefabDataManager.Instance.SavePrefabData();
        }

        // Force cleanup any remaining active objects just in case
        Unit[] lingeringUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (var unit in lingeringUnits)
        {
            unit.gameObject.SetActive(false);
            unit.OnReturnToPool();
        }

        GetCurrentBoss()?.ResetBossData();
        Debug.Log("[SpawnManager] Visual reset complete and saved.");
    }

    /// <summary>
    /// Spawns a fruit from the object pool near the boss.
    /// If it already exists, calls UpgradeEffect on the existing unit.
    /// </summary>
    public void SpawnUnitFromPool(string UnitID)
    {
        if (PoolManager.Instance == null) return;

        // If unit already exists on field, don't spawn a new one, just upgrade it
        if (_activeUnits.TryGetValue(UnitID, out var existingUnit) && existingUnit.gameObject.activeInHierarchy)
        {
            existingUnit.UpgradeEffect();
            return;
        }

        PoolObject fruit = PoolManager.Instance.CreateUnitPrefabs(UnitID);
        if (fruit != null)
        {
            // Y is fixed at -3.5 as requested
            float fixedY = -3.5f;
            
            // X is limited to -2 to 0 as requested
            float randomX = Random.Range(-2f, 0f);
            Vector3 spawnPosition = new Vector3(randomX, fixedY, 0);

            fruit.transform.position = spawnPosition;
            fruit.transform.rotation = Quaternion.identity;
            
            // Removed forcing localScale to Vector3.one to preserve prefab scale

            fruit.gameObject.SetActive(true);

            if (fruit.TryGetComponent<Unit>(out var unit))
            {
                unit.SetupUnit(UnitID);
                _activeUnits[UnitID] = unit;
            }
        }
        else
        {
            Debug.LogWarning($"[SpawnManager] Failed to spawn {UnitID} from pool.");
        }
    }

    public void RemoveUnitFromField(string UnitID)
    {
        if (_activeUnits.TryGetValue(UnitID, out var unit))
        {
            if (ObjectPool.Instance != null)
            {
                ObjectPool.Instance.ReturnObject(UnitID, unit);
            }
            else
            {
                unit.gameObject.SetActive(false);
            }
            _activeUnits.Remove(UnitID);
        }
    }

    public void ReturnAllUnitToPool()
    {
        // Try standard pool return first
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnAllObjects();
        }

        _activeUnits.Clear();

        // Backup cleanup: Find all active Unit objects in the scene and return them to pool
        Unit[] activeUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        int forceReturned = 0;
        foreach (var unit in activeUnits)
        {
            if (unit.gameObject.activeInHierarchy)
            {
                unit.gameObject.SetActive(false);
                unit.OnReturnToPool();
                forceReturned++;
            }
        }

        Debug.Log($"[SpawnManager] Cleaned all fruits. Pool return + {forceReturned} units force deactivated.");
    }

    /// <summary>
    /// Spawns all fruits from the saved inventory data.
    /// Call this during game initialization.
    /// </summary>
    public void SpawnInitialUnits()
    {
        StartCoroutine(SpawnInitialUnitsCoroutine());
    }

    private IEnumerator SpawnInitialUnitsCoroutine()
    {
        if (PlayerDataManager.Instance?.NowPlayerData?.Inventory == null) yield break;

        int totalTypes = 0;
        foreach (var item in PlayerDataManager.Instance.NowPlayerData.Inventory.Values)
        {
            if (item.Amount > 0)
            {
                SpawnUnitFromPool(item.ID);
                totalTypes++;
                yield return new WaitForSeconds(0.2f); // Spawn one by one with delay
            }
        }
        Debug.Log($"[SpawnManager] Initialized field with {totalTypes} unit types sequentially.");
    }

    public Boss GetCurrentBoss()
    {
        if (_currentBoss == null)
        {
            _currentBoss = FindAnyObjectByType<Boss>();
        }
        return _currentBoss;
    }
}
