using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    private readonly Dictionary<string, Queue<PoolObject>> _poolDictionary = new();
    private readonly Dictionary<string, PoolObject> _prefabDictionary = new();

    /// <summary>
    /// 특정 태그에 대한 새로운 오브젝트 풀을 생성합니다.
    /// </summary>
    public void AddObjectPool(string tag, PoolObject prefab, int size)
    {
        if (string.IsNullOrEmpty(tag) || prefab == null) return;

        if (_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPool] 태그 '{tag}'의 풀이 이미 존재합니다.");
            return;
        }

        _prefabDictionary[tag] = prefab;
        var objectPool = new Queue<PoolObject>();

        for (int i = 0; i < size; i++)
        {
            PoolObject obj = CreateNewObject(prefab);
            objectPool.Enqueue(obj);
        }

        _poolDictionary.Add(tag, objectPool);
    }

    /// <summary>
    /// 풀에서 오브젝트를 꺼내 특정 위치와 회전값으로 배치합니다.
    /// 지정된 컴포넌트 타입을 반환합니다.
    /// </summary>
    public T Spawn<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
    {
        PoolObject obj = SpawnFromPool(tag);
        if (obj != null)
        {
            obj.OnSpawn(position, rotation);
            if (obj.TryGetComponent<T>(out var component))
            {
                return component;
            }
        }
        return null;
    }

    /// <summary>
    /// 풀에서 오브젝트를 꺼냅니다. 사용할 수 있는 오브젝트가 없으면 새로 생성하여 풀을 확장합니다.
    /// </summary>
    public PoolObject SpawnFromPool(string tag)
    {
        if (!_poolDictionary.TryGetValue(tag, out var queue))
        {
            Debug.LogWarning($"[ObjectPool] 태그 '{tag}'에 해당하는 풀을 찾을 수 없습니다.");
            return null;
        }

        PoolObject targetObj;

        if (queue.Count > 0)
        {
            targetObj = queue.Dequeue();
        }
        else
        {
            // 풀이 비어있으면 확장
            if (_prefabDictionary.TryGetValue(tag, out var prefab))
            {
                targetObj = CreateNewObject(prefab);
            }
            else
            {
                Debug.LogError($"[ObjectPool] 풀을 확장하기 위한 태그 '{tag}'의 프리팹을 찾을 수 없습니다.");
                return null;
            }
        }

        targetObj.gameObject.SetActive(true);
        return targetObj;
    }

    /// <summary>
    /// 오브젝트를 비활성화하고 리셋하여 풀로 반납합니다.
    /// </summary>
    public void ReturnObject(string tag, PoolObject obj)
    {
        if (!_poolDictionary.TryGetValue(tag, out var queue))
        {
            Debug.LogWarning($"[ObjectPool] 태그 '{tag}'의 풀을 찾을 수 없습니다. 오브젝트를 파괴합니다.");
            Destroy(obj.gameObject);
            return;
        }

        if (obj != null)
        {
            obj.OnReturnToPool();
            obj.gameObject.SetActive(false);
            
            // 계층 구조를 깔끔하게 유지하기 위해 풀 트랜스폼 하위로 이동
            obj.transform.SetParent(this.transform);
            queue.Enqueue(obj);
        }
    }

    /// <summary>
    /// 모든 풀의 모든 오브젝트를 반납 처리합니다.
    /// </summary>
    public void ReturnAllObjects()
    {
        // 현재 활성화된 모든 PoolObject 자식들을 찾아 반납 처리
        PoolObject[] activeObjects = GetComponentsInChildren<PoolObject>(false);
        int totalReturned = 0;
        
        foreach (var obj in activeObjects)
        {
            if (obj.gameObject.activeInHierarchy)
            {
                obj.gameObject.SetActive(false);
                totalReturned++;
            }
        }
        Debug.Log($"[ObjectPool] 모든 오브젝트 비활성화 완료. 총합: {totalReturned}");
    }

    private PoolObject CreateNewObject(PoolObject prefab)
    {
        PoolObject obj = Instantiate(prefab, transform);
        
        // TMP 월드 공간 텍스트의 CanvasRenderer 경고 방지
        if (obj.GetComponent<TMPro.TextMeshPro>() != null)
        {
            var canvasRenderer = obj.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
            {
                DestroyImmediate(canvasRenderer);
            }
        }

        obj.gameObject.SetActive(false);
        return obj;
    }

    // PoolManager 초기화를 위한 헬퍼 메서드
    public IEnumerable<PoolObject> GetPool(string tag)
    {
        if (_poolDictionary.TryGetValue(tag, out var queue))
        {
            return queue;
        }
        return null;
    }

    public bool HasPool(string tag) => _poolDictionary.ContainsKey(tag);

    /// <summary>
    /// 등록된 모든 풀 큐를 반환합니다.
    /// </summary>
    public IEnumerable<Queue<PoolObject>> GetAllPools() => _poolDictionary.Values;
}
