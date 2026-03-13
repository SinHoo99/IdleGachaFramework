using UnityEngine;

public interface IUIElement
{
    void Show();
    void Hide();
}

public interface IShowAndHide
{
    Vector3 OriginalPosition { get; }
    void ShowAndHide();
}
