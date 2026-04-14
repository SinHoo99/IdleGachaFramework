using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DG.Tweening;

public class UI_CardSelectionPopup : UI_Base
{
    [Header("References")]
    [SerializeField] private UI_Card _cardPrefab;      
    [SerializeField] private Transform _cardContainer; 
    [SerializeField] private CanvasGroup _canvasGroup; 
    
    [Header("Settings")]
    [SerializeField] private int _selectionCount = 3;  
    
    private Action _onSelectionComplete;

    public void Show(Action onComplete)
    {
        Debug.Log("[UI_CardSelectionPopup] Show() called.");
        _onSelectionComplete = onComplete;
        
        if (_canvasGroup != null) _canvasGroup.alpha = 0;
        gameObject.SetActive(true);
        
        _canvasGroup?.DOFade(1f, 0.3f).SetUpdate(true);
        
        Time.timeScale = 0f; 
        RandomizeCards();
    }

    private void RandomizeCards()
    {
        foreach (Transform child in _cardContainer)
        {
            Destroy(child.gameObject);
        }

        // DataManager에서 로드된 카드 데이터 사용
        var allCardDatas = DataManager.Instance.AllCardDatas;

        if (allCardDatas == null || allCardDatas.Count < _selectionCount)
        {
            Debug.LogError($"[UI_CardSelectionPopup] Not enough cards in DataManager! Have: {allCardDatas?.Count ?? 0}");
            return;
        }

        var selectedCards = allCardDatas.OrderBy(x => UnityEngine.Random.value).Take(_selectionCount).ToList();

        for (int i = 0; i < selectedCards.Count; i++)
        {
            UI_Card newCard = Instantiate(_cardPrefab, _cardContainer);
            newCard.Setup(selectedCards[i], OnCardSelected);
            
            newCard.transform.localScale = Vector3.zero;
            float delay = i * 0.15f;
            
            newCard.transform.DOScale(1f, 0.5f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }

    private void OnCardSelected(CardData data)
    {
        // PlayerDataManager에게 효과 적용 위임
        PlayerDataManager.Instance.ApplyCardEffect(data);
        
        gameObject.SetActive(false);
        Time.timeScale = 1f; 
        _onSelectionComplete?.Invoke();
    }
}
