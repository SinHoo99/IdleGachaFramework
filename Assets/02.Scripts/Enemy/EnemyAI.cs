using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic AI controller that executes a sequence of EnemyAttackPatterns.
/// Attach this to Enemy prefabs that need complex behaviors (like bosses).
/// </summary>
public class EnemyAI : MonoBehaviour
{
    private Enemy _owner;
    private Animator _animator;

    [Header("Behavior Settings")]
    [SerializeField] private List<EnemyAttackPattern> _patterns;
    [SerializeField] private bool _isLooping = true;
    [SerializeField] private bool _randomize = false;

    private int _currentPatternIndex = 0;
    private Coroutine _aiCoroutine;

    private void Awake()
    {
        _owner = GetComponent<Enemy>();
        _animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Starts the AI behavior loop.
    /// </summary>
    public void StartAI()
    {
        if (_patterns == null || _patterns.Count == 0) return;
        
        StopAI();
        _aiCoroutine = StartCoroutine(AIRoutine());
    }

    /// <summary>
    /// Stops the AI behavior loop.
    /// </summary>
    public void StopAI()
    {
        if (_aiCoroutine != null)
        {
            StopCoroutine(_aiCoroutine);
            _aiCoroutine = null;
        }
    }

    private IEnumerator AIRoutine()
    {
        // Initial delay before starting first attack
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (_patterns.Count == 0) yield break;

            // 1. Select Pattern
            var pattern = _patterns[_currentPatternIndex];

            // 2. Play Animation if defined
            if (_animator != null && !string.IsNullOrEmpty(pattern.AnimationTrigger))
            {
                _animator.SetTrigger(pattern.AnimationTrigger);
            }

            // 3. Execute Logic
            Debug.Log($"[EnemyAI] Executing pattern: {pattern.name}");
            pattern.Execute(_owner);

            // 4. Wait for Cooldown
            yield return new WaitForSeconds(pattern.Cooldown);

            // 5. Progress to next pattern
            if (_randomize)
            {
                _currentPatternIndex = Random.Range(0, _patterns.Count);
            }
            else
            {
                _currentPatternIndex++;
                if (_currentPatternIndex >= _patterns.Count)
                {
                    if (_isLooping) _currentPatternIndex = 0;
                    else yield break;
                }
            }
        }
    }

    private void OnDisable()
    {
        StopAI();
    }
}
