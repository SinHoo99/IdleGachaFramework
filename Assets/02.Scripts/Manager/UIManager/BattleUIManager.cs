using UnityEngine;

public class BattleUIManager : Singleton<BattleUIManager>
{
    [Header("UI Views")]
    [SerializeField] private UI_BattlePanel _battlePanel;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("[BattleUIManager] Awake called.");
        if (_battlePanel == null)
        {
            Debug.LogError("[BattleUIManager] _battlePanel is NOT assigned in the Inspector!");
        }
    }

    private void Start()
    {
        Debug.Log("[BattleUIManager] Start called.");
        
        // 1. 이벤트 구독
        EventBus.Subscribe(GameEventType.OnStageClear, OnStageClear);
        EventBus.Subscribe(GameEventType.OnStageFail, OnStageFail);

        // 2. 처음에 시작 패널 표시
        ShowStartPanel();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe(GameEventType.OnStageClear, OnStageClear);
        EventBus.Unsubscribe(GameEventType.OnStageFail, OnStageFail);
    }

    private void ShowStartPanel()
    {
        Debug.Log("[BattleUIManager] Attempting to ShowStartPanel...");
        if (_battlePanel == null) 
        {
            Debug.LogError("[BattleUIManager] Cannot show Start Panel because _battlePanel is null!");
            return;
        }

        // UIManager의 토글 오프 로직을 방지하기 위해 Show() 전에 비활성화되어 있는지 확인
        _battlePanel.gameObject.SetActive(false);

        _battlePanel.Setup(
            "ADVENTURE", 
            "Prepare for the next stage.", 
            "START", 
            () => {
                Debug.Log("[BattleUIManager] Action Button: StartStage called.");
                StageManager.Instance.StartStage();
            }
        );
        _battlePanel.Show();
    }

    private void OnStageClear()
    {
        // 자동 플레이 중인 경우 승리 패널을 표시하지 않음
        if (StageManager.Instance != null && StageManager.Instance.IsAutoPlay) return;
        
        if (_battlePanel != null)
        {
            _battlePanel.Setup(
                "VICTORY!", 
                "Stage Cleared! You've earned rewards.", 
                "NEXT STAGE", 
                () => StageManager.Instance.StartStage()
            );
            _battlePanel.Show();
        }
    }

    private void OnStageFail()
    {
        if (_battlePanel != null)
        {
            _battlePanel.Setup(
                "DEFEAT", 
                "The enemies breached your defense.", 
                "RETRY", 
                () => {
                    StageManager.Instance.ResetStage();
                    StageManager.Instance.StartStage();
                }
            );
            _battlePanel.Show();
        }
    }
}
