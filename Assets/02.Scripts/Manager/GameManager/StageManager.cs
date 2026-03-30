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

    [Header("Current Progress")]
    private int _remainingEnemiesToSpawn;
    private int _activeEnemies;
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

        _currentState = GameState.Playing;
        _remainingEnemiesToSpawn = _enemiesPerStage + (_currentStage * 2); // Example scaling
        _activeEnemies = 0;

        Debug.Log($"[StageManager] Stage {_currentStage} Started!");
        EventBus.Publish(GameEventType.OnStageStart);
        
        StartCoroutine(StageRoutine());
    }

    private IEnumerator StageRoutine()
    {
        while (_remainingEnemiesToSpawn > 0)
        {
            SpawnManager.Instance.SpawnEnemy();
            _remainingEnemiesToSpawn--;
            _activeEnemies++;
            yield return new WaitForSeconds(_spawnInterval);
        }

        // Wait until all enemies are defeated
        while (_activeEnemies > 0)
        {
            yield return null;
        }

        CompleteStage();
    }

    public void OnEnemyDefeated()
    {
        _activeEnemies--;
        EventBus.Publish(GameEventType.OnEnemyDefeated);
    }

    private void CompleteStage()
    {
        _currentState = GameState.Win;
        Debug.Log($"[StageManager] Stage {_currentStage} Cleared!");
        EventBus.Publish(GameEventType.OnStageClear);

        _currentStage++;

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
        _currentState = GameState.Lose;
        StopAllCoroutines();
        Debug.Log("[StageManager] Stage Failed!");
        EventBus.Publish(GameEventType.OnStageFail);
    }

    public void ResetStage()
    {
        _currentStage = 1;
        _currentState = GameState.Ready;
    }
}
