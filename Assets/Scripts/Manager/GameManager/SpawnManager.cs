using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private GameObject boss;
    public GameObject Boss => boss;

    [SerializeField] private float minSpawnDistance = 2f;
    [SerializeField] private float maxSpawnDistance = 5f;

    private Boss _currentBoss;

    /// <summary>
    /// Spawns a fruit from the object pool near the boss.
    /// </summary>
    public void SpawnFruitFromPool(FruitsID fruitID)
    {
        string tag = fruitID.ToString();
        
        if (PoolManager.Instance == null) return;

        PoolObject fruit = PoolManager.Instance.CreateUnitPrefabs(tag);
        if (fruit != null)
        {
            Vector3 centerPos = boss != null ? boss.transform.position : Vector3.zero;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 spawnPosition = centerPos + new Vector3(randomDir.x * distance, randomDir.y * distance, 0);

            fruit.transform.position = spawnPosition;
            fruit.transform.rotation = Quaternion.identity;
            fruit.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[SpawnManager] Failed to spawn {fruitID} from pool.");
        }
    }

    public void ReturnAllFruitsToPool()
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnAllObjects();
        }
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
