using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI GameoverText;
    [SerializeField] private TextMeshProUGUI ContinueText;
    [SerializeField] private Button ContinueButton;

    private Sequence sequence;

    private void Start()
    {
        StartTween();
    }

    private void StartTween()
    {
        // 시퀀스 생성
        sequence = DOTween.Sequence().SetUpdate(true);

        // 트윈 생성
        Tween gameoverText = GameoverText.DOFade(1f, 1.5f).SetUpdate(true);
        Tween continueText = ContinueText.DOFade(1f, 1.0f).SetUpdate(true);

        // 시퀀스에 트윈 추가
        sequence.Append(gameoverText);
        sequence.Append(continueText);
    }

    public void OnClickContinue()
    {
        // 플레이어 데이터 매니저의 싱글톤 인스턴스를 직접 사용하여 데이터를 초기화합니다.
        PlayerDataManager.Instance?.DestroyData();
    }
}
