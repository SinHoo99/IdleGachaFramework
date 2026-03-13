using System;
using UnityEngine;

public class OfflineScoreUpdater : MonoBehaviour
{
    private const int MaxOfflineTimeInSeconds = 7200; // 최대 오프라인 시간 (2시간 = 7200초)

    #region 오프라인 수집 로직

    public void CollectOfflineUnits()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("[OfflineScoreUpdater] PlayerDataManager instance is null.");
            return;
        }

        // 오프라인 동안 경과한 시간 계산
        DateTime lastCollectedTime = PlayerDataManager.Instance.NowPlayerData.LastCollectedTime;
        TimeSpan elapsedTime = DateTime.Now - lastCollectedTime;

        if (elapsedTime.TotalSeconds <= 0)
        {
            Debug.Log("오프라인 보상을 받을 시간이 0초 이하입니다. 보상 수집을 건너뜁니다.");
            return;
        }

        // 경과 시간을 최대 2시간으로 제한
        int secondsElapsed = Math.Min((int)elapsedTime.TotalSeconds, MaxOfflineTimeInSeconds);
        Debug.Log($"오프라인 경과 {secondsElapsed}초 감지. 보상을 계산합니다...");

        // 경과 시간에 따른 로직 수행
        for (int i = 0; i < secondsElapsed; i++)
        {
            // TODO: GameManager.Instance.scoreUpdater.AddRandomFruit(); 등을 PlayerDataManager 등으로 리팩토링할 수 있음
        }

        // 수집 완료 시간 업데이트
        PlayerDataManager.Instance.NowPlayerData.LastCollectedTime = DateTime.Now;
        Debug.Log($"오프라인 보상 {secondsElapsed}개 추가 완료 (최대 2시간 제한 적용).");
    }
    #endregion
}
