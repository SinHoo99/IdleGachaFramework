using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatusUI : MonoBehaviour
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
        Boss.OnBossDefeated += UpdateCoinUI; // 보스 처치 이벤트 구독
    }

    private void OnDisable()
    {
        Boss.OnBossDefeated -= UpdateCoinUI; // 이벤트 해제
    }


    public void PlayerCoin()
    {
        CoinText.text = $"{GM.PlayerDataManager.NowPlayerData.PlayerCoin}";
    }

    public void BossStatus()
    {
        BossText.text = $"보스zzzzz 단계 {GM.BossDataManager.BossRuntimeData.CurrentBossID}";
    }
    private void UpdateCoinUI(int reward)
    {
        CoinText.text = $"{GM.PlayerDataManager.NowPlayerData.PlayerCoin}";
    }
}
