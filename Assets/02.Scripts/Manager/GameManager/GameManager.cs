using UnityEngine;
using UnityEngine.Audio;

public class GameManager : Singleton<GameManager>
{
    #region Script Setup


    #endregion

    protected override void Awake()
    {
        if (IsDuplicates()) return;
        base.Awake();
        
        Application.targetFrameRate = 60;
        InitializeComponents();
    }

    private GameState _currentState = GameState.Ready;
    public GameState CurrentState => _currentState;

    private void Start()
    {
        InitializeGame();
    }

    #region 초기화 로직
    private void InitializeComponents()
    {
        // 1. DataManager (CSV 로드)
        if (DataManager.Instance != null) DataManager.Instance.Initialize();
        
        // 2. SoundManager (설정 로드 및 볼륨 적용)
        if (SoundManager.Instance != null) 
        {
            SoundManager.Instance.Initialize();
            SoundManager.Instance.LoadOptionData();
        }

        // 2.5 StageManager
        if (StageManager.Instance != null)
        {
            // StageManager 자체에서 초기 상태 설정
        }
    }

    public void SetGameState(GameState newState)
    {
        _currentState = newState;
        Debug.Log($"[GameManager] 게임 상태 변경: {newState}");
    }

    private void InitializeGame()
    {
        // 3. 오브젝트 풀 (DataManager의 캐시된 데이터 사용)
        if (PoolManager.Instance != null) PoolManager.Instance.AddObjectPool();
        
        // 4. 플레이어 데이터 (저장 파일 로드)
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.Initialize();
        
        // 5. 시각적 상태 (로드된 플레이어 데이터를 기반으로 아이템 생성)
        if (SpawnManager.Instance != null) SpawnManager.Instance.SpawnInitialUnits();
        
        // 6. UI 동기화
        if (UIManager.Instance?.InventoryManager != null)
            UIManager.Instance.InventoryManager.TriggerInventoryUpdate();
            
        if (SoundManager.Instance?.SettingPopup != null)
            SoundManager.Instance.SettingPopup.Initializer();
            
        Debug.Log("[GameManager] 정의된 순서에 따라 전체 초기화 완료.");
    }
    #endregion

    #region 애플리케이션 이벤트
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        if (SoundManager.Instance?.SettingPopup != null)
        {
            SoundManager.Instance.SettingPopup.gameObject.SetActive(false);
        }

        // SaveAllData(); // 저장을 방지하려면 주석 처리
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // SaveAllData(); // 저장을 방지하려면 주석 처리
        }
    }

    public void SaveAllData()
    {
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.SavePlayerData();
        if (PrefabDataManager.Instance != null) PrefabDataManager.Instance.SavePrefabData();
        if (SoundManager.Instance != null) SoundManager.Instance.SaveOptionData();
    }
    #endregion

    #region Data Accessors
    // Prefab accessors moved to PoolManager
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
