using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [Header("Stage Settings")]
    [SerializeField] private int _currentStage = 1;
    [SerializeField] private bool _isAutoPlay = false;
    [SerializeField] private int _enemiesPerStage = 5;
    [SerializeField] private float _spawnInterval = 2f;
    [SerializeField] private int _winThreshold = 5;  // 5마리 잡으면 승리
    [SerializeField] private int _loseThreshold = 5; // 5마리 놓치면 패배

    [Header("Current Progress")]
    private int _remainingEnemiesToSpawn;
    private int _activeEnemies;
    private int _defeatedEnemies; // 처치한 수
    private int _escapedEnemies;  // 놓친 수
    private GameState _currentState = GameState.Ready;

    public int CurrentStage => _currentStage;
    public bool IsAutoPlay { get => _isAutoPlay; set => _isAutoPlay = value; }
    public GameState CurrentState => _currentState;

    private void Start()
    {
        _currentState = GameState.Ready;
        
        // 보상 이벤트 구독
        EventBus<EnemyData>.Subscribe(GameEventType.OnRewardEarned, HandleEnemyReward);
    }

    private void OnDestroy()
    {
        EventBus<EnemyData>.Unsubscribe(GameEventType.OnRewardEarned, HandleEnemyReward);
    }

    private void HandleEnemyReward(EnemyData data)
    {
        if (data == null) return;

        // 1. 경험치 지급
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.GainExp(data.Exp);
        }

        // 2. 보스일 경우 추가 보상
        if (data.Type == EntityType.Boss)
        {
            if (PlayerDataManager.Instance?.NowPlayerData != null)
            {
                PlayerDataManager.Instance.NowPlayerData.PlayerCoin += 100; // 보스 보상
                PlayerDataManager.Instance.SavePlayerData();
            }
        }

        Debug.Log($"[StageManager] Reward Earned from {data.Name}: {data.Exp} EXP");
    }

    public void StartStage()
    {
        if (_currentState == GameState.Playing) return;

        // 새 스테이지를 시작하기 전에 필드 정리
        CleanupField();

        _currentState = GameState.Playing;
        if (GameManager.Instance != null) GameManager.Instance.SetGameState(GameState.Playing);

        _remainingEnemiesToSpawn = 100; // 충분히 많이 스폰 (조건 달성 전까지)
        _activeEnemies = 0;
        _defeatedEnemies = 0;
        _escapedEnemies = 0;

        Debug.Log($"[StageManager] Stage {_currentStage} Started! (Win: {_winThreshold}, Lose: {_loseThreshold})");
        EventBus.Publish(GameEventType.OnStageStart);
        
        StartCoroutine(StageRoutine());
    }

    private void CleanupField()
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.ReturnAllUnitToPool(); 
        }
    }

    private IEnumerator StageRoutine()
    {
        var enemyData = DataManager.Instance.GetEnemyData(_currentStage);
        if (enemyData == null)
        {
            Debug.LogError($"[StageManager] No EnemyData found for stage {_currentStage}!");
            yield break;
        }

        // CSV의 Count를 승리 조건으로 사용 (테스트를 위해 임시로 5로 설정)
        // _winThreshold = enemyData.Count;
        // _remainingEnemiesToSpawn = enemyData.Count;
        _winThreshold = 5; 
        _remainingEnemiesToSpawn = 5; 

        _defeatedEnemies = 0;
        _escapedEnemies = 0;

        if (enemyData.Type == EntityType.Boss)
        {
            // 보스 생성 (통합된 SpawnEnemy 로직 사용)
            SpawnManager.Instance.SpawnEnemy(_currentStage);
            _activeEnemies = 1;
            _remainingEnemiesToSpawn = 0;
        }
        else
        {
            // Count에 의해 제한되는 일반적인 적 생성 동작
            while (_remainingEnemiesToSpawn > 0 && _currentState == GameState.Playing)
            {
                SpawnManager.Instance.SpawnEnemy(_currentStage);
                _activeEnemies++;
                _remainingEnemiesToSpawn--;
                
                if (_remainingEnemiesToSpawn > 0)
                {
                    yield return new WaitForSeconds(_spawnInterval);
                }
            }
        }
    }

    public void OnEnemyDefeated()
    {
        if (_currentState != GameState.Playing) return;

        _activeEnemies--;
        _defeatedEnemies++;
        
        Debug.Log($"[StageManager] Enemy Defeated: {_defeatedEnemies}/{_winThreshold}");
        EventBus.Publish(GameEventType.OnEnemyDefeated);

        if (_defeatedEnemies >= _winThreshold)
        {
            CompleteStage();
        }
    }

    public void OnEnemyEscaped()
    {
        if (_currentState != GameState.Playing) return;

        _activeEnemies--;
        _escapedEnemies++;

        Debug.Log($"[StageManager] Enemy Escaped: {_escapedEnemies}/{_loseThreshold}");

        if (_escapedEnemies >= _loseThreshold)
        {
            FailStage();
        }
    }

    private void CompleteStage()
    {
        if (_currentState != GameState.Playing) return;

        _currentState = GameState.Win;
        if (GameManager.Instance != null) GameManager.Instance.SetGameState(GameState.Win);
        
        Debug.Log($"[StageManager] Stage {_currentStage} Cleared!");
        
        _currentStage++;
        EventBus.Publish(GameEventType.OnStageClear);

        if (_isAutoPlay)
        {
            StartCoroutine(AutoPlayNextStage());
        }
    }

    private IEnumerator AutoPlayNextStage()
    {
        yield return new WaitForSeconds(3f); // 다음 스테이지까지의 지연 시간
        if (_isAutoPlay && _currentState == GameState.Win)
        {
            StartStage();
        }
    }

    public void FailStage()
    {
        if (_currentState != GameState.Playing) return;

        _currentState = GameState.Lose;
        if (GameManager.Instance != null) GameManager.Instance.SetGameState(GameState.Lose);
        
        StopAllCoroutines(); // 생성 및 스테이지 루틴 중지
        
        Debug.Log("[StageManager] Stage Failed!");
        EventBus.Publish(GameEventType.OnStageFail);
    }

    public void ResetStage()
    {
        _currentStage = 1; // 1스테이지로 돌아감
        _currentState = GameState.Ready;
        if (GameManager.Instance != null) GameManager.Instance.SetGameState(GameState.Ready);
        
        CleanupField();
    }
}
