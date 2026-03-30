using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_BattleHeader : MonoBehaviour
{
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private Toggle _autoPlayToggle;

    private void OnEnable()
    {
        UpdateStageText();
        EventBus.Subscribe(GameEventType.OnStageStart, UpdateStageText);
        EventBus.Subscribe(GameEventType.OnStageClear, UpdateStageText);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(GameEventType.OnStageStart, UpdateStageText);
        EventBus.Unsubscribe(GameEventType.OnStageClear, UpdateStageText);
    }

    private void Start()
    {
        if (_autoPlayToggle != null)
        {
            // Sync with current StageManager setting
            _autoPlayToggle.isOn = StageManager.Instance.IsAutoPlay;
            _autoPlayToggle.onValueChanged.AddListener(OnAutoPlayToggle);
        }
    }

    private void UpdateStageText()
    {
        if (_stageText != null && StageManager.Instance != null)
        {
            _stageText.text = $"STAGE {StageManager.Instance.CurrentStage}";
        }
    }

    private void OnAutoPlayToggle(bool value)
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.IsAutoPlay = value;
        }
    }
}
