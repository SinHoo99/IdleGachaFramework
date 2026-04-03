using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("Experience UI")]
    [SerializeField] private Slider _expSlider;
    [SerializeField] private TextMeshProUGUI _expText;
    [SerializeField] private TextMeshProUGUI _levelText;

    private void Start()
    {
        if (PlayerDataManager.Instance != null)
        {
            // 이벤트 구독
            PlayerDataManager.Instance.OnExpChanged += UpdateExpUI;
            PlayerDataManager.Instance.OnLevelChanged += UpdateLevelUI;

            // 초기 데이터 설정
            var data = PlayerDataManager.Instance.NowPlayerData;
            if (data != null)
            {
                UpdateExpUI(data.CurrentExp, data.MaxExp);
                UpdateLevelUI(data.Level);
            }
        }
    }

    private void OnDestroy()
    {
        // 앱 종료 중이 아닐 때만 이벤트 해제 시도
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnExpChanged -= UpdateExpUI;
            PlayerDataManager.Instance.OnLevelChanged -= UpdateLevelUI;
        }
    }

    private void UpdateExpUI(float currentExp, float maxExp)
    {
        if (_expSlider != null)
        {
            _expSlider.maxValue = maxExp;
            _expSlider.value = currentExp;
        }

        if (_expText != null)
        {
            _expText.text = $"{currentExp:F0} / {maxExp:F0}";
        }
    }

    private void UpdateLevelUI(int level)
    {
        if (_levelText != null)
        {
            _levelText.text = $"LV. {level}";
        }
    }
}
