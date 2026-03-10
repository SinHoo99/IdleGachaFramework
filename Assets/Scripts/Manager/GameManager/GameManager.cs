using UnityEngine;
using UnityEngine.Audio;

public class GameManager : Singleton<GameManager>
{
    #region Script Setup
    [Header("Game Objects")]
    [SerializeField] private PoolObject _bulletPrefabs;
    #endregion

    #region Public Properties (Redirection to Singletons for compatibility)
    public UIManager UIManager => UIManager.Instance;
    public PlayerStatusUI PlayerStatusUI => PlayerStatusUI.Instance;
    public SpawnManager SpawnManager => SpawnManager.Instance;
    public PlayerDataManager PlayerDataManager => PlayerDataManager.Instance;
    public ObjectPool ObjectPool => ObjectPool.Instance;
    public DataManager DataManager => DataManager.Instance;
    public SaveManager SaveManager => SaveManager.Instance;
    public PoolManager PoolManager => PoolManager.Instance;
    public BossDataManager BossDataManager => BossDataManager.Instance;
    public SoundManager SoundManager => SoundManager.Instance;
    public AlertManager AlertManager => AlertManager.Instance;
    public ScoreUpdater ScoreUpdater => ScoreUpdater.Instance;
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
        // Core Initialization in specific order
        if (DataManager.Instance != null) DataManager.Instance.Initialize();
        if (SoundManager.Instance != null) SoundManager.Instance.Initialize();
        if (SoundManager.Instance != null && SoundManager.Instance.LoadOptionData())
        {
            // Apply loaded options if needed
        }
    }

    private void InitializeGame()
    {
        if (PoolManager.Instance != null) PoolManager.Instance.AddObjectPool();
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.Initialize();
        
        if (UIManager.Instance?.InventoryManager != null)
            UIManager.Instance.InventoryManager.TriggerInventoryUpdate();
            
        if (SoundManager.Instance?.SettingPopup != null)
            SoundManager.Instance.SettingPopup.Initializer();
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
    private static readonly CollectedFruitData EmptyCollectedFruitData = new CollectedFruitData { ID = FruitsID.None, Amount = 0 };

    public FruitsData GetFruitsData(FruitsID id)
    {
        if (DataManager.Instance == null || DataManager.Instance.FruitDatas == null) return null;
        return DataManager.Instance.FruitDatas.TryGetValue(id, out var data) ? data : null;
    }

    public int GetFruitAmount(FruitsID id)
    {
        if (PlayerDataManager.Instance?.NowPlayerData?.Inventory == null) return 0;
        return PlayerDataManager.Instance.NowPlayerData.Inventory.TryGetValue(id, out var collectedData) ? collectedData.Amount : 0;
    }

    public CollectedFruitData GetCollectedFruitData(FruitsID id)
    {
        if (PlayerDataManager.Instance?.NowPlayerData?.Inventory == null) return EmptyCollectedFruitData;
        return PlayerDataManager.Instance.NowPlayerData.Inventory.TryGetValue(id, out var collectedData) ? collectedData : EmptyCollectedFruitData;
    }

    public BossData GetBossData(BossID id)
    {
        if (DataManager.Instance == null || DataManager.Instance.BossDatas == null) return null;
        return DataManager.Instance.BossDatas.TryGetValue(id, out BossData bossData) ? bossData : null;
    }

    public int GetBossReward(BossID id)
    {
        if (DataManager.Instance == null || DataManager.Instance.BossDatas == null) return 0;
        return DataManager.Instance.BossDatas.TryGetValue(id, out BossData bossData) ? bossData.Reward : 0;
    }

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
