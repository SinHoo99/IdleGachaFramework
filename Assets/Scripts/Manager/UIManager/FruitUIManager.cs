using System.Collections.Generic;
using UnityEngine;

public class FruitUIManager : Singleton<FruitUIManager>
{
    private GameManager GM => GameManager.Instance;

    [SerializeField] private GameObject fruitItemPrefab;
    [SerializeField] private Transform fruitListParent;

    private readonly Dictionary<FruitsID, FruitItem> _fruitUIItems = new();
    private readonly Dictionary<FruitsID, Sprite> _fruitSprites = new();
  
    public void SetFruitData(Dictionary<FruitsID, FruitsData> fruitData)
    {
        if (fruitData == null) return;

        _fruitSprites.Clear();
        foreach (var (id, data) in fruitData)
        {
            _fruitSprites[id] = data.Image;
        }
    }

    public void UpdateFruitCountsUI(Dictionary<FruitsID, int> fruitCounts)
    {
        if (fruitCounts == null) return;

        foreach (var (fruitID, count) in fruitCounts)
        {
            if (count > 0)
                UpdateOrCreateFruitUI(fruitID, count);
            else
                RemoveFruitUI(fruitID);
        }
    }

    public void UpdateOrCreateFruitUI(FruitsID id, int count)
    {
        if (_fruitUIItems.TryGetValue(id, out var fruitItem))
        {
            fruitItem.UpdateFruit(id, count, GetFruitImage(id));
        }
        else
        {
            CreateFruitUI(id, count);
        }
    }

    public void CreateFruitUI(FruitsID id, int count)
    {
        if (DataManager.Instance == null) return;

        var fruitData = DataManager.Instance.FruitDatas.TryGetValue(id, out var data) ? data : null;
        if (fruitData == null || fruitItemPrefab == null) return;

        var fruitItemObject = Instantiate(fruitItemPrefab, fruitListParent);
        if (fruitItemObject.TryGetComponent<FruitItem>(out var fruitItem))
        {
            fruitItem.UpdateFruit(id, count, fruitData.Image);
            _fruitUIItems[id] = fruitItem;
        }
        else
        {
            Debug.LogError($"[FruitUIManager] FruitItem component not found on prefab for {id}.");
            Destroy(fruitItemObject);
        }
    }

    public void RemoveFruitUI(FruitsID id)
    {
        if (!_fruitUIItems.TryGetValue(id, out var fruitItem)) return;

        if (fruitItem != null)
            Destroy(fruitItem.gameObject);
            
        _fruitUIItems.Remove(id);
    }

    public Sprite GetFruitImage(FruitsID id)
    {
        if (DataManager.Instance == null) return null;
        return DataManager.Instance.FruitDatas.TryGetValue(id, out var data) ? data.Image : null;
    }
}
