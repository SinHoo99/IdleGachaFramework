using UnityEngine;

/// <summary>
/// ParringEffect 프리팹에 부착되어 애니메이션 이벤트를 플레이어에게 전달하는 역할을 합니다.
/// </summary>
public class ParryEffectDispatcher : MonoBehaviour
{
    // 애니메이션 이벤트: OnParry
    public void OnParry()
    {
        if (Player.Instance != null)
        {
            Player.Instance.StartParry();
        }
    }

    // 애니메이션 이벤트: OnParryAnimationEnd
    public void OnParryAnimationEnd()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnParryAnimationEnd();
        }
    }
}
