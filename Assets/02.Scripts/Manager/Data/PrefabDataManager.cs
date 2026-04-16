using System.Collections.Generic;
using UnityEngine;

public class PrefabDataManager : Singleton<PrefabDataManager>
{
    /// <summary>
    /// 현재 활성화된 풀링된 오브젝트의 상태를 JSON 파일로 저장합니다.
    /// </summary>
    public void SavePrefabData()
    {
        if (ObjectPool.Instance == null || SaveManager.Instance == null) return;

        var prefabDataList = new List<PrefabData>();
        int unitLayer = LayerMask.NameToLayer(Layer.Unit);

        foreach (var pool in ObjectPool.Instance.GetAllPools())
        {
            foreach (var obj in pool)
            {
                if (obj != null && obj.gameObject.activeInHierarchy && obj.gameObject.layer == unitLayer)
                {
                    // 일관성을 위해 이름에서 "(Clone)" 접미사 제거
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
    /// 저장된 프리팹 상태를 로드하고 풀에서 오브젝트를 생성합니다.
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
