using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private float fixedY = -3.5f;
    [SerializeField] private float _bossXOffset = -2f; // Offset to bring stationary bosses on-screen
    [SerializeField] private Transform _enemySpawnPoint;
    [SerializeField] private Transform _bossSpawnPoint;

    private Dictionary<string, Unit> _activeUnits = new();
    private List<Enemy> _activeEnemies = new();

    public IReadOnlyList<Enemy> ActiveEnemies => _activeEnemies;

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
        ReturnAllEnemiesToPool();
        
        // Clear tracking collections
        _activeUnits.Clear();
        _activeEnemies.Clear();

        if (PrefabDataManager.Instance != null)
        {
            PrefabDataManager.Instance.SavePrefabData();
        }

        Debug.Log("[SpawnManager] Visual reset complete and saved.");
    }

    #region Unit Management
    public void SpawnUnitFromPool(string UnitID)
    {
        if (PoolManager.Instance == null) return;

        if (_activeUnits.TryGetValue(UnitID, out var existingUnit) && existingUnit.gameObject.activeInHierarchy)
        {
            existingUnit.UpgradeEffect();
            return;
        }

        PoolObject fruit = PoolManager.Instance.CreateUnitPrefabs(UnitID);
        if (fruit != null)
        {
            float randomX = Random.Range(-2f, 0f);
            Vector3 spawnPosition = new Vector3(randomX, fixedY, 0);

            fruit.transform.position = spawnPosition;
            fruit.transform.rotation = Quaternion.identity;
            fruit.gameObject.SetActive(true);

            if (fruit.TryGetComponent<Unit>(out var unit))
            {
                unit.SetupUnit(UnitID);
                _activeUnits[UnitID] = unit;
            }
        }
    }

    public void RemoveUnitFromField(string UnitID)
    {
        if (_activeUnits.TryGetValue(UnitID, out var unit))
        {
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.ReturnObject(UnitID, unit);
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
        if (PoolManager.Instance == null) return;

        foreach (var unit in _activeUnits.Values)
        {
            if (unit != null) PoolManager.Instance.ReturnObject(unit.UnitID, unit);
        }
        _activeUnits.Clear();
    }
    #endregion

    #region Enemy Management
    public void SpawnEnemy(int stage)
    {
        if (PoolManager.Instance == null) return;

        var enemyData = DataManager.Instance.GetEnemyData(stage);
        if (enemyData == null) return;

        Vector3 spawnPos;
        if (enemyData.Type == EntityType.Boss && _bossSpawnPoint != null)
        {
            // Apply X offset for bosses to bring them on-screen
            spawnPos = _bossSpawnPoint.position + new Vector3(_bossXOffset, 0f, 0f);
        }
        else
        {
            spawnPos = (_enemySpawnPoint != null ? _enemySpawnPoint.position : new Vector3(10f, fixedY, 0f));
        }
        
        var enemy = PoolManager.Instance.Spawn<Enemy>(enemyData.Name, spawnPos, Quaternion.identity);
        if (enemy != null)
        {
            enemy.Setup(enemyData);
            _activeEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Remove(enemy);
        }
    }

    public void ReturnAllEnemiesToPool()
    {
        if (PoolManager.Instance == null) return;

        // Use a temporary list to avoid modification during enumeration
        var enemiesToReturn = new List<Enemy>(_activeEnemies);
        foreach (var enemy in enemiesToReturn)
        {
            if (enemy != null)
            {
                // Internal Enemy.HandleDeath or ReturnToPool should handle unregistering
                PoolManager.Instance.ReturnObject(enemy.gameObject.name.Replace("(Clone)", "").Trim(), enemy);
            }
        }
        _activeEnemies.Clear();
    }
    #endregion

    #region Initial Spawning
    /// <summary>
    /// Spawns all units from the saved inventory data.
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
    #endregion
}
