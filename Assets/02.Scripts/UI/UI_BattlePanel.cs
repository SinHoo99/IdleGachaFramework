using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UI_BattlePanel : UI_Base
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TMP_Text _buttonText;

    public void Setup(string title, string description, string buttonLabel, UnityAction onAction)
    {
        Debug.Log($"[UI_BattlePanel] Setup - Title: {title}, Desc: {description}");

        if (_titleText != null) _titleText.text = title;
        else Debug.LogWarning("[UI_BattlePanel] _titleText is missing!");

        if (_descriptionText != null) _descriptionText.text = description;
        else Debug.LogWarning("[UI_BattlePanel] _descriptionText is missing!");

        if (_buttonText != null) _buttonText.text = buttonLabel;

        if (_actionButton != null)
        {
            _actionButton.onClick.RemoveAllListeners();
            _actionButton.onClick.AddListener(() => {
                Debug.Log("[UI_BattlePanel] Action Button Clicked!");
                onAction?.Invoke();
                Hide(); // Now using the overridden Hide with UIManager
            });
        }
        else Debug.LogWarning("[UI_BattlePanel] _actionButton is missing!");
    }

    public override void Hide()
    {
        // UI_Base의 단순 SetActive(false) 대신 UIManager를 통해 애니메이션과 함께 닫음
        if (UIManager.Instance != null)
        {
            // UIManager에 접근하여 현재 활성 UI를 닫는 로직 호출 (필요시 UIManager 수정)
            gameObject.SetActive(false); 
        }
        else
        {
            base.Hide();
        }
    }
}
