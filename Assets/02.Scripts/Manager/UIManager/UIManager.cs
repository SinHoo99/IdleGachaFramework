using DG.Tweening;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private DictionaryManager _dictionaryManager;
    [SerializeField] private UI_CardSelectionPopup _cardSelectionPopup;
    [SerializeField] private UI_StatPopup _statPopup;
    
    public InventoryManager InventoryManager => _inventoryManager;
    public DictionaryManager DictionaryManager => _dictionaryManager;

    private GameObject _currentActiveUI = null;
    private float _frontZ = -5f;

    private void Start()
    {
        if (_inventoryManager != null)
            _inventoryManager.TriggerInventoryUpdate();

        // 레벨업 이벤트 구독
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnLevelChanged += HandleLevelUp;
        }
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnLevelChanged -= HandleLevelUp;
        }
    }

    private void HandleLevelUp(int newLevel)
    {
        if (_cardSelectionPopup != null)
        {
            _cardSelectionPopup.Show(() => {
                Debug.Log($"[UIManager] Level Up Selection Complete for Level {newLevel}");
            });
        }
    }

    public void ToggleStatPopup()
    {
        if (_statPopup == null) return;
        
        if (_statPopup.gameObject.activeSelf)
            _statPopup.Close();
        else
            _statPopup.Open();
    }

    /// <summary>
    /// UI 오브젝트가 보이는 상태면 숨기고, 숨겨진 상태면 애니메이션과 함께 표시합니다.
    /// </summary>
    public void OnDoTween(GameObject uiObject, Vector3 originalPos)
    {
        if (uiObject == null) return;

        bool isVisible = uiObject.activeSelf;

        if (!isVisible)
        {
            if (_currentActiveUI != null && _currentActiveUI != uiObject)
            {
                HideUI(_currentActiveUI);
            }

            // 전면 Z 위치로 설정
            uiObject.transform.position = new Vector3(originalPos.x, originalPos.y, _frontZ);

            float targetPositionY = GetUIScreenCenterY(uiObject);

            uiObject.SetActive(true);
            uiObject.transform.DOMoveY(targetPositionY, 0.5f).SetEase(Ease.OutCubic);
            _currentActiveUI = uiObject;
        }
        else
        {
            HideUI(uiObject);
        }
    }

    private void HideUI(GameObject uiObject)
    {
        if (uiObject == null) return;

        if (uiObject.TryGetComponent<IShowAndHide>(out var uiScript))
        {
            uiObject.transform.DOMoveY(uiScript.OriginalPosition.y, 0.5f)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    uiObject.SetActive(false);
                    // OriginalPosition을 사용하여 Z 위치 재설정
                    uiObject.transform.position = uiScript.OriginalPosition;
                });
        }
        else
        {
            uiObject.SetActive(false);
        }

        if (_currentActiveUI == uiObject)
            _currentActiveUI = null;
    }

    private float GetUIScreenCenterY(GameObject uiObject)
    {
        Canvas canvas = uiObject.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return Screen.height * 0.5f;
        }

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return Screen.height * 0.5f;
        }
        else
        {
            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
            Vector3 worldCenter = Camera.main != null ? Camera.main.ScreenToWorldPoint(screenCenter) : Vector3.zero;
            return worldCenter.y;
        }
    }
}
