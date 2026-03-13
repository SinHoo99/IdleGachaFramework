using UnityEngine;
using UnityEngine.Audio;

public class GameManager : Singleton<GameManager>
{
    #region Script Setup
    [Header("Game Objects")]
    [SerializeField] private PoolObject _bulletPrefabs;
    #endregion

    protected override void Awake()
    {
        if (IsDuplicates()) return;
        base.Awake();
        
        Application.targetFrameRate = 60;
        InitializeComponents();
    }

    private void Start()
    {
        InitializeGame();
    }

    #region Initialization Logic
    private void InitializeComponents()
    {
        // 1. DataManager (Loads CSVs)
        if (DataManager.Instance != null) DataManager.Instance.Initialize();
        
        // 2. SoundManager (Loads Settings & Applies Volumes)
        if (SoundManager.Instance != null) 
        {
            SoundManager.Instance.Initialize();
            SoundManager.Instance.LoadOptionData();
        }
    }

    private void InitializeGame()
    {
        // 3. Object Pool (Uses DataManager's cached data)
        if (PoolManager.Instance != null) PoolManager.Instance.AddObjectPool();
        
        // 4. Player Data (Loads Save Files)
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.Initialize();
        
        // 5. Visual State (Spawns items based on Loaded Player Data)
        if (SpawnManager.Instance != null) SpawnManager.Instance.SpawnInitialUnits();
        
        // 6. UI Synchronization
        if (UIManager.Instance?.InventoryManager != null)
            UIManager.Instance.InventoryManager.TriggerInventoryUpdate();
            
        if (SoundManager.Instance?.SettingPopup != null)
            SoundManager.Instance.SettingPopup.Initializer();
            
        Debug.Log("[GameManager] Full Initialization Complete in defined sequence.");
    }
    #endregion

    #region Application Events
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        if (SoundManager.Instance?.SettingPopup != null)
        {
            SoundManager.Instance.SettingPopup.gameObject.SetActive(false);
        }

        SaveAllData();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveAllData();
        }
    }

    public void SaveAllData()
    {
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.SavePlayerData();
        if (PrefabDataManager.Instance != null) PrefabDataManager.Instance.SavePrefabData();
        if (BossDataManager.Instance != null) BossDataManager.Instance.SaveBossRuntimeData();
        if (SoundManager.Instance != null) SoundManager.Instance.SaveOptionData();
    }
    #endregion

    #region Data Accessors
    public PoolObject GetBullet()
    {
        return _bulletPrefabs;
    }
    #endregion

    #region Sound Methods
    public AudioMixer GetAudioMixer()
    {
        return SoundManager.Instance != null ? SoundManager.Instance.AudioMixer : null;
    }

    public void PlayBGM(BGM target)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBGM(target);
    }

    public void PlaySFX(SFX target)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(target);
    }
    #endregion
}
