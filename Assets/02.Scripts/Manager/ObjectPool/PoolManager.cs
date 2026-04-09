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

    #region 오브젝트 풀 초기화 로직
    /// <summary>
    /// 게임 데이터를 기반으로 오브젝트 풀을 초기화합니다.
    /// </summary>
    public void AddObjectPool()
    {
        var dataManager = DataManager.Instance;
        if (ObjectPool == null || dataManager == null) return;

        int unitPoolsCreated = 0;
        int enemyPoolsCreated = 0;
        
        // 1. 유닛 풀 초기화
        foreach (var unitData in dataManager.UnitDatas.Values)
        {
            if (unitData.Prefab != null)
            {
                string tag = unitData.ID; 
                ObjectPool.AddObjectPool(tag, unitData.Prefab, 1);
                
                // 새로 생성된 비활성 인스턴스들에 대해 UnitID 초기화
                var pool = ObjectPool.GetPool(tag);
                if (pool != null)
                {
                    foreach (var obj in pool)
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

        // 2. 적 풀 초기화 (각 고유한 적 타입마다 하나씩)
        foreach (var enemyData in dataManager.EnemyDatas.Values)
        {
            if (enemyData.Prefab != null)
            {
                // CSV의 Name을 풀 태그로 사용
                string tag = enemyData.Name;
                
                // 스테이지 간에 동일한 적 이름에 대해 중복 풀 생성을 방지
                if (!ObjectPool.HasPool(tag))
                {
                    // 적절한 양을 미리 생성 (예: 타입당 5개, 일반 적의 경우 더 많이)
                    int initialSize = (enemyData.Type == EntityType.Boss) ? 1 : 5;
                    ObjectPool.AddObjectPool(tag, enemyData.Prefab, initialSize);
                    enemyPoolsCreated++;
                }
            }
        }

        // 3. 글로벌 풀 (총알 등)
        if (_bulletPrefab != null)
        {
            ObjectPool.AddObjectPool(Tag.Bullet, _bulletPrefab, 50);
        }

        if (_damageTextPrefab != null)
        {
            ObjectPool.AddObjectPool(Tag.DamageText, _damageTextPrefab, 20);
        }

        Debug.Log($"[PoolManager] 초기화 완료. 유닛: {unitPoolsCreated}, 적: {enemyPoolsCreated}");
    }

    /// <summary>
    /// 오브젝트 풀에서 유닛 프리팹을 생성합니다.
    /// </summary>
    public PoolObject CreateUnitPrefabs(string tag)
    {
        if (ObjectPool == null) return null;

        PoolObject fruit = ObjectPool.SpawnFromPool(tag);
        if (fruit == null)
        {
            Debug.LogError($"[PoolManager] 오브젝트 풀에서 {tag} 생성에 실패했습니다.");
            return null;
        }

        // Unit 컴포넌트인지 확인 (선택적 검증)
        if (fruit.TryGetComponent<Unit>(out var unit))
        {
            // 풀이 동적으로 확장된 경우, 새로운 오브젝트는 OnEnable 전에 UnitID가 설정되지 않았을 수 있습니다.
            // 하지만 ObjectPool은 반환하기 전에 활성화합니다.
            // 여기서 설정하는 것은 폴백(fallback)입니다.
            if (string.IsNullOrEmpty(unit.UnitID))
            {
                unit.SetupUnit(tag);
            }
            return fruit;
        }

    Debug.LogWarning($"[PoolManager] 생성된 {tag} 오브젝트에 Unit 컴포넌트가 없습니다.");
        return fruit;
    }
    #endregion

    #region 범용 풀링 인터페이스
    /// <summary>
    /// 기본 ObjectPool 인스턴스를 사용하여 풀에서 오브젝트를 생성합니다.
    /// </summary>
    public T Spawn<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
    {
        if (ObjectPool == null) return null;
        return ObjectPool.Spawn<T>(tag, position, rotation);
    }

    /// <summary>
    /// 기본 ObjectPool 인스턴스를 사용하여 오브젝트를 풀로 반납합니다.
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
