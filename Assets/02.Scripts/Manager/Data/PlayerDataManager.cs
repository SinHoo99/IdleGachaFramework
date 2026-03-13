using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : Singleton<PlayerDataManager>
{
    private PrefabDataManager _prefabDataManager => PrefabDataManager.Instance;

    public PlayerData NowPlayerData { get; private set; }

    public void Initialize()
    {
        LoadAllData();
        InitializeInventory();
    }

    #region Inventory Initialization
    public void InitializeInventory()
    {
        if (NowPlayerData == null)
        {
            NowPlayerData = new PlayerData();
        }

        NowPlayerData.Inventory ??= new Dictionary<string, CollectedUnitData>();
        NowPlayerData.DictionaryCollection ??= new Dictionary<string, bool>();

        if (DataManager.Instance != null)
        {
            foreach (var id in DataManager.Instance.UnitDatas.Keys)
            {
                if (!NowPlayerData.Inventory.ContainsKey(id))
                {
                    NowPlayerData.Inventory.Add(id, new CollectedUnitData { ID = id, Amount = 0 });
                }

                if (!NowPlayerData.DictionaryCollection.ContainsKey(id))
                {
                    NowPlayerData.DictionaryCollection.Add(id, false);
                }
            }
        }

        if (NowPlayerData.LastCollectedTime == default)
        {
            NowPlayerData.LastCollectedTime = DateTime.Now;
        }
    }
    #endregion

    #region Save/Load Data
    public void SavePlayerData()
    {
        if (NowPlayerData == null) return;

        NowPlayerData.LastCollectedTime = DateTime.Now;
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveData(NowPlayerData);
        }
    }

    public bool LoadPlayerData()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.TryLoadData(out PlayerData data))
        {
            NowPlayerData = data;
            
            if (UIManager.Instance?.InventoryManager != null)
                UIManager.Instance.InventoryManager.TriggerInventoryUpdate();
                
            return true;
        }

        NowPlayerData = new PlayerData();
        return false;
    }

    public bool LoadAllData()
    {
        bool playerDataLoaded = LoadPlayerData();
        _prefabDataManager?.LoadPrefabData();
        return playerDataLoaded;
    }
    #endregion

    /// <summary>
    /// Resets all fruit collection status in the dictionary to false.
    /// </summary>
    public void ResetDictionaryData()
    {
        if (NowPlayerData == null || NowPlayerData.DictionaryCollection == null) return;

        // Set all collected status to false
        var keys = new List<string>(NowPlayerData.DictionaryCollection.Keys);
        foreach (var id in keys)
        {
            NowPlayerData.DictionaryCollection[id] = false;
        }

        SavePlayerData();

        // Notify systems that dictionary data has changed
        EventBus.Publish(GameEventType.OnDictionaryUpdate);

        Debug.Log("[PlayerDataManager] Dictionary collection has been reset.");
    }

    #region Data Modification
    public void DestroyData()
    {
        if (NowPlayerData == null) return;

        // Force reset all inventory amounts to 0
        if (NowPlayerData.Inventory != null)
        {
            foreach (var item in NowPlayerData.Inventory.Values)
            {
                item.Amount = 0;
            }
        }
        
        if (NowPlayerData.DictionaryCollection != null)
        {
            NowPlayerData.DictionaryCollection.Clear();
        }
        
        NowPlayerData.PlayerCoin = 1000;
        
        InitializeInventory();

        // Save the reset data to file immediately
        SavePlayerData();
        
        // Notify all systems that data has been completely reset
        EventBus.Publish(GameEventType.OnDataReset);
        EventBus.Publish(GameEventType.OnInventoryUpdate);
        
        Debug.Log("[PlayerDataManager] All player data has been DESTROYED and saved. All amounts set to 0.");
    }

    /// <summary>
    /// Adds a fruit to the player's collection dictionary.
    /// </summary>
    public void CollectUnit(string unitID)
    {
        if (NowPlayerData == null || string.IsNullOrEmpty(unitID)) return;

        if (!NowPlayerData.DictionaryCollection.TryGetValue(unitID, out bool isCollected) || !isCollected)
        {
            NowPlayerData.DictionaryCollection[unitID] = true;
            Debug.Log($"[PlayerDataManager] New fruit collected: {unitID}");
            SavePlayerData();
        }
    }

    /// <summary>
    /// Attempts to sell a unit from inventory.
    /// Returns true if successful.
    /// </summary>
    public bool TrySellUnit(string id, int amount)
    {
        if (DataManager.Instance == null || NowPlayerData == null) return false;

        var unitData = DataManager.Instance.GetUnitData(id);
        if (unitData == null)
        {
            Debug.LogWarning($"[PlayerDataManager] Unit data for {id} not found.");
            return false;
        }

        if (!NowPlayerData.Inventory.TryGetValue(id, out var collectedUnit) || collectedUnit.Amount < amount)
        {
            Debug.LogWarning($"[PlayerDataManager] Not enough {id} to sell.");
            return false;
        }

        collectedUnit.Amount -= amount;
        NowPlayerData.PlayerCoin += unitData.Price;
        
        // Notify systems that inventory or coins changed
        EventBus.Publish(GameEventType.OnInventoryUpdate);
        
        return true;
    }

    /// <summary>
    /// Attempts to spend a certain amount of coins.
    /// Returns true if successful.
    /// </summary>
    public bool TrySpendCoin(int amount)
    {
        if (NowPlayerData == null) return false;

        if (NowPlayerData.PlayerCoin < amount)
        {
            Debug.LogWarning($"[PlayerDataManager] Not enough coins. Required: {amount}, Current: {NowPlayerData.PlayerCoin}");
            return false;
        }

        NowPlayerData.PlayerCoin -= amount;
        
        // Notify systems that coins changed
        EventBus.Publish(GameEventType.OnInventoryUpdate);
        
        return true;
    }
    #endregion
}
