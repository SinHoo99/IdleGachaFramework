using System.Collections.Generic;
using UnityEngine;

public class DictionaryManager : MonoBehaviour
{
    private GameManager GM => GameManager.Instance;

    [SerializeField] private GameObject fruitDictionaryPrefab; 
    [SerializeField] private Transform dictionaryContent; 

    private readonly Dictionary<FruitsID, FruitDictionaryItem> _fruitDictionaryItems = new();

    private void Start()
    {
        if (GM == null || GM.DataManager == null || GM.ScoreUpdater == null)
        {
            Debug.LogError("[DictionaryManager] GameManager or its sub-managers are missing.");
            return;
        }

        InitializeDictionary(GM.DataManager.FruitDatas);

        // Subscribe to fruit collection events
        GM.ScoreUpdater.OnFruitCollected += UpdateDictionaryUI;
    }

    private void OnDestroy()
    {
        if (GM != null && GM.ScoreUpdater != null)
        {
            GM.ScoreUpdater.OnFruitCollected -= UpdateDictionaryUI;
        }
    }

    /// <summary>
    /// Initializes the dictionary UI with fruit data.
    /// </summary>
    public void InitializeDictionary(Dictionary<FruitsID, FruitsData> fruitData)
    {
        if (fruitData == null || fruitData.Count == 0) return;

        ClearExistingUI();

        foreach (var (id, data) in fruitData)
        {
            CreateFruitDictionaryItem(id, data);
        }

        UpdateAllDictionaryUI();
    }

    private void ClearExistingUI()
    {
        foreach (var item in _fruitDictionaryItems.Values)
        {
            if (item != null) Destroy(item.gameObject);
        }
        _fruitDictionaryItems.Clear();

        foreach (Transform child in dictionaryContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateFruitDictionaryItem(FruitsID id, FruitsData data)
    {
        if (fruitDictionaryPrefab == null)
        {
            Debug.LogError("[DictionaryManager] fruitDictionaryPrefab is not assigned.");
            return;
        }

        var itemObj = Instantiate(fruitDictionaryPrefab, dictionaryContent);
        if (!itemObj.TryGetComponent<FruitDictionaryItem>(out var fruitItem))
        {
            Debug.LogError($"[DictionaryManager] {id} item is missing FruitDictionaryItem component!");
            Destroy(itemObj);
            return;
        }

        fruitItem.Setup(id, data.Image);
        _fruitDictionaryItems[id] = fruitItem;
    }

    /// <summary>
    /// Updates a specific fruit UI entry.
    /// </summary>
    public void UpdateDictionaryUI(FruitsID fruitID)
    {
        if (!_fruitDictionaryItems.TryGetValue(fruitID, out var item)) return;

        bool isCollected = GM.PlayerDataManager.NowPlayerData.DictionaryCollection.TryGetValue(fruitID, out bool collected) && collected;
        item.UpdateFruitUI(isCollected);
    }

    /// <summary>
    /// Updates all fruit UI entries in the dictionary.
    /// </summary>
    public void UpdateAllDictionaryUI()
    {
        foreach (var (id, item) in _fruitDictionaryItems)
        {
            bool isCollected = GM.PlayerDataManager.NowPlayerData.DictionaryCollection.TryGetValue(id, out bool collected) && collected;
            item.UpdateFruitUI(isCollected);
        }
    }
}
