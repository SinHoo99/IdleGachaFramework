using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentDataManager : Singleton<EquipmentDataManager>
{
    // 등급별 데이터 정의
    private Dictionary<EquipmentGrade, GradeData> _gradeTable = new()
    {
        { EquipmentGrade.Common, new GradeData(10, 2, 100, 1.2f, 0.5f, 0.05f) },     // 기본10, +2씩, 비용100, 배율1.2, 확률50%, 실패보정5%
        { EquipmentGrade.Rare, new GradeData(50, 10, 500, 1.3f, 0.25f, 0.03f) },     // 기본50, +10씩, 비용500, 배율1.3, 확률25%, 실패보정3%
        { EquipmentGrade.Epic, new GradeData(200, 40, 2000, 1.5f, 0.1f, 0.01f) },    // 기본200, +40씩, 비용2000, 배율1.5, 확률10%, 실패보정1%
        { EquipmentGrade.Legendary, new GradeData(1000, 200, 10000, 2.0f, 0f, 0f) }  // 최종 등급
    };

    public class GradeData
    {
        public float BaseStat;
        public float StatPerLevel;
        public int BaseCost;
        public float CostMultiplier;
        public float SuccessRate;
        public float PityIncrement;

        public GradeData(float b, float s, int cost, float mult, float rate, float pity)
        {
            BaseStat = b;
            StatPerLevel = s;
            BaseCost = cost;
            CostMultiplier = mult;
            SuccessRate = rate;
            PityIncrement = pity;
        }
    }

    // 장비의 현재 능력치 계산
    public float GetStat(EquipmentInstance item)
    {
        if (!_gradeTable.TryGetValue(item.Grade, out var data)) return 0;
        return data.BaseStat + (item.Level - 1) * data.StatPerLevel;
    }

    // 강화 비용 계산
    public int GetEnhanceCost(EquipmentInstance item)
    {
        if (!_gradeTable.TryGetValue(item.Grade, out var data)) return 0;
        return Mathf.RoundToInt(data.BaseCost * Mathf.Pow(data.CostMultiplier, item.Level - 1));
    }

    // 등급업 비용 계산
    public int GetGradeUpCost(EquipmentInstance item)
    {
        if (!_gradeTable.TryGetValue(item.Grade, out var data)) return 0;
        return data.BaseCost * 10; // 강화 비용의 10배 정도로 책정
    }

    // 강화 시도
    public bool Enhance(EquipmentInstance item)
    {
        if (item.Level >= 10) return false;

        int cost = GetEnhanceCost(item);
        if (PlayerDataManager.Instance.TrySpendCoin(cost))
        {
            item.Level++;
            Debug.Log($"[Equipment] {item.Type} 강화 성공! Level: {item.Level}");
            PlayerDataManager.Instance.RefreshPlayerStats();
            return true;
        }
        return false;
    }

    // 등급업 시도
    public bool GradeUp(EquipmentInstance item)
    {
        if (item.Level < 10 || item.Grade == EquipmentGrade.Legendary) return false;

        int cost = GetGradeUpCost(item);
        if (!PlayerDataManager.Instance.TrySpendCoin(cost)) return false;

        if (!_gradeTable.TryGetValue(item.Grade, out var data)) return false;

        float currentChance = data.SuccessRate + item.PityBonus;
        float roll = UnityEngine.Random.value;

        if (roll <= currentChance)
        {
            // 성공
            item.Grade++;
            item.Level = 1;
            item.PityBonus = 0;
            Debug.Log($"<color=cyan>[Equipment] {item.Type} 등급업 성공! Grade: {item.Grade}</color>");
            PlayerDataManager.Instance.RefreshPlayerStats();
            return true;
        }
        else
        {
            // 실패 (천장 누적)
            item.PityBonus += data.PityIncrement;
            Debug.Log($"<color=red>[Equipment] {item.Type} 등급업 실패... 현재 보너스: {item.PityBonus * 100}%</color>");
            return false;
        }
    }
}
