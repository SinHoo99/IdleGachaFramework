using UnityEngine;

public class BossDataManager : Singleton<BossDataManager>
{
    private GameManager GM => GameManager.Instance;

    public BossData StaticBossData { get; private set; }
    public BossRuntimeData BossRuntimeData { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        LoadAllData();
    }

    #region Save/Load Data
    public bool LoadBossData(BossID bossID)
    {
        if (GM == null) return false;

        BossData bossData = GM.GetBossData(bossID);
        if (bossData != null)
        {
            StaticBossData = bossData;
            return true;
        }

        Debug.LogWarning($"[BossDataManager] Boss data for {bossID} not found. Falling back to default (A).");
        var defaultBoss = GM.GetBossData(BossID.A);
        if (defaultBoss != null)
        {
            StaticBossData = new BossData(BossID.A, defaultBoss.MaxHealth, defaultBoss.AnimationState, defaultBoss.Reward);
        }
        return false;
    }

    public void SaveBossRuntimeData()
    {
        if (BossRuntimeData == null || GM?.SaveManager == null) return;
        GM.SaveManager.SaveData(BossRuntimeData);
    }

    public bool LoadBossRuntimeData()
    {
        if (GM?.SaveManager != null && GM.SaveManager.TryLoadData(out BossRuntimeData data))
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
        if (GM == null) return;

        var defaultBoss = GM.GetBossData(BossID.A);
        if (defaultBoss != null)
        {
            BossRuntimeData = new BossRuntimeData(BossID.A, defaultBoss.MaxHealth);
            StaticBossData = new BossData(BossID.A, defaultBoss.MaxHealth, defaultBoss.AnimationState, defaultBoss.Reward);
            GM.SaveManager.SaveData(BossRuntimeData);
        }

        if (GM.SpawnManager != null)
        {
            Boss boss = GM.SpawnManager.GetCurrentBoss();
            if (boss != null)
            {
                boss.ResetBossData();
            }
        }

        Debug.Log("[BossDataManager] Boss data reset completed.");
    }
    #endregion
}
