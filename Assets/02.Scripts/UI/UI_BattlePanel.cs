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
        if (_titleText != null) _titleText.text = title;
        if (_descriptionText != null) _descriptionText.text = description;
        if (_buttonText != null) _buttonText.text = buttonLabel;

        if (_actionButton != null)
        {
            _actionButton.onClick.RemoveAllListeners();
            _actionButton.onClick.AddListener(onAction);
            _actionButton.onClick.AddListener(Hide); // Close when clicked
        }
    }
}
