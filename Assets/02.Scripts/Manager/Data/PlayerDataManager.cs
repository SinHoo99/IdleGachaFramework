using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : Singleton<PlayerDataManager>
{
    private PrefabDataManager _prefabDataManager => PrefabDataManager.Instance;

    [Header("Initial Player Stats")]
    [SerializeField] private float _initDamage = 50f;
    [SerializeField] private float _initAttackRange = 7f; // 15에서 7로 하향 조정
    [SerializeField] private float _initMaxHP = 100f;
    [SerializeField] private float _initAttackSpeed = 1.0f;

    public PlayerData NowPlayerData { get; private set; }

    public void Initialize()
    {
        // LoadAllData(); // 로딩 주석 처리
        NowPlayerData = new PlayerData(); // 항상 새로운 데이터로 시작
        
        // 인스펙터 설정값 적용 (기본 스탯)
        NowPlayerData.Damage = _initDamage;
        NowPlayerData.AttackRange = _initAttackRange;
        NowPlayerData.MaxHP = _initMaxHP;
        NowPlayerData.AttackSpeed = _initAttackSpeed;

        InitializeInventory();
        InitializeInitialEquipment(); // 초기 장비 지급 추가
        RefreshPlayerStats(); // 최종 스탯 계산
    }

    private void InitializeInitialEquipment()
    {
        if (NowPlayerData.EquippedItems == null || NowPlayerData.EquippedItems.Count == 0)
        {
            NowPlayerData.EquippedItems = new Dictionary<EquipmentType, EquipmentInstance>();
            foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
            {
                NowPlayerData.EquippedItems.Add(type, new EquipmentInstance(type));
            }
            Debug.Log("[PlayerDataManager] 초기 장비 4종이 지급되었습니다.");
        }
    }

    public void RefreshPlayerStats()
    {
        if (NowPlayerData == null) return;

        // 1. 기본 스탯 리셋 (초기 인스펙터 설정값 + 레벨업 보너스)
        float baseDmg = _initDamage + (NowPlayerData.Level - 1) * 5f;
        float baseMaxHP = _initMaxHP + (NowPlayerData.Level - 1) * 10f;
        float baseRange = _initAttackRange + (NowPlayerData.Level - 1) * 0.05f;
        float baseAS = _initAttackSpeed;

        // 2. 장비 스탯 합산
        if (EquipmentDataManager.Instance != null && NowPlayerData.EquippedItems != null)
        {
            foreach (var item in NowPlayerData.EquippedItems.Values)
            {
                float stat = EquipmentDataManager.Instance.GetStat(item);
                switch (item.Type)
                {
                    case EquipmentType.Weapon: baseDmg += stat; break;
                    case EquipmentType.Armor: baseMaxHP += stat; break;
                    case EquipmentType.Glove: baseAS += (stat * 0.001f); break; // 공격 속도는 보정치 적용
                    case EquipmentType.Ring: baseRange += (stat * 0.01f); break; // 사거리 보정
                }
            }
        }

        // 3. 최종 스탯 적용
        NowPlayerData.Damage = baseDmg;
        NowPlayerData.MaxHP = baseMaxHP;
        NowPlayerData.AttackRange = baseRange;
        NowPlayerData.AttackSpeed = baseAS;

        OnStatChanged?.Invoke();
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
    public event Action OnStatChanged; // 능력치 변경 이벤트 추가

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
        OnStatChanged?.Invoke(); // 능력치 변경 알림
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
                GetComponent<HealthSystem>()?.Heal(data.value); 
                break;
            case CardEffectType.AttackRange:
                NowPlayerData.AttackRange += data.value;
                break;
            case CardEffectType.AttackSpeed:
                NowPlayerData.AttackSpeed += data.value; // 공격 속도 증가 (예: +0.1)
                break;
            case CardEffectType.MultiShot:
                NowPlayerData.MultiShotCount += (int)data.value; // 이제 내부 프로퍼티가 알아서 1~3으로 제한함
                break;
        }

        Debug.Log($"<color=cyan>[PlayerData] Applied Card: {data.cardName} (+{data.value})</color>");
        OnStatChanged?.Invoke(); // 능력치 변경 알림
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
    /// 플레이어에게 코인을 지급합니다.
    /// </summary>
    public void GainCoin(int amount)
    {
        if (NowPlayerData == null) return;

        NowPlayerData.PlayerCoin += amount;
        
        // 코인이 변경되었음을 시스템에 알림
        EventBus.Publish(GameEventType.OnInventoryUpdate);
        
        Debug.Log($"[PlayerDataManager] {amount} 코인 획득. 현재 코인: {NowPlayerData.PlayerCoin}");
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

        NowPlayerData.PlayerCoin -= amount;
        
        // 코인이 변경되었음을 시스템에 알림
        EventBus.Publish(GameEventType.OnInventoryUpdate);
        
        return true;
    }
    #endregion
}
