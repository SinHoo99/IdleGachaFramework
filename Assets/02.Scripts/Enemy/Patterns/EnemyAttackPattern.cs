using UnityEngine;

/// <summary>
/// 모든 적/보스 공격 행동의 기본 클래스입니다.
/// 서로 다른 적들 사이에서 재사용 가능한 패턴을 허용하기 위해 ScriptableObject로 생성되었습니다.
/// </summary>
public abstract class EnemyAttackPattern : ScriptableObject
{
    [Header("Pattern Base Settings")]
    public float Cooldown = 2f;
    public string AnimationTrigger = "Attack"; // 재생할 선택적 애니메이션 트리거

    /// <summary>
    /// 공격 로직을 실행합니다.
    /// </summary>
    /// <param name="owner">이 패턴을 실행하는 Enemy 인스턴스입니다.</param>
    public abstract void Execute(Enemy owner);
}
