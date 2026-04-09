using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일련의 EnemyAttackPattern을 실행하는 범용 AI 컨트롤러입니다.
/// 복잡한 행동이 필요한 적 프리팹(예: 보스)에 이 컴포넌트를 부착하십시오.
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
    /// AI 행동 루프를 시작합니다.
    /// </summary>
    public void StartAI()
    {
        if (_patterns == null || _patterns.Count == 0) return;
        
        StopAI();
        _aiCoroutine = StartCoroutine(AIRoutine());
    }

    /// <summary>
    /// AI 행동 루프를 중지합니다.
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
        // 첫 번째 공격을 시작하기 전의 초기 지연 시간
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (_patterns.Count == 0) yield break;

            // 1. 패턴 선택
            var pattern = _patterns[_currentPatternIndex];

            // 2. 애니메이션이 정의된 경우 애니메이션 재생
            if (_animator != null && !string.IsNullOrEmpty(pattern.AnimationTrigger))
            {
                _animator.SetTrigger(pattern.AnimationTrigger);
            }

            // 3. 로직 실행
            Debug.Log($"[EnemyAI] Executing pattern: {pattern.name}");
            pattern.Execute(_owner);

            // 4. 쿨다운 대기
            yield return new WaitForSeconds(pattern.Cooldown);

            // 5. 다음 패턴으로 진행
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
