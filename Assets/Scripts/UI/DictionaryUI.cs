using UnityEngine;

public class DictionaryUI : MonoBehaviour, IShowAndHide
{
    [SerializeField] private Vector3 _originalPosition;
    public Vector3 OriginalPosition => _originalPosition;

    private GameManager GM => GameManager.Instance;

    private void Awake()
    {
        _originalPosition = transform.position;
    }

    private void OnEnable()
    {
        if (GM != null && GM.UIManager?.InventoryManager != null)
            GM.UIManager.InventoryManager.TriggerInventoryUpdate();
    }

    public void ShowAndHide()
    {
        if (GM != null)
        {
            GM.PlaySFX(SFX.Click);
            if (GM.UIManager != null)
            {
                if (GM.UIManager.InventoryManager != null)
                    GM.UIManager.InventoryManager.TriggerInventoryUpdate();
                    
                GM.UIManager.OnDoTween(gameObject, _originalPosition);
            }
        }
    }
}
