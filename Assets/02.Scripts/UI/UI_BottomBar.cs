using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 하단 바의 모든 UI 요소(스탯 버튼, 오토 토글 등)를 통합 관리하는 클래스입니다.
/// </summary>
public class UI_BottomBar : MonoBehaviour
{
    [Header("Stat Info")]
    [SerializeField] private Button _statButton;

    [Header("Auto Play")]
    [SerializeField] private Toggle _autoPlayToggle;
    [SerializeField] private GameObject _autoEffect; 
    [SerializeField] private AutoPlayController _autoPlayController;

    private void Awake()
    {
        // 컨트롤러가 할당되지 않았다면 같은 오브젝트에서 찾아봅니다.
        if (_autoPlayController == null)
            _autoPlayController = GetComponent<AutoPlayController>();
    }

    private void Start()
    {
        // 1. 스탯 팝업 버튼 연결
        if (_statButton != null)
        {
            _statButton.onClick.AddListener(() => {
                if (UIManager.Instance != null)
                    UIManager.Instance.ToggleStatPopup();
            });
        }

        // 2. 오토 플레이 토글 초기화 및 연결
        if (_autoPlayToggle != null && _autoPlayController != null)
        {
            _autoPlayToggle.isOn = _autoPlayController.IsAutoPlay;
            UpdateAutoEffect(_autoPlayController.IsAutoPlay);

            _autoPlayToggle.onValueChanged.AddListener(OnAutoToggleChanged);
        }
    }

    private void OnAutoToggleChanged(bool value)
    {
        if (_autoPlayController != null)
        {
            _autoPlayController.IsAutoPlay = value;
            UpdateAutoEffect(value);
        }
    }

    private void OnEnable()
    {
        if (_autoPlayToggle != null && _autoPlayController != null)
        {
            bool currentState = _autoPlayController.IsAutoPlay;
            _autoPlayToggle.SetIsOnWithoutNotify(currentState);
            UpdateAutoEffect(currentState);
        }
    }

    private void UpdateAutoEffect(bool isActive)
    {
        if (_autoEffect != null)
            _autoEffect.SetActive(isActive);
    }
}
