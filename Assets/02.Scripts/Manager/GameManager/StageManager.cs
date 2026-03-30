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
    }

    public void StartStage()
    {
        if (_currentState == GameState.Playing) return;

        // Cleanup before starting new stage
        CleanupField();

        _currentState = GameState.Playing;
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
        while (_currentState == GameState.Playing)
        {
            SpawnManager.Instance.SpawnEnemy();
            _activeEnemies++;
            yield return new WaitForSeconds(_spawnInterval);
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
        yield return new WaitForSeconds(3f); // Delay before next stage
        if (_isAutoPlay && _currentState == GameState.Win)
        {
            StartStage();
        }
    }

    public void FailStage()
    {
        if (_currentState != GameState.Playing) return;

        _currentState = GameState.Lose;
        StopAllCoroutines(); // Stop spawning and stage routine
        
        Debug.Log("[StageManager] Stage Failed!");
        EventBus.Publish(GameEventType.OnStageFail);
    }

    public void ResetStage()
    {
        _currentStage = 1; // Return to stage 1
        _currentState = GameState.Ready;
        CleanupField();
    }
}
