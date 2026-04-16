using UnityEngine;

public class UI_Base : MonoBehaviour, IShowAndHide
{
    protected Vector3 _originalPosition;
    public Vector3 OriginalPosition => _originalPosition;

    protected virtual void Awake()
    {
        _originalPosition = transform.position;
    }

    public virtual void Show()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnDoTween(gameObject, _originalPosition);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    public virtual void ShowAndHide()
    {
        if (gameObject.activeSelf) Hide();
        else Show();
    }
}
