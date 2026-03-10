using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    private GameManager GM => GameManager.Instance;
    private PrefabDataManager _prefabDataManager;

    public PlayerData NowPlayerData { get; private set; }

    public void Initialize()
    {
        _prefabDataManager = new PrefabDataManager();
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

        NowPlayerData.Inventory ??= new Dictionary<FruitsID, CollectedFruitData>();
        NowPlayerData.DictionaryCollection ??= new Dictionary<FruitsID, bool>();

        foreach (FruitsID id in Enum.GetValues(typeof(FruitsID)))
        {
            if (id == FruitsID.None) continue;

            if (!NowPlayerData.Inventory.ContainsKey(id))
            {
                NowPlayerData.Inventory[id] = new CollectedFruitData { ID = id, Amount = 0 };
            }

            if (!NowPlayerData.DictionaryCollection.ContainsKey(id))
            {
                NowPlayerData.DictionaryCollection[id] = false;
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
        if (GM != null && GM.SaveManager != null)
        {
            GM.SaveManager.SaveData(NowPlayerData);
        }
    }

    public bool LoadPlayerData()
    {
        if (GM != null && GM.SaveManager != null && GM.SaveManager.TryLoadData(out PlayerData data))
        {
            NowPlayerData = data;
            
            if (GM.UIManager?.InventoryManager != null)
                GM.UIManager.InventoryManager.TriggerInventoryUpdate();
                
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

    #region Data Modification
    public void DestroyData()
    {
        if (NowPlayerData == null) return;

        NowPlayerData.Inventory.Clear();
        NowPlayerData.DictionaryCollection.Clear();
        NowPlayerData.PlayerCoin = 1000;
        
        if (GM != null)
        {
            if (GM.BossDataManager != null) GM.BossDataManager.DestroyData();
            InitializeInventory();
            
            if (GM.UIManager?.InventoryManager != null)
                GM.UIManager.InventoryManager.TriggerInventoryUpdate();
                
            if (GM.SpawnManager != null)
                GM.SpawnManager.ReturnAllFruitsToPool();
        }
    }

    /// <summary>
    /// Adds a fruit to the player's collection dictionary.
    /// </summary>
    public void CollectFruit(FruitsID fruitID)
    {
        if (NowPlayerData == null || fruitID == FruitsID.None) return;

        if (!NowPlayerData.DictionaryCollection.TryGetValue(fruitID, out bool isCollected) || !isCollected)
        {
            NowPlayerData.DictionaryCollection[fruitID] = true;
            Debug.Log($"[PlayerDataManager] New fruit collected: {fruitID}");
            SavePlayerData();
        }
    }
    #endregion
}
