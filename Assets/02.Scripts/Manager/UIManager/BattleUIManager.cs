using UnityEngine;

public class BattleUIManager : Singleton<BattleUIManager>
{
    [Header("UI Views")]
    [SerializeField] private UI_BattlePanel _battlePanel;

    private void Start()
    {
        // 1. Subscribe to Events
        EventBus.Subscribe(GameEventType.OnStageClear, OnStageClear);
        EventBus.Subscribe(GameEventType.OnStageFail, OnStageFail);

        // 2. Show Start Panel initially
        ShowStartPanel();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe(GameEventType.OnStageClear, OnStageClear);
        EventBus.Unsubscribe(GameEventType.OnStageFail, OnStageFail);
    }

    private void ShowStartPanel()
    {
        if (_battlePanel == null) return;

        // Ensure it's deactivated before Show() to prevent UIManager toggle-off logic
        _battlePanel.gameObject.SetActive(false);

        _battlePanel.Setup(
            "ADVENTURE", 
            "Prepare for the next stage.", 
            "START", 
            () => StageManager.Instance.StartStage()
        );
        _battlePanel.Show();
    }

    private void OnStageClear()
    {
        // If auto-playing, don't show the win panel
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
