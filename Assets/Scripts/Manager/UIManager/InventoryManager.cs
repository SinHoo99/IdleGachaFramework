using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    private GameManager GM => GameManager.Instance;
    
    [SerializeField] private FruitUIManager _fruitUIManager;
    public FruitUIManager FruitUIManager => _fruitUIManager;

    public event Action OnInventoryUpdated;

    private void Start()
    {
        OnInventoryUpdated += UpdateInventoryUI;
        
        if (DataManager.Instance != null)
        {
            _fruitUIManager.SetFruitData(DataManager.Instance.FruitDatas);
        }
    }

    private void OnDestroy()
    {
        OnInventoryUpdated -= UpdateInventoryUI;
    }

    public void TriggerInventoryUpdate()
    {
        OnInventoryUpdated?.Invoke();
        
        if (PlayerStatusUI.Instance != null) PlayerStatusUI.Instance.PlayerCoin();
        if (DictionaryManager.Instance != null) DictionaryManager.Instance.UpdateAllDictionaryUI();
    }

    private void UpdateInventoryUI()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.NowPlayerData?.Inventory == null)
        {
            if (_fruitUIManager != null)
                _fruitUIManager.UpdateFruitCountsUI(new Dictionary<FruitsID, int>());
            return;
        }

        var inventory = PlayerDataManager.Instance.NowPlayerData.Inventory;
        _fruitUIManager.UpdateFruitCountsUI(inventory.ToDictionary(kv => kv.Key, kv => kv.Value.Amount));
    }
}
