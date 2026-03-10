using System;
using System.Linq;
using UnityEngine;

public class ScoreUpdater : Singleton<ScoreUpdater>
{
    private GameManager GM => GameManager.Instance;

    public event Action<FruitsID> OnFruitCollected;

    #region Fruit Logic
    /// <summary>
    /// Adds a fruit to the player's collection.
    /// </summary>
    public void AddFruits(FruitsID fruitID)
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.NowPlayerData?.Inventory == null) return;
        
        var inventory = PlayerDataManager.Instance.NowPlayerData.Inventory;

        if (!inventory.ContainsKey(fruitID))
        {
            Debug.LogWarning($"{fruitID} not found in Inventory.");
            return;
        }

        // Collect fruit status
        PlayerDataManager.Instance.CollectFruit(fruitID);

        // Increment amount
        inventory[fruitID].Amount++;

        // Save data
        PlayerDataManager.Instance.SavePlayerData();

        // Update UI
        if (FruitUIManager.Instance != null)
            FruitUIManager.Instance.UpdateOrCreateFruitUI(fruitID, inventory[fruitID].Amount);

        // Invoke event
        OnFruitCollected?.Invoke(fruitID);

        Debug.Log($"[ScoreUpdater] {fruitID} added. Current amount: {inventory[fruitID].Amount}");
    }

    /// <summary>
    /// Adds a random fruit based on probability.
    /// </summary>
    public void AddRandomFruit()
    {
        FruitsID? selectedFruit = GetRandomFruitByProbability();

        if (selectedFruit.HasValue)
        {
            AddFruits(selectedFruit.Value); 
            if (SpawnManager.Instance != null)
                SpawnManager.Instance.SpawnFruitFromPool(selectedFruit.Value);
        }
    }

    private FruitsID? GetRandomFruitByProbability()
    {
        if (DataManager.Instance == null) return null;
        
        var fruits = DataManager.Instance.FruitDatas.Values.ToList();
        float totalProbability = fruits.Sum(f => f.Probability);
        float randomValue = UnityEngine.Random.Range(0f, totalProbability + 1.0f); 

        float cumulativeProbability = 0f;

        foreach (var fruit in fruits)
        {
            cumulativeProbability += fruit.Probability;
            if (randomValue <= cumulativeProbability)
            {
                if (AlertManager.Instance != null)
                    AlertManager.Instance.ShowAlert($"{fruit.ID} Collection Success");
                return fruit.ID; 
            }
        }

        if (AlertManager.Instance != null)
            AlertManager.Instance.ShowAlert("Failed to collect random fruit.");
        return null; 
    }
    #endregion

    #region Input Handling
    public void HandleInput()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SFX.Click);

        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.NowPlayerData.PlayerCoin >= 100)
        {
            if (PlayerStatusUI.Instance != null)
                PlayerStatusUI.Instance.PlayerCoin();
                
            AddRandomFruit(); 
            
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.TriggerInventoryUpdate();
        }
        else
        {
            if (AlertManager.Instance != null)
                AlertManager.Instance.ShowAlert("Not enough coins.");
        }
    }
    #endregion
}
