using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    private GameManager GM => GameManager.Instance;
    
    [SerializeField] private UnitUIManager _unitUIManager;
    public UnitUIManager UnitUIManager => _unitUIManager != null ? _unitUIManager : UnitUIManager.Instance;

    public event Action OnInventoryUpdated;

    private void Start()
    {
        OnInventoryUpdated += UpdateInventoryUI;
        
        if (DataManager.Instance != null && UnitUIManager != null)
        {
            UnitUIManager.SetUnitData(DataManager.Instance.UnitDatas);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe(GameEventType.OnInventoryUpdate, TriggerInventoryUpdate);
        EventBus.Subscribe(GameEventType.OnDataReset, TriggerInventoryUpdate);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(GameEventType.OnInventoryUpdate, TriggerInventoryUpdate);
        EventBus.Unsubscribe(GameEventType.OnDataReset, TriggerInventoryUpdate);
    }

    private void OnDestroy()
    {
        OnInventoryUpdated -= UpdateInventoryUI;
    }

    public void TriggerInventoryUpdate()
    {
        OnInventoryUpdated?.Invoke();
    }

    private void UpdateInventoryUI()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.NowPlayerData?.Inventory == null)
        {
            if (UnitUIManager != null)
                UnitUIManager.UpdateUnitCountsUI(new Dictionary<string, int>());
            return;
        }

        if (UnitUIManager == null)
        {
            Debug.LogWarning("[InventoryManager] FruitUIManager is missing.");
            return;
        }

        var inventory = PlayerDataManager.Instance.NowPlayerData.Inventory;
        
        // ToDictionary를 수행하기 전에 값이 null이 아닌지 확인
        var filteredInventory = inventory
            .Where(kv => kv.Value != null)
            .ToDictionary(kv => kv.Key, kv => kv.Value.Amount);

        UnitUIManager.UpdateUnitCountsUI(filteredInventory);
    }
}
