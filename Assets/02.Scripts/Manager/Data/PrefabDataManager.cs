using System.Collections.Generic;
using UnityEngine;

public class PrefabDataManager : Singleton<PrefabDataManager>
{
    /// <summary>
    /// Saves the current state of active pooled objects to a JSON file.
    /// </summary>
    public void SavePrefabData()
    {
        if (ObjectPool.Instance == null || SaveManager.Instance == null) return;

        var prefabDataList = new List<PrefabData>();
        int unitLayer = LayerMask.NameToLayer(Layer.Unit);

        foreach (var pool in ObjectPool.Instance.PoolDictionary.Values)
        {
            foreach (var obj in pool)
            {
                if (obj != null && obj.gameObject.activeInHierarchy && obj.gameObject.layer == unitLayer)
                {
                    // Remove "(Clone)" suffix from name for consistency
                    string cleanName = obj.name.Replace("(Clone)", "").Trim();
                    
                    prefabDataList.Add(new PrefabData(
                        cleanName,
                        obj.transform.position,
                        obj.transform.rotation
                    ));
                }
            }
        }

        SaveManager.Instance.SaveData(prefabDataList);
    }

    /// <summary>
    /// Loads saved prefab state and spawns objects from the pool.
    /// </summary>
    public void LoadPrefabData()
    {
        if (ObjectPool.Instance == null || SaveManager.Instance == null) return;

        if (SaveManager.Instance.TryLoadData(out List<PrefabData> prefabDataList))
        {
            foreach (var prefabData in prefabDataList)
            {
                string cleanKey = prefabData.prefabName.Trim();
                PoolObject obj = ObjectPool.Instance.SpawnFromPool(cleanKey);

                if (obj != null)
                {
                    obj.transform.position = prefabData.position.ToVector3();
                    obj.transform.rotation = prefabData.rotation.ToQuaternion();
                    obj.gameObject.SetActive(true);
                }
            }
        }
    }
}
