using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ForgeUI : MonoBehaviour
{
    [Header("Detailed Info")]
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _statTitleText;
    [SerializeField] private TextMeshProUGUI _currentStatText;
    [SerializeField] private TextMeshProUGUI _nextStatText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _pityChanceText;

    [Header("Buttons")]
    [SerializeField] private Button _enhanceButton;
    [SerializeField] private Button _gradeUpButton;
    [SerializeField] private Button _closeButton;

    private EquipmentInstance _selectedItem;

    private void Awake()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);
            
        if (_enhanceButton != null)
            _enhanceButton.onClick.AddListener(OnEnhanceClicked);
            
        if (_gradeUpButton != null)
            _gradeUpButton.onClick.AddListener(OnGradeUpClicked);
            
        gameObject.SetActive(false);
    }

    public void Open(EquipmentInstance item)
    {
        if (item == null) return;
        
        _selectedItem = item;
        gameObject.SetActive(true);
        
        // 팝업 열기 연출
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        
        RefreshDetails();
    }

    public void Close()
    {
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }

    private void RefreshDetails()
    {
        if (_selectedItem == null) return;

        _itemNameText.text = $"{_selectedItem.Grade} {_selectedItem.Type}";
        _itemNameText.color = GetGradeColor(_selectedItem.Grade);
        
        float currentStat = EquipmentDataManager.Instance.GetStat(_selectedItem);
        _statTitleText.text = GetStatName(_selectedItem.Type);
        _currentStatText.text = currentStat.ToString("F1");

        if (_selectedItem.Level < 10)
        {
            _enhanceButton.gameObject.SetActive(true);
            _gradeUpButton.gameObject.SetActive(false);

            _costText.text = EquipmentDataManager.Instance.GetEnhanceCost(_selectedItem).ToString();
            _pityChanceText.text = "Guaranteed Success";

            EquipmentInstance nextLevelFake = new EquipmentInstance(_selectedItem.Type) { Grade = _selectedItem.Grade, Level = _selectedItem.Level + 1 };
            _nextStatText.text = $"> {EquipmentDataManager.Instance.GetStat(nextLevelFake):F1}";
        }
        else
        {
            _enhanceButton.gameObject.SetActive(false);
            _gradeUpButton.gameObject.SetActive(true);

            if (_selectedItem.Grade == EquipmentGrade.Legendary)
            {
                _gradeUpButton.interactable = false;
                _costText.text = "MAX GRADE";
                _nextStatText.text = "";
                _pityChanceText.text = "";
            }
            else
            {
                _gradeUpButton.interactable = true;
                _costText.text = EquipmentDataManager.Instance.GetGradeUpCost(_selectedItem).ToString();
                
                // 확률 계산 표시 (Common:50%, Rare:25%, Epic:10%)
                float baseChance = _selectedItem.Grade switch {
                    EquipmentGrade.Common => 0.5f,
                    EquipmentGrade.Rare => 0.25f,
                    EquipmentGrade.Epic => 0.1f,
                    _ => 0f
                };
                _pityChanceText.text = $"Chance: {((baseChance + _selectedItem.PityBonus) * 100):F0}%";

                EquipmentInstance nextGradeFake = new EquipmentInstance(_selectedItem.Type) { Grade = _selectedItem.Grade + 1, Level = 1 };
                _nextStatText.text = $"> {EquipmentDataManager.Instance.GetStat(nextGradeFake):F1}";
            }
        }
    }

    public void OnEnhanceClicked()
    {
        if (_selectedItem == null) return;

        if (EquipmentDataManager.Instance.Enhance(_selectedItem))
        {
            _enhanceButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
            RefreshDetails();
        }
    }

    public void OnGradeUpClicked()
    {
        if (_selectedItem == null) return;

        bool success = EquipmentDataManager.Instance.GradeUp(_selectedItem);
        if (success)
        {
            _gradeUpButton.transform.DOShakePosition(0.5f, 10f);
            RefreshDetails();
        }
        else
        {
            _gradeUpButton.transform.DOShakeRotation(0.3f, 5f);
            RefreshDetails(); 
        }
    }

    private string GetStatName(EquipmentType type) => type switch {
        EquipmentType.Weapon => "Damage",
        EquipmentType.Armor => "Max HP",
        EquipmentType.Glove => "Atk Speed",
        EquipmentType.Ring => "Atk Range",
        _ => "Stat"
    };

    private Color GetGradeColor(EquipmentGrade grade) => grade switch {
        EquipmentGrade.Common => Color.white,
        EquipmentGrade.Rare => new Color(0.2f, 0.6f, 1f),
        EquipmentGrade.Epic => new Color(0.7f, 0.2f, 1f),
        EquipmentGrade.Legendary => new Color(1f, 0.6f, 0f),
        _ => Color.white
    };
}
