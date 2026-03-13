using UnityEngine;

public class SellingSystem : MonoBehaviour
{
    public void OnQuitButtonPressed()
    {
        // 각 데이터 매니저의 싱글톤 인스턴스를 직접 사용하여 데이터를 저장합니다.
        PlayerDataManager.Instance?.SavePlayerData();
        BossDataManager.Instance?.SaveBossRuntimeData();
        SoundManager.Instance?.SaveOptionData();

        // Unity 에디터에서 재생 모드를 중지하거나 실제 빌드에서는 애플리케이션을 종료합니다.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
