using System.Collections.Generic;
using UnityEngine;

public class DictionaryManager : Singleton<DictionaryManager>
{
    private GameManager GM => GameManager.Instance;

    [SerializeField] private GameObject UnitDictionaryPrefab; 
    [SerializeField] private Transform dictionaryContent; 

    private readonly Dictionary<string, UnitDictionaryItem> _UnitDictionaryItems = new();

    private void Start()
    {
        if (DataManager.Instance == null || ScoreUpdater.Instance == null)
        {
            Debug.LogError("[DictionaryManager] DataManager or ScoreUpdater is missing.");
            return;
        }

        InitializeDictionary(DataManager.Instance.UnitDatas);

        // Subscribe to fruit collection events
        ScoreUpdater.Instance.OnUnitCollected += UpdateDictionaryUI;
    }

    private void OnEnable()
    {
        EventBus.Subscribe(GameEventType.OnDictionaryUpdate, UpdateAllDictionaryUI);
        EventBus.Subscribe(GameEventType.OnDataReset, UpdateAllDictionaryUI);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(GameEventType.OnDictionaryUpdate, UpdateAllDictionaryUI);
        EventBus.Unsubscribe(GameEventType.OnDataReset, UpdateAllDictionaryUI);
    }

    private void OnDestroy()
    {
        if (ScoreUpdater.Instance != null)
        {
            ScoreUpdater.Instance.OnUnitCollected -= UpdateDictionaryUI;
        }
    }

    /// <summary>
    /// Initializes the dictionary UI with fruit data.
    /// </summary>
    public void InitializeDictionary(IReadOnlyDictionary<string, UnitData> UnitData)
    {
        if (UnitData == null || UnitData.Count == 0) return;

        ClearExistingUI();

        foreach (var pair in UnitData)
        {
            CreateUnitDictionaryItem(pair.Key, pair.Value);
        }

        UpdateAllDictionaryUI();
    }

    private void ClearExistingUI()
    {
        foreach (var item in _UnitDictionaryItems.Values)
        {
            if (item != null) Destroy(item.gameObject);
        }
        _UnitDictionaryItems.Clear();

        if (dictionaryContent == null)
        {
            Debug.LogWarning("[DictionaryManager] dictionaryContent is not assigned in the inspector.");
            return;
        }

        foreach (Transform child in dictionaryContent)
        {
            if (child != null) Destroy(child.gameObject);
        }
    }

    private void CreateUnitDictionaryItem(string id, UnitData data)
    {
        if (UnitDictionaryPrefab == null || dictionaryContent == null)
        {
            Debug.LogError("[DictionaryManager] Prefab or Content Transform is missing.");
            return;
        }

        var itemObj = Instantiate(UnitDictionaryPrefab, dictionaryContent);
        if (!itemObj.TryGetComponent<UnitDictionaryItem>(out var UnitItem))
        {
            Debug.LogError($"[DictionaryManager] {id} item is missing FruitDictionaryItem component!");
            Destroy(itemObj);
            return;
        }

        UnitItem.Setup(id, data.Image);
        _UnitDictionaryItems[id] = UnitItem;
    }

    /// <summary>
    /// Updates a specific fruit UI entry.
    /// </summary>
    public void UpdateDictionaryUI(string UnitID)
    {
        if (!_UnitDictionaryItems.TryGetValue(UnitID, out var item)) return;
        if (PlayerDataManager.Instance?.NowPlayerData == null) return;

        bool isCollected = PlayerDataManager.Instance.NowPlayerData.DictionaryCollection.TryGetValue(UnitID, out bool collected) && collected;
        item.UpdateUnitUI(isCollected);
    }

    /// <summary>
    /// Updates all fruit UI entries in the dictionary.
    /// </summary>
    public void UpdateAllDictionaryUI()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.NowPlayerData == null) return;

        foreach (var pair in _UnitDictionaryItems)
        {
            var id = pair.Key;
            var item = pair.Value;
            if (item == null) continue;
            bool isCollected = PlayerDataManager.Instance.NowPlayerData.DictionaryCollection.TryGetValue(id, out bool collected) && collected;
            item.UpdateUnitUI(isCollected);
        }
    }
}
