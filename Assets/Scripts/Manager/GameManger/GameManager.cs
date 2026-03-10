
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : Singleton<GameManager>
{
    #region  스크립트 참조
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

    private PrefabDataManager _prefabDataManager;
    public bool isQuitting = false;

    #endregion

    #region  Public Properties (읽기 전용)
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

    #region  게임 초기화 로직
    private void InitializeComponents()
    {
        // 1. 데이터 관련 초기화
        _dataManager = GetComponentInChildren<DataManager>();
        _saveManager = GetComponentInChildren<SaveManager>();
        _playerDataManager = GetComponentInChildren<PlayerDataManager>();
        _bossDataManager = GetComponentInChildren<BossDataManager>();
        _soundManager = GetComponentInChildren<SoundManager>();

        // 2. 오브젝트 풀링 관련 초기화
        _objectPool = GetComponentInChildren<ObjectPool>();
        _poolManager = GetComponentInChildren<PoolManager>();

        // 3. UI 관련 초기화
        _uiManager = GetComponentInChildren<UIManager>();

        // 4. 기타 매니저 초기화
        _alertManager = GetComponentInChildren<AlertManager>();
        _spawnManager = GetComponentInChildren<SpawnManager>();


        // 5. 데이터 초기화
        _dataManager.Initialize();
        _soundManager.Initialize();
    }

    private void InitializeGame()
    {
        _poolManager.AddObjectPool();
        _playerDataManager.Initialize();
        _prefabDataManager = new PrefabDataManager();
        _uiManager.InventoryManager.TriggerInventoryUpdate();
        _soundManager.SettingPopup.Initializer();
    }
    #endregion

    #region  애플리케이션 이벤트
    private void OnApplicationQuit()
    {
        isQuitting = true;

        // UI 비활성화 (오류 방지)
        if (_soundManager?.SettingPopup != null)
        {
            _soundManager.SettingPopup.gameObject.SetActive(false);
        }

        // 데이터 저장
        _playerDataManager.SavePlayerData();
        _prefabDataManager.SavePrefabData();
        _bossDataManager.SaveBossRuntimeData();
        _soundManager.SaveOptionData();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && !isQuitting)
        {
            _playerDataManager.SavePlayerData();
            _bossDataManager.SaveBossRuntimeData();
            _soundManager.SaveOptionData();
        }
    }
    #endregion

    #region  게임 데이터 참조
    private static readonly CollectedFruitData EmptyCollectedFruitData = new CollectedFruitData { ID = FruitsID.None, Amount = 0 };

    public FruitsData GetFruitsData(FruitsID id)
    {
        return _dataManager.FruitDatas[id];
    }

    public int GetFruitAmount(FruitsID id)
    {
        return _playerDataManager.NowPlayerData.Inventory.TryGetValue(id, out var collectedData) ? collectedData.Amount : 0;
    }

    public CollectedFruitData GetCollectedFruitData(FruitsID id)
    {
        return _playerDataManager.NowPlayerData.Inventory.TryGetValue(id, out var collectedData) ? collectedData : EmptyCollectedFruitData;
    }

    public BossData GetBossData(BossID id)
    {
        return _dataManager.BossDatas.TryGetValue(id, out BossData bossData) ? bossData : null;
    }
    public int GetBossReward(BossID id)
    {
        return _dataManager.BossDatas.TryGetValue(id, out BossData bossData) ? bossData.Reward : 0;
    }

    public PoolObject GetBullet()
    {
        return _bulletPrefabs;
    }
    #endregion

    #region  사운드 관련 메서드
    public AudioMixer GetAudioMixer()
    {
        return _soundManager.AudioMixer;
    }

    public void PlayBGM(BGM target)
    {
        _soundManager.PlayBGM(target);
    }

    public void PlaySFX(SFX target)
    {
        _soundManager.PlaySFX(target);
    }
    #endregion
}
