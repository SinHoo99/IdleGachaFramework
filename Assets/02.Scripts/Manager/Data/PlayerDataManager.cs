using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : Singleton<PlayerDataManager>
{
    private PrefabDataManager _prefabDataManager => PrefabDataManager.Instance;

    public PlayerData NowPlayerData { get; private set; }

    public void Initialize()
    {
        // LoadAllData(); // 로딩 주석 처리
        NowPlayerData = new PlayerData(); // 항상 새로운 데이터로 시작
        InitializeInventory();
    }

    #region 인벤토리 초기화
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

    #region 데이터 저장/로드
    public void SavePlayerData()
    {
        if (NowPlayerData == null) return;

        NowPlayerData.LastCollectedTime = DateTime.Now;
        /*
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveData(NowPlayerData);
        }
        */
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
    /// 도감의 모든 유닛 수집 상태를 false로 초기화합니다.
    /// </summary>
    public void ResetDictionaryData()
    {
        if (NowPlayerData == null || NowPlayerData.DictionaryCollection == null) return;

        // 모든 수집 상태를 false로 설정
        var keys = new List<string>(NowPlayerData.DictionaryCollection.Keys);
        foreach (var id in keys)
        {
            NowPlayerData.DictionaryCollection[id] = false;
        }

        SavePlayerData();

        // 도감 데이터가 변경되었음을 시스템에 알림
        EventBus.Publish(GameEventType.OnDictionaryUpdate);

        Debug.Log("[PlayerDataManager] 도감 수집 데이터가 초기화되었습니다.");
    }

    public event Action<float, float> OnExpChanged;
    public event Action<int> OnLevelChanged;

    #region 경험치 및 레벨업 로직
    public void GainExp(float amount)
    {
        if (NowPlayerData == null) return;

        NowPlayerData.CurrentExp += amount;
        Debug.Log($"[PlayerDataManager] {amount} EXP 획득. 현재: {NowPlayerData.CurrentExp}/{NowPlayerData.MaxExp}");

        while (NowPlayerData.CurrentExp >= NowPlayerData.MaxExp)
        {
            LevelUp();
        }

        OnExpChanged?.Invoke(NowPlayerData.CurrentExp, NowPlayerData.MaxExp);
    }

    private void LevelUp()
    {
        NowPlayerData.CurrentExp -= NowPlayerData.MaxExp;
        NowPlayerData.Level++;

        // 레벨업 공식 (간단한 선형 또는 곡선형 성장)
        NowPlayerData.MaxExp = Mathf.RoundToInt(100 * Mathf.Pow(1.2f, NowPlayerData.Level - 1));
        
        // 능력치 상승
        NowPlayerData.Damage += 5f;          // 레벨당 데미지 +5
        NowPlayerData.MaxHP += 10f;          // 레벨당 최대 체력 +10
        NowPlayerData.AttackRange += 0.05f;  // 레벨당 사거리 +0.05

        Debug.Log($"<color=yellow>[PlayerDataManager] LEVEL UP! Level: {NowPlayerData.Level}</color>");
        
        OnLevelChanged?.Invoke(NowPlayerData.Level);
        SavePlayerData();
    }
    #endregion

    public void ApplyCardEffect(CardData data)
    {
        if (NowPlayerData == null || data == null) return;

        switch (data.effectType)
        {
            case CardEffectType.Damage:
                NowPlayerData.Damage += data.value;
                break;
            case CardEffectType.MaxHP:
                NowPlayerData.MaxHP += data.value;
                // 현재 체력 회복도 같이 해주면 좋습니다 (옵션)
                GetComponent<HealthSystem>()?.Heal(data.value); 
                break;
            case CardEffectType.AttackRange:
                NowPlayerData.AttackRange += data.value;
                break;
            case CardEffectType.AttackSpeed:
                // 향후 추가될 공격 속도 로직 반영
                break;
        }

        Debug.Log($"<color=cyan>[PlayerData] Applied Card: {data.cardName} (+{data.value})</color>");
        SavePlayerData();
    }

    #region 데이터 수정
    public void DestroyData()
    {
        if (NowPlayerData == null) return;

        // 모든 인벤토리 수량을 강제로 0으로 초기화
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
        
        NowPlayerData.PlayerCoin = 100000;
        
        InitializeInventory();

        // 초기화된 데이터를 즉시 파일에 저장
        SavePlayerData();
        
        // 데이터가 완전히 초기화되었음을 모든 시스템에 알림
        EventBus.Publish(GameEventType.OnDataReset);
        EventBus.Publish(GameEventType.OnInventoryUpdate);
        
        Debug.Log("[PlayerDataManager] 모든 플레이어 데이터가 파괴되고 저장되었습니다. 모든 수량이 0으로 설정되었습니다.");
    }

    /// <summary>
    /// 플레이어의 도감 딕셔너리에 유닛을 추가합니다.
    /// </summary>
    public void CollectUnit(string unitID)
    {
        if (NowPlayerData == null || string.IsNullOrEmpty(unitID)) return;

        if (!NowPlayerData.DictionaryCollection.TryGetValue(unitID, out bool isCollected) || !isCollected)
        {
            NowPlayerData.DictionaryCollection[unitID] = true;
            Debug.Log($"[PlayerDataManager] 새로운 유닛 수집됨: {unitID}");
            SavePlayerData();
        }
    }

    /// <summary>
    /// 인벤토리에서 유닛 판매를 시도합니다.
    /// 성공하면 true를 반환합니다.
    /// </summary>
    public bool TrySellUnit(string id, int amount)
    {
        if (DataManager.Instance == null || NowPlayerData == null) return false;

        var unitData = DataManager.Instance.GetUnitData(id);
        if (unitData == null)
        {
            Debug.LogWarning($"[PlayerDataManager] {id}에 대한 유닛 데이터를 찾을 수 없습니다.");
            return false;
        }

        if (!NowPlayerData.Inventory.TryGetValue(id, out var collectedUnit) || collectedUnit.Amount < amount)
        {
            Debug.LogWarning($"[PlayerDataManager] 판매할 {id} 수량이 부족합니다.");
            return false;
        }

        collectedUnit.Amount -= amount;
        NowPlayerData.PlayerCoin += unitData.Price;
        
        // 인벤토리 또는 코인이 변경되었음을 시스템에 알림
        EventBus.Publish(GameEventType.OnInventoryUpdate);
        
        return true;
    }

    /// <summary>
    /// 특정 금액의 코인 소모를 시도합니다.
    /// 성공하면 true를 반환합니다.
    /// </summary>
    public bool TrySpendCoin(int amount)
    {
        if (NowPlayerData == null) return false;

        if (NowPlayerData.PlayerCoin < amount)
        {
            Debug.LogWarning($"[PlayerDataManager] 코인이 부족합니다. 필요량: {amount}, 현재량: {NowPlayerData.PlayerCoin}");
            return false;
        }

       // NowPlayerData.PlayerCoin -= amount;
        
        // 코인이 변경되었음을 시스템에 알림
        EventBus.Publish(GameEventType.OnInventoryUpdate);
        
        return true;
    }
    #endregion
}
