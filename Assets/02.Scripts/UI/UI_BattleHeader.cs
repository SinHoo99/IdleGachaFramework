using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_BattleHeader : MonoBehaviour
{
    [SerializeField] private TMP_Text _stageText;

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

    private void UpdateStageText()
    {
        if (_stageText != null && StageManager.Instance != null)
        {
            _stageText.text = $"STAGE {StageManager.Instance.CurrentStage}";
        }
    }
}
