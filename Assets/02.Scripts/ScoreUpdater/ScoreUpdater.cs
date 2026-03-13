using System;
using System.Linq;
using UnityEngine;

public class ScoreUpdater : Singleton<ScoreUpdater>
{
    public event Action<string> OnUnitCollected;

    #region Unit Logic
    /// <summary>
    /// Adds a fruit to the player's collection.
    /// </summary>
    public void AddUnit(string UnitID)
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.NowPlayerData?.Inventory == null) return;
        
        var inventory = PlayerDataManager.Instance.NowPlayerData.Inventory;

        if (!inventory.ContainsKey(UnitID))
        {
            Debug.LogWarning($"{UnitID} not found in Inventory.");
            return;
        }

        // Mark as collected in dictionary
        PlayerDataManager.Instance.CollectUnit(UnitID);

        // Increase level (stored as Amount)
        inventory[UnitID].Amount++;

        // Save progress
        PlayerDataManager.Instance.SavePlayerData();

        // Update physical unit or play upgrade effect
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SpawnUnitFromPool(UnitID);

        // Update UI with new Level
        if (UnitUIManager.Instance != null)
            UnitUIManager.Instance.UpdateOrCreateUnitUI(UnitID, inventory[UnitID].Amount);

        // Notify systems
        EventBus.Publish(GameEventType.OnInventoryUpdate);
        EventBus.Publish(GameEventType.OnDictionaryUpdate);

        OnUnitCollected?.Invoke(UnitID);

        Debug.Log($"[ScoreUpdater] {UnitID} Leveled up. Current Level: {inventory[UnitID].Amount}");
    }

    /// <summary>
    /// Attempts to summon a unit. If already owned, it levels up.
    /// </summary>
    public void AddRandomUnit()
    {
        string selectedUnit = GetRandomUnitByProbability();

        if (!string.IsNullOrEmpty(selectedUnit))
        {
            AddUnit(selectedUnit); 
        }
    }

    private string GetRandomUnitByProbability()
    {
        if (DataManager.Instance == null) return string.Empty;
        
        var Units = DataManager.Instance.UnitDatas.Values.ToList();
        float totalProbability = Units.Sum(f => f.Probability);
        float randomValue = UnityEngine.Random.Range(0f, totalProbability); 

        float cumulativeProbability = 0f;

        foreach (var Unit in Units)
        {
            cumulativeProbability += Unit.Probability;
            if (randomValue <= cumulativeProbability)
            {
                if (AlertManager.Instance != null)
                    AlertManager.Instance.ShowAlert($"{Unit.Name} LEVEL UP!");
                return Unit.ID; 
            }
        }

        if (AlertManager.Instance != null)
            AlertManager.Instance.ShowAlert("Failed to summon unit.");
        return string.Empty; 
    }
    #endregion

    #region Input Handling
    private float _lastInputTime = 0f;
    private float _inputCooldown = 0.5f;

    public void HandleInput()
    {
        if (Time.time - _lastInputTime < _inputCooldown) return;
        _lastInputTime = Time.time;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SFX.Click);

        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.TrySpendCoin(100))
        {
            if (PlayerStatusUI.Instance != null)
                PlayerStatusUI.Instance.UpdateCoinUI();
                
            AddRandomUnit(); 
            
            // Notify inventory update through EventBus
            EventBus.Publish(GameEventType.OnInventoryUpdate);
        }
        else
        {
            if (AlertManager.Instance != null)
                AlertManager.Instance.ShowAlert("Not enough coins.");
        }
    }
    #endregion
}
