using UnityEngine;

public class BattleUIManager : Singleton<BattleUIManager>
{
    [Header("UI Views")]
    [SerializeField] private UI_BattlePanel _startPanel;
    [SerializeField] private UI_BattlePanel _winPanel;
    [SerializeField] private UI_BattlePanel _losePanel;

    private void Start()
    {
        // 1. Subscribe to Events
        EventBus.Subscribe(GameEventType.OnStageClear, OnStageClear);
        EventBus.Subscribe(GameEventType.OnStageFail, OnStageFail);

        // 2. Setup Panels
        SetupPanels();

        // 3. Show Start Panel initially
        if (_startPanel != null) _startPanel.Show();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe(GameEventType.OnStageClear, OnStageClear);
        EventBus.Unsubscribe(GameEventType.OnStageFail, OnStageFail);
    }

    private void SetupPanels()
    {
        if (_startPanel != null)
        {
            _startPanel.Setup(
                "ADVENTURE", 
                "Prepare for the next stage.", 
                "START", 
                () => StageManager.Instance.StartStage()
            );
        }

        if (_winPanel != null)
        {
            _winPanel.Setup(
                "VICTORY!", 
                "Stage Cleared! You've earned rewards.", 
                "NEXT STAGE", 
                () => StageManager.Instance.StartStage()
            );
        }

        if (_losePanel != null)
        {
            _losePanel.Setup(
                "DEFEAT", 
                "The enemies breached your defense.", 
                "RETRY", 
                () => {
                    StageManager.Instance.ResetStage();
                    StageManager.Instance.StartStage();
                }
            );
        }
    }

    private void OnStageClear()
    {
        // If auto-playing, don't show the win panel
        if (StageManager.Instance != null && StageManager.Instance.IsAutoPlay) return;
        
        if (_winPanel != null) _winPanel.Show();
    }

    private void OnStageFail()
    {
        if (_losePanel != null) _losePanel.Show();
    }
}
