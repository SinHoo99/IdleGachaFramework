using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening; // 추가
using UnityEngine.EventSystems; // 추가

public class UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler // 인터페이스 추가
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Button _selectButton;

    private CardData _data;
    private Action<CardData> _onCardSelected;
    private Vector3 _originalScale = Vector3.one;

    public void Setup(CardData data, Action<CardData> onSelected)
    {
        // ... (기존 로그 코드 유지)
        _data = data;
        _onCardSelected = onSelected;
        
        if (_nameText != null) _nameText.text = _data.cardName;
        if (_descText != null) _descText.text = _data.GetFormattedDescription();
        if (_iconImage != null) _iconImage.sprite = _data.cardIcon;

        if (_selectButton != null)
        {
            _selectButton.onClick.RemoveAllListeners();
            _selectButton.onClick.AddListener(HandleClick);
        }
    }

    // 마우스 올렸을 때 커지는 연출
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(_originalScale * 1.1f, 0.2f).SetUpdate(true);
    }

    // 마우스 뗐을 때 원래대로
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(_originalScale, 0.2f).SetUpdate(true);
    }

    private void HandleClick()
    {
        // 클릭 시 살짝 눌리는 느낌 추가
        transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0), 0.2f).SetUpdate(true).OnComplete(() => {
            _onCardSelected?.Invoke(_data);
        });
    }
}
