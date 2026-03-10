using UnityEngine;
using UnityEngine.Audio;

public class GameManager : Singleton<GameManager>
{
    #region Script Setup
    [Header("Managers")]
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private PlayerStatusUI _playerStatusUI;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private SoundManager _soundManager;
    [SerializeField] private PoolManager _poolManager;
    [SerializeField] private AlertManager _alertManager;
    [SerializeField] private ObjectPool _objectPool;
    [SerializeField] private PlayerDataManager _playerDataManager;
    [SerializeField] private BossDataManager _bossDataManager;
    [SerializeField] private DataManager _dataManager;
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private ScoreUpdater _scoreUpdater;

    [Header("Game Objects")]
    [SerializeField] private PoolObject _bulletPrefabs;
    #endregion

    private PrefabDataManager _prefabDataManager;

    #region Public Properties
    public UIManager UIManager => _uiManager;
    public PlayerStatusUI PlayerStatusUI => _playerStatusUI;
    public SpawnManager SpawnManager => _spawnManager;
    public PlayerDataManager PlayerDataManager => _playerDataManager;
    public ObjectPool ObjectPool => _objectPool;
    public DataManager DataManager => _dataManager;
    public SaveManager SaveManager => _saveManager;
    public PoolManager PoolManager => _poolManager;
    public BossDataManager BossDataManager => _bossDataManager;
    public SoundManager SoundManager => _soundManager;
    public AlertManager AlertManager => _alertManager;
    public ScoreUpdater ScoreUpdater => _scoreUpdater;
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
        // Find components if they are not assigned in the Inspector
        _dataManager ??= GetComponentInChildren<DataManager>();
        _saveManager ??= GetComponentInChildren<SaveManager>();
        _playerDataManager ??= GetComponentInChildren<PlayerDataManager>();
        _bossDataManager ??= GetComponentInChildren<BossDataManager>();
        _soundManager ??= GetComponentInChildren<SoundManager>();
        _objectPool ??= GetComponentInChildren<ObjectPool>();
        _poolManager ??= GetComponentInChildren<PoolManager>();
        _uiManager ??= GetComponentInChildren<UIManager>();
        _alertManager ??= GetComponentInChildren<AlertManager>();
        _spawnManager ??= GetComponentInChildren<SpawnManager>();

        // Core Initialization
        if (_dataManager != null) _dataManager.Initialize();
        if (_soundManager != null) _soundManager.Initialize();
    }

    private void InitializeGame()
    {
        if (_poolManager != null) _poolManager.AddObjectPool();
        if (_playerDataManager != null) _playerDataManager.Initialize();
        
        _prefabDataManager = new PrefabDataManager();
        
        if (_uiManager?.InventoryManager != null)
            _uiManager.InventoryManager.TriggerInventoryUpdate();
            
        if (_soundManager?.SettingPopup != null)
            _soundManager.SettingPopup.Initializer();
    }
    #endregion

    #region Application Events
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        if (_soundManager?.SettingPopup != null)
        {
            _soundManager.SettingPopup.gameObject.SetActive(false);
        }

        SaveAllData();
    }

    private void OnApplicationPause(bool pause)
    {
        // Save on pause only if not already quitting
        if (pause)
        {
            SaveAllData();
        }
    }

    public void SaveAllData()
    {
        if (_playerDataManager != null) _playerDataManager.SavePlayerData();
        if (_prefabDataManager != null) _prefabDataManager.SavePrefabData();
        if (_bossDataManager != null) _bossDataManager.SaveBossRuntimeData();
        if (_soundManager != null) _soundManager.SaveOptionData();
    }
    #endregion

    #region Data Accessors
    private static readonly CollectedFruitData EmptyCollectedFruitData = new CollectedFruitData { ID = FruitsID.None, Amount = 0 };

    public FruitsData GetFruitsData(FruitsID id)
    {
        if (_dataManager == null || _dataManager.FruitDatas == null) return null;
        return _dataManager.FruitDatas.TryGetValue(id, out var data) ? data : null;
    }

    public int GetFruitAmount(FruitsID id)
    {
        if (_playerDataManager?.NowPlayerData?.Inventory == null) return 0;
        return _playerDataManager.NowPlayerData.Inventory.TryGetValue(id, out var collectedData) ? collectedData.Amount : 0;
    }

    public CollectedFruitData GetCollectedFruitData(FruitsID id)
    {
        if (_playerDataManager?.NowPlayerData?.Inventory == null) return EmptyCollectedFruitData;
        return _playerDataManager.NowPlayerData.Inventory.TryGetValue(id, out var collectedData) ? collectedData : EmptyCollectedFruitData;
    }

    public BossData GetBossData(BossID id)
    {
        if (_dataManager == null || _dataManager.BossDatas == null) return null;
        return _dataManager.BossDatas.TryGetValue(id, out BossData bossData) ? bossData : null;
    }

    public int GetBossReward(BossID id)
    {
        if (_dataManager == null || _dataManager.BossDatas == null) return 0;
        return _dataManager.BossDatas.TryGetValue(id, out BossData bossData) ? bossData.Reward : 0;
    }

    public PoolObject GetBullet()
    {
        return _bulletPrefabs;
    }
    #endregion

    #region Sound Methods
    public AudioMixer GetAudioMixer()
    {
        return _soundManager != null ? _soundManager.AudioMixer : null;
    }

    public void PlayBGM(BGM target)
    {
        if (_soundManager != null) _soundManager.PlayBGM(target);
    }

    public void PlaySFX(SFX target)
    {
        if (_soundManager != null) _soundManager.PlaySFX(target);
    }
    #endregion
}
