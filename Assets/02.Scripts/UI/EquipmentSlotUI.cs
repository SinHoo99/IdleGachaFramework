using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentSlotUI : MonoBehaviour
{
    [SerializeField] private EquipmentType _type;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image _gradeBorder; // 등급에 따른 테두리 색상
    [SerializeField] private GameObject _selectOverlay;

    private EquipmentInstance _item;
    private ForgeUI _forgeUI;

    public void Setup(EquipmentInstance item, ForgeUI forgeUI)
    {
        _item = item;
        _forgeUI = forgeUI;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_item == null) return;

        // 레벨 표시 (+1, +10 등)
        _levelText.text = $"+{_item.Level}";

        // 등급 색상 (간단히 구현)
        Color gradeColor = GetGradeColor(_item.Grade);
        if (_gradeBorder != null) _gradeBorder.color = gradeColor;
        _levelText.color = gradeColor;

        // 아이콘 등은 추후 Resource 로드나 Atlas를 통해 설정 가능
    }

    private Color GetGradeColor(EquipmentGrade grade)
    {
        return grade switch
        {
            EquipmentGrade.Common => Color.white,
            EquipmentGrade.Rare => new Color(0.2f, 0.6f, 1f), // Blue
            EquipmentGrade.Epic => new Color(0.7f, 0.2f, 1f), // Purple
            EquipmentGrade.Legendary => new Color(1f, 0.6f, 0f), // Orange/Gold
            _ => Color.white
        };
    }

    public void OnClick()
    {
        if (_forgeUI != null && _item != null)
        {
            _forgeUI.Open(_item);
        }
    }

    public void SetSelect(bool isSelected)
    {
        if (_selectOverlay != null) _selectOverlay.SetActive(isSelected);
    }
}
