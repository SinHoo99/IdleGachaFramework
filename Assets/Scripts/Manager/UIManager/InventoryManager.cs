using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private GameManager GM => GameManager.Instance;
    
    [SerializeField] private FruitUIManager _fruitUIManager;
    public FruitUIManager FruitUIManager => _fruitUIManager;

    public event Action OnInventoryUpdated;

    private void Start()
    {
        OnInventoryUpdated += UpdateInventoryUI;
        
        if (GM != null && GM.DataManager != null)
        {
            _fruitUIManager.SetFruitData(GM.DataManager.FruitDatas);
        }
    }

    private void OnDestroy()
    {
        OnInventoryUpdated -= UpdateInventoryUI;
    }

    public void TriggerInventoryUpdate()
    {
        OnInventoryUpdated?.Invoke();
        
        if (GM != null)
        {
            if (GM.PlayerStatusUI != null) GM.PlayerStatusUI.PlayerCoin();
            if (GM.UIManager?.DictionaryManager != null) GM.UIManager.DictionaryManager.UpdateAllDictionaryUI();
        }
    }

    private void UpdateInventoryUI()
    {
        if (GM == null || GM.PlayerDataManager?.NowPlayerData?.Inventory == null)
        {
            if (_fruitUIManager != null)
                _fruitUIManager.UpdateFruitCountsUI(new Dictionary<FruitsID, int>());
            return;
        }

        var inventory = GM.PlayerDataManager.NowPlayerData.Inventory;
        _fruitUIManager.UpdateFruitCountsUI(inventory.ToDictionary(kv => kv.Key, kv => kv.Value.Amount));
    }
}
