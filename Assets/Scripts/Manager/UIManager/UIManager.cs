using DG.Tweening;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private DictionaryManager _dictionaryManager;
    
    public InventoryManager InventoryManager => _inventoryManager;
    public DictionaryManager DictionaryManager => _dictionaryManager;

    private GameObject _currentActiveUI = null;
    private float _frontZ = -5f;

    private void Start()
    {
        if (_inventoryManager != null)
            _inventoryManager.TriggerInventoryUpdate();
    }

    /// <summary>
    /// Animates a UI object into view or hides it if already visible.
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

            // Set to front Z position
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
                    // Reset Z position using OriginalPosition
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
