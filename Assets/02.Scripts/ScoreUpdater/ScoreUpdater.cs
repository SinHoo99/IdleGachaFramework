using System;
using System.Linq;
using UnityEngine;

public class ScoreUpdater : Singleton<ScoreUpdater>
{
    #region 유닛 로직
    /// <summary>
    /// 플레이어의 소지 목록에 유닛을 추가합니다.
    /// </summary>
    public void AddUnit(string UnitID)
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.NowPlayerData?.Inventory == null) return;
        
        var inventory = PlayerDataManager.Instance.NowPlayerData.Inventory;

        if (!inventory.ContainsKey(UnitID))
        {
            Debug.LogWarning($"인벤토리에서 {UnitID}를 찾을 수 없습니다.");
            return;
        }

        // 도감에 수집된 것으로 표시
        PlayerDataManager.Instance.CollectUnit(UnitID);

        // 레벨 증가 (Amount로 저장됨)
        inventory[UnitID].Amount++;

        // 진행 상황 저장
        PlayerDataManager.Instance.SavePlayerData();

        // 물리적 유닛 업데이트 (SpawnManager가 생성 또는 업그레이드 여부를 처리)
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SpawnUnitFromPool(UnitID);

        // 시스템 알림 - UI 컴포넌트들이 이 이벤트들을 구독해야 함
        EventBus.Publish(GameEventType.OnInventoryUpdate);
        EventBus.Publish(GameEventType.OnDictionaryUpdate);

        Debug.Log($"[ScoreUpdater] {UnitID} 레벨업. 현재 레벨: {inventory[UnitID].Amount}");
    }

    /// <summary>
    /// 유닛 소환을 시도합니다. 이미 보유 중인 경우 레벨이 올라갑니다.
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
                    AlertManager.Instance.ShowAlert($"{Unit.Name} 레벨 업!");
                return Unit.ID; 
            }
        }

        if (AlertManager.Instance != null)
            AlertManager.Instance.ShowAlert("유닛 소환에 실패했습니다.");
        return string.Empty; 
    }
    #endregion

    #region 입력 처리
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
            // if (PlayerStatusUI.Instance != null)
            //     PlayerStatusUI.Instance.UpdateCoinUI();
                
            AddRandomUnit(); 
            
            // EventBus를 통해 인벤토리 업데이트 알림
            EventBus.Publish(GameEventType.OnInventoryUpdate);
        }
        else
        {
            if (AlertManager.Instance != null)
                AlertManager.Instance.ShowAlert("코인이 부족합니다.");
        }
    }
    #endregion
}
