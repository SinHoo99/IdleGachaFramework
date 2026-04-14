using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_StatPopup : UI_Base
{
    [Header("Enhancement Info")]
    [SerializeField] private TextMeshProUGUI _enhanceLevelText; // "강화 레벨: 5" 형식

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

        // 1. 강화 레벨 (현재 레벨을 강화 레벨로 표시)
        if (_enhanceLevelText != null) 
            _enhanceLevelText.text = $"Level: {data.Level}";

        // 2. 공격력
        if (_damageText != null) 
            _damageText.text = $"{data.Damage:F0}";

        // 3. 체력 정보 (현재/최대)
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

        // 4. 공격 속도
        if (_speedText != null) 
            _speedText.text = $"{data.AttackSpeed:F1}";

        // 5. 사거리
        if (_rangeText != null) 
            _rangeText.text = $"{data.AttackRange:F1}";
    }
}
