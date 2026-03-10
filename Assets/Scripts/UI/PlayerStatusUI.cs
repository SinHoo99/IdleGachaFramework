using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatusUI : Singleton<PlayerStatusUI>
{
    private GameManager GM => GameManager.Instance;
    public TextMeshProUGUI CoinText;
    public TextMeshProUGUI BossText;

    private void Start()
    {
        PlayerCoin();
        BossStatus();
    }
    private void OnEnable()
    {
        Boss.OnBossDefeated += UpdateCoinUI; 
    }

    private void OnDisable()
    {
        Boss.OnBossDefeated -= UpdateCoinUI; 
    }


    public void PlayerCoin()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.NowPlayerData != null)
        {
            CoinText.text = $"{PlayerDataManager.Instance.NowPlayerData.PlayerCoin}";
        }
    }

    public void BossStatus()
    {
        if (BossDataManager.Instance != null && BossDataManager.Instance.BossRuntimeData != null)
        {
            BossText.text = $" ܰ {BossDataManager.Instance.BossRuntimeData.CurrentBossID}";
        }
    }
    private void UpdateCoinUI(int reward)
    {
        PlayerCoin();
    }
}
