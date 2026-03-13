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
        Boss.OnBossDefeated += HandleBossDefeated;
        EventBus.Subscribe(GameEventType.OnInventoryUpdate, UpdateCoinUI);
        EventBus.Subscribe(GameEventType.OnDataReset, UpdateCoinUI);
    }

    private void OnDisable()
    {
        Boss.OnBossDefeated -= HandleBossDefeated;
        EventBus.Unsubscribe(GameEventType.OnInventoryUpdate, UpdateCoinUI);
        EventBus.Unsubscribe(GameEventType.OnDataReset, UpdateCoinUI);
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
        if (BossDataManager.Instance != null && BossDataManager.Instance.BossRuntimeData != null)
        {
            if (BossText != null)
                BossText.text = $"Boss Stage: {BossDataManager.Instance.BossRuntimeData.CurrentBossID}";
        }
    }

    private void HandleBossDefeated(int reward)
    {
        UpdateCoinUI();
    }
}
