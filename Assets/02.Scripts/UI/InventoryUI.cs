using UnityEngine;

public class InventoryUI : MonoBehaviour, IShowAndHide
{
    [SerializeField] private Vector3 _originalPosition;
    public Vector3 OriginalPosition => _originalPosition;

    private void Awake()
    {
        _originalPosition = transform.position;
    }

    private void OnEnable()
    {
     
        if (UIManager.Instance?.InventoryManager != null)
            UIManager.Instance.InventoryManager.TriggerInventoryUpdate();
    }
    private void Start()
    {
        this.gameObject.SetActive(false);
    }
    public void ShowAndHide()
    {
        // 사운드 매니저 싱글톤을 사용하여 클릭 효과음 재생
        SoundManager.Instance?.PlaySFX(SFX.Click);

        if (UIManager.Instance != null)
            UIManager.Instance.OnDoTween(gameObject, _originalPosition);
    }
}
