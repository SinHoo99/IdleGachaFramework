using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    private readonly Dictionary<string, List<PoolObject>> _poolDictionary = new();

    public Dictionary<string, List<PoolObject>> PoolDictionary => _poolDictionary;

    /// <summary>
    /// Creates a new object pool for a specific tag.
    /// </summary>
    public void AddObjectPool(string tag, PoolObject prefab, int size)
    {
        if (string.IsNullOrEmpty(tag) || prefab == null) return;

        if (_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPool] Pool with tag '{tag}' already exists.");
            return;
        }

        var objectPool = new List<PoolObject>();
        for (int i = 0; i < size; i++)
        {
            PoolObject obj = CreateNewObject(prefab);
            objectPool.Add(obj);
        }

        _poolDictionary.Add(tag, objectPool);
    }

    /// <summary>
    /// Spawns an object from the pool. Creates a new one if none are available.
    /// </summary>
    public PoolObject SpawnFromPool(string tag)
    {
        if (!_poolDictionary.TryGetValue(tag, out var list) || list.Count == 0)
        {
            Debug.LogWarning($"[ObjectPool] Pool for tag '{tag}' not found or empty.");
            return null;
        }

        foreach (var obj in list)
        {
            if (obj != null && !obj.gameObject.activeInHierarchy)
            {
                obj.gameObject.SetActive(true);
                return obj;
            }
        }

        // Expand pool if all are active
        PoolObject newObj = CreateNewObject(list[0]);
        newObj.gameObject.SetActive(true);
        list.Add(newObj);
        return newObj;
    }

    /// <summary>
    /// Finds an active object by tag.
    /// </summary>
    public PoolObject FindActiveObject(string tag)
    {
        if (!_poolDictionary.TryGetValue(tag, out var list)) return null;

        foreach (var obj in list)
        {
            if (obj != null && obj.gameObject.activeInHierarchy)
            {
                return obj;
            }
        }
        return null;
    }

    /// <summary>
    /// Deactivates and resets an object, returning it to the pool.
    /// </summary>
    public void ReturnObject(string tag, PoolObject obj)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPool] No pool found for tag '{tag}'.");
            return;
        }

        if (obj != null)
        {
            obj.OnReturnToPool();
            obj.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Returns all objects in all pools.
    /// </summary>
    public void ReturnAllObjects()
    {
        foreach (var list in _poolDictionary.Values)
        {
            foreach (var obj in list)
            {
                if (obj != null && obj.gameObject.activeInHierarchy)
                {
                    obj.OnReturnToPool();
                    obj.gameObject.SetActive(false);
                }
            }
        }
        Debug.Log("[ObjectPool] All objects returned to pool.");
    }

    private PoolObject CreateNewObject(PoolObject prefab)
    {
        PoolObject obj = Instantiate(prefab, transform);
        obj.gameObject.SetActive(false);
        return obj;
    }
}
