using System.Collections.Generic;
using UnityEngine;

public class UnitUIManager : Singleton<UnitUIManager>
{
    private GameManager GM => GameManager.Instance;

    [SerializeField] private GameObject UnitItemPrefab;
    [SerializeField] private Transform UnitListParent;

    private readonly Dictionary<string, UnitItem> _UnitUIItems = new();
    private readonly Dictionary<string, Sprite> _UnitSprites = new();
  
    public void SetUnitData(IReadOnlyDictionary<string, UnitData> UnitData)
    {
        if (UnitData == null) return;

        _UnitSprites.Clear();
        foreach (var pair in UnitData)
        {
            _UnitSprites[pair.Key] = pair.Value.Image;
        }
    }

    public void UpdateUnitCountsUI(Dictionary<string, int> UnitCounts)
    {
        if (UnitCounts == null) return;

        foreach (var pair in UnitCounts)
        {
            if (pair.Value > 0)
                UpdateOrCreateUnitUI(pair.Key, pair.Value);
            else
                RemoveUnitUI(pair.Key);
        }
    }

    public void UpdateOrCreateUnitUI(string id, int count)
    {
        if (_UnitUIItems.TryGetValue(id, out var fruitItem))
        {
            fruitItem.UpdateUnit(id, count, GetUnitImage(id));
        }
        else
        {
            CreateUnitUI(id, count);
        }
    }

    public void CreateUnitUI(string id, int count)
    {
        if (DataManager.Instance == null) return;

        var UnitData = DataManager.Instance.UnitDatas.TryGetValue(id, out var data) ? data : null;
        if (UnitData == null || UnitItemPrefab == null) return;

        var UnitItemObject = Instantiate(UnitItemPrefab, UnitListParent);
        if (UnitItemObject.TryGetComponent<UnitItem>(out var UnitItem))
        {
            UnitItem.UpdateUnit(id, count, UnitData.Image);
            _UnitUIItems[id] = UnitItem;
        }
        else
        {
            Debug.LogError($"[FruitUIManager] FruitItem component not found on prefab for {id}.");
            Destroy(UnitItemObject);
        }
    }

    public void RemoveUnitUI(string id)
    {
        if (!_UnitUIItems.TryGetValue(id, out var UnitItem)) return;

        if (UnitItem != null)
            Destroy(UnitItem.gameObject);
            
        _UnitUIItems.Remove(id);
    }

    public Sprite GetUnitImage(string id)
    {
        if (DataManager.Instance == null) return null;
        return DataManager.Instance.UnitDatas.TryGetValue(id, out var data) ? data.Image : null;
    }
}
