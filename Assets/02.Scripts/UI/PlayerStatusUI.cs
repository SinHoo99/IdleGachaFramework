using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("Experience UI")]
    [SerializeField] private Slider _expSlider;
    [SerializeField] private TextMeshProUGUI _expText;
    [SerializeField] private TextMeshProUGUI _levelText;

    private float _lastMaxExp = -1;
    private float _lastCurrentExp = -1;
    private Sequence _expSequence;

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
                // 초기 설정은 애니메이션 없이 즉시 반영
                _lastMaxExp = data.MaxExp;
                _lastCurrentExp = data.CurrentExp;
                
                if (_expSlider != null)
                {
                    _expSlider.maxValue = _lastMaxExp;
                    _expSlider.value = _lastCurrentExp;
                }
                UpdateLevelUI(data.Level);
                UpdateExpText(_lastCurrentExp, _lastMaxExp);
            }
        }
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnExpChanged -= UpdateExpUI;
            PlayerDataManager.Instance.OnLevelChanged -= UpdateLevelUI;
        }
        
        _expSequence?.Kill();
    }

    private void UpdateExpUI(float currentExp, float maxExp)
    {
        if (_expSlider == null) return;

        // 기존 애니메이션 중지
        _expSequence?.Kill();
        _expSequence = DOTween.Sequence();

        // 레벨업 발생 여부 체크 (최대 경험치가 늘어났거나, 현재 경험치가 줄어들었을 때)
        if (_lastMaxExp > 0 && (maxExp > _lastMaxExp || currentExp < _lastCurrentExp))
        {
            // 1. 이전 Max치까지 채우기
            _expSequence.Append(_expSlider.DOValue(_lastMaxExp, 0.3f).SetEase(Ease.InCubic));
            
            // 2. 0으로 즉시 초기화 및 새로운 MaxExp 설정
            _expSequence.AppendCallback(() => {
                _expSlider.maxValue = maxExp;
                _expSlider.value = 0;
            });

            // 3. 다시 새로운 목표치까지 채우기
            _expSequence.Append(_expSlider.DOValue(currentExp, 0.4f).SetEase(Ease.OutCubic));
        }
        else
        {
            // 일반적인 경험치 획득 상황
            _expSlider.maxValue = maxExp;
            _expSequence.Append(_expSlider.DOValue(currentExp, 0.5f).SetEase(Ease.OutCubic));
        }

        // 텍스트는 즉시 혹은 애니메이션 도중에 업데이트
        UpdateExpText(currentExp, maxExp);

        _lastMaxExp = maxExp;
        _lastCurrentExp = currentExp;
    }

    private void UpdateExpText(float current, float max)
    {
        if (_expText != null)
        {
            _expText.text = $"{current:F0} / {max:F0}";
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
