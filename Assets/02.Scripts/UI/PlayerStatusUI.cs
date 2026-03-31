using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatusUI : Singleton<PlayerStatusUI>
{
    public TextMeshProUGUI CoinText;
    public TextMeshProUGUI BossText;

    private void Start()
    {
        UpdateCoinUI();
        BossStatus();
    }

    private void OnEnable()
    {
        EventBus.Subscribe(GameEventType.OnInventoryUpdate, UpdateCoinUI);
        EventBus.Subscribe(GameEventType.OnDataReset, UpdateCoinUI);
        EventBus.Subscribe(GameEventType.OnEnemyDefeated, UpdateCoinUI); // Update coin when enemy/boss is defeated
        EventBus.Subscribe(GameEventType.OnStageStart, BossStatus);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(GameEventType.OnInventoryUpdate, UpdateCoinUI);
        EventBus.Unsubscribe(GameEventType.OnDataReset, UpdateCoinUI);
        EventBus.Unsubscribe(GameEventType.OnEnemyDefeated, UpdateCoinUI);
        EventBus.Unsubscribe(GameEventType.OnStageStart, BossStatus);
    }

    public void UpdateCoinUI()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.NowPlayerData != null)
        {
            if (CoinText != null)
                CoinText.text = $"{PlayerDataManager.Instance.NowPlayerData.PlayerCoin}";
        }
    }

    public void BossStatus()
    {
        if (StageManager.Instance != null)
        {
            if (BossText != null)
            {
                var enemyData = DataManager.Instance.GetEnemyData(StageManager.Instance.CurrentStage);
                if (enemyData != null && enemyData.Type == EntityType.Boss)
                {
                    BossText.text = $"Boss: {enemyData.Name}";
                }
                else
                {
                    BossText.text = $"Stage: {StageManager.Instance.CurrentStage}";
                }
            }
        }
    }
}
