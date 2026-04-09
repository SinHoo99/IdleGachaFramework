using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using DG.Tweening; // DOTween 추가

public class UI_CardSelectionPopup : UI_Base
{
    [Header("References")]
    [SerializeField] private UI_Card _cardPrefab;      
    [SerializeField] private Transform _cardContainer; 
    [SerializeField] private CanvasGroup _canvasGroup; // 팝업 전체 투명도 조절용
    
    [Header("Settings")]
    [SerializeField] private int _selectionCount = 3;  
    
    private Action _onSelectionComplete;

    public void Show(Action onComplete)
    {
        Debug.Log("[UI_CardSelectionPopup] Show() called.");
        _onSelectionComplete = onComplete;
        
        // 팝업 초기화 (투명하게)
        if (_canvasGroup != null) _canvasGroup.alpha = 0;
        gameObject.SetActive(true);
        
        // 팝업 페이드 인 연출
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

        if (_allCards == null || _allCards.Count == 0)
        {
            _allCards = Resources.LoadAll<CardData>("Cards").ToList();
        }

        var selectedCards = _allCards.OrderBy(x => UnityEngine.Random.value).Take(_selectionCount).ToList();

        for (int i = 0; i < selectedCards.Count; i++)
        {
            UI_Card newCard = Instantiate(_cardPrefab, _cardContainer);
            newCard.Setup(selectedCards[i], OnCardSelected);
            
            // --- 연출 추가 ---
            newCard.transform.localScale = Vector3.zero; // 0에서 시작
            float delay = i * 0.15f; // 0.15초 간격으로 순차 등장
            
            newCard.transform.DOScale(1f, 0.5f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack) // 통 튀는 느낌
                .SetUpdate(true);      // 일시정지 무시
        }
    }

    private void OnCardSelected(CardData data)
    {
        ApplyCardEffect(data);
        gameObject.SetActive(false);
        Time.timeScale = 1f; 
        _onSelectionComplete?.Invoke();
    }

    private void ApplyCardEffect(CardData data)
    {
        var playerData = PlayerDataManager.Instance.NowPlayerData;
        
        switch (data.effectType)
        {
            case CardEffectType.Damage:
                playerData.Damage += data.value;
                break;
            case CardEffectType.MaxHP:
                playerData.MaxHP += data.value;
                // 현재 체력도 증가시켜주는 것이 일반적
                break;
            case CardEffectType.AttackRange:
                playerData.AttackRange += data.value;
                break;
            case CardEffectType.AttackSpeed:
                // PlayerData에 AttackSpeed가 아직 없으므로 나중에 추가 고려
                break;
        }

        Debug.Log($"<color=cyan>[CardSystem] Applied Card: {data.cardName} (+{data.value})</color>");
        PlayerDataManager.Instance.SavePlayerData();
    }
}
