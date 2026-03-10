using System.Collections.Generic;
using UnityEngine;

public class DictionaryManager : Singleton<DictionaryManager>
{
    private GameManager GM => GameManager.Instance;

    [SerializeField] private GameObject fruitDictionaryPrefab; 
    [SerializeField] private Transform dictionaryContent; 

    private readonly Dictionary<FruitsID, FruitDictionaryItem> _fruitDictionaryItems = new();

    private void Start()
    {
        if (DataManager.Instance == null || ScoreUpdater.Instance == null)
        {
            Debug.LogError("[DictionaryManager] DataManager or ScoreUpdater is missing.");
            return;
        }

        InitializeDictionary(DataManager.Instance.FruitDatas);

        // Subscribe to fruit collection events
        ScoreUpdater.Instance.OnFruitCollected += UpdateDictionaryUI;
    }

    private void OnDestroy()
    {
        if (ScoreUpdater.Instance != null)
        {
            ScoreUpdater.Instance.OnFruitCollected -= UpdateDictionaryUI;
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

        bool isCollected = PlayerDataManager.Instance.NowPlayerData.DictionaryCollection.TryGetValue(fruitID, out bool collected) && collected;
        item.UpdateFruitUI(isCollected);
    }

    /// <summary>
    /// Updates all fruit UI entries in the dictionary.
    /// </summary>
    public void UpdateAllDictionaryUI()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.NowPlayerData == null) return;

        foreach (var (id, item) in _fruitDictionaryItems)
        {
            bool isCollected = PlayerDataManager.Instance.NowPlayerData.DictionaryCollection.TryGetValue(id, out bool collected) && collected;
            item.UpdateFruitUI(isCollected);
        }
    }
}
