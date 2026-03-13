using UnityEngine;

public class BossDataManager : Singleton<BossDataManager>
{
    public BossData StaticBossData { get; private set; }
    public BossRuntimeData BossRuntimeData { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        LoadAllData();
    }

    private void OnEnable()
    {
        EventBus.Subscribe(GameEventType.OnDataReset, DestroyData);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(GameEventType.OnDataReset, DestroyData);
    }

    #region Save/Load Data
    public bool LoadBossData(BossID bossID)
    {
        BossData bossData = DataManager.Instance.GetBossData(bossID);
        if (bossData != null)
        {
            StaticBossData = bossData;
            return true;
        }

        Debug.LogWarning($"[BossDataManager] Boss data for {bossID} not found. Falling back to default (A).");
        var defaultBoss = DataManager.Instance.GetBossData(BossID.A);
        if (defaultBoss != null)
        {
            StaticBossData = new BossData(BossID.A, defaultBoss.MaxHealth, defaultBoss.AnimationState, defaultBoss.Reward);
        }
        return false;
    }

    public void SaveBossRuntimeData()
    {
        if (BossRuntimeData == null || SaveManager.Instance == null) return;
        SaveManager.Instance.SaveData(BossRuntimeData);
    }

    public bool LoadBossRuntimeData()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.TryLoadData(out BossRuntimeData data))
        {
            BossRuntimeData = data;
            return true;
        }

        Debug.LogWarning("[BossDataManager] Boss runtime data not found. Initializing with defaults.");
        BossRuntimeData = new BossRuntimeData(BossID.A, 100f);
        return false;
    }

    public bool LoadAllData()
    {
        bool runtimeLoaded = LoadBossRuntimeData();
        
        // Ensure StaticBossData is also loaded based on current runtime ID
        bool bossDataLoaded = LoadBossData(BossRuntimeData?.CurrentBossID ?? BossID.A);
        
        return runtimeLoaded && bossDataLoaded;
    }
    #endregion

    #region Data Modification
    public void DestroyData()
    {
        var defaultBoss = DataManager.Instance.GetBossData(BossID.A);
        if (defaultBoss != null)
        {
            BossRuntimeData = new BossRuntimeData(BossID.A, defaultBoss.MaxHealth);
            StaticBossData = new BossData(BossID.A, defaultBoss.MaxHealth, defaultBoss.AnimationState, defaultBoss.Reward);
            SaveManager.Instance.SaveData(BossRuntimeData);
        }

        Debug.Log("[BossDataManager] Boss data reset completed.");
    }
    #endregion
}
