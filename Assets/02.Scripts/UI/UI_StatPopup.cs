using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UI_StatPopup : UI_Base
{
    [Serializable]
    public struct EquipmentUIContainer
    {
        public EquipmentType Type;
        public Image IconImage;
        public Image GradeBGImage;
        public TextMeshProUGUI InfoText;
        public Button SlotButton; // 버튼 추가
    }

    [Header("Enhancement Info")]
    [SerializeField] private TextMeshProUGUI _enhanceLevelText; // 캐릭터 레벨

    [Header("Equipment Visuals")]
    [SerializeField] private EquipmentUIContainer[] _equipmentUIs;
    [SerializeField] private ForgeUI _forgePopup; // 강화 팝업 참조

    [Header("Combat Stats")]
    [SerializeField] private TextMeshProUGUI _damageText;      // 공격력
    [SerializeField] private TextMeshProUGUI _hpText;          // 체력 (현재/최대)
    [SerializeField] private TextMeshProUGUI _speedText;       // 공격 속도
    [SerializeField] private TextMeshProUGUI _rangeText;       // 사거리

    [Header("Buttons")]
    [SerializeField] private Button _closeButton;

    private void Start()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);

        // 슬롯 버튼 이벤트 연결
        if (_equipmentUIs != null)
        {
            foreach (var ui in _equipmentUIs)
            {
                var type = ui.Type; // 클로저 캡처 방지
                if (ui.SlotButton != null)
                    ui.SlotButton.onClick.AddListener(() => OnSlotClicked(type));
            }
        }
    }

    private void OnSlotClicked(EquipmentType type)
    {
        if (PlayerDataManager.Instance.NowPlayerData.EquippedItems.TryGetValue(type, out var item))
        {
            if (_forgePopup != null)
            {
                _forgePopup.Open(item);
            }
        }
    }

    private void OnEnable()
    {
        UpdateUI();
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnStatChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnStatChanged -= UpdateUI;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void UpdateUI()
    {
        var data = PlayerDataManager.Instance.NowPlayerData;
        if (data == null) return;

        // 1. 캐릭터 레벨
        if (_enhanceLevelText != null) 
            _enhanceLevelText.text = $"Level: {data.Level}";

        // 2. 장비 비주얼 업데이트
        if (_equipmentUIs != null)
        {
            foreach (var ui in _equipmentUIs)
            {
                UpdateEquipmentSlot(ui);
            }
        }

        // 3. 공격력
        if (_damageText != null) 
            _damageText.text = $"{data.Damage:F0}";

        // 4. 체력 정보
        var health = Player.Instance?.GetComponent<HealthSystem>();
        if (health != null)
        {
            if (_hpText != null) 
                _hpText.text = $"{health.CurHP:F0} / {health.MaxHP:F0}";
        }
        else
        {
            if (_hpText != null) 
                _hpText.text = $"- / {data.MaxHP:F0}";
        }

        // 5. 공격 속도
        if (_speedText != null) 
            _speedText.text = $"{data.AttackSpeed:F1}";

        // 6. 사거리
        if (_rangeText != null) 
            _rangeText.text = $"{data.AttackRange:F1}";
    }

    private void UpdateEquipmentSlot(EquipmentUIContainer ui)
    {
        if (PlayerDataManager.Instance.NowPlayerData.EquippedItems == null) return;

        if (PlayerDataManager.Instance.NowPlayerData.EquippedItems.TryGetValue(ui.Type, out var item))
        {
            Color gradeColor = GetGradeColor(item.Grade);
            
            if (ui.InfoText != null)
                ui.InfoText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(gradeColor)}>{item.Grade}</color> +{item.Level}";

            if (ui.GradeBGImage != null)
                ui.GradeBGImage.color = gradeColor;

            if (ui.IconImage != null)
                ui.IconImage.gameObject.SetActive(true);
        }
        else
        {
            if (ui.InfoText != null) ui.InfoText.text = "None";
            if (ui.GradeBGImage != null) ui.GradeBGImage.color = Color.gray;
            if (ui.IconImage != null) ui.IconImage.gameObject.SetActive(false);
        }
    }

    private Color GetGradeColor(EquipmentGrade grade) => grade switch {
        EquipmentGrade.Common => Color.white,
        EquipmentGrade.Rare => new Color(0.2f, 0.6f, 1f),
        EquipmentGrade.Epic => new Color(0.7f, 0.2f, 1f),
        EquipmentGrade.Legendary => new Color(1f, 0.6f, 0f),
        _ => Color.white
    };
}
