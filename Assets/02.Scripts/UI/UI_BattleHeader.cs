using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_BattleHeader : MonoBehaviour
{
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private Toggle _autoPlayToggle;

    private void Start()
    {
        if (_autoPlayToggle != null)
        {
            _autoPlayToggle.isOn = StageManager.Instance.IsAutoPlay;
            _autoPlayToggle.onValueChanged.AddListener(OnAutoPlayToggle);
        }
        
        UpdateStageText();
        EventBus.Subscribe(GameEventType.OnStageStart, UpdateStageText);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe(GameEventType.OnStageStart, UpdateStageText);
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
