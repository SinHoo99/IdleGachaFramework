using UnityEngine;

/// <summary>
/// Controller responsible for managing the AutoPlay logic and state.
/// This is a standard MonoBehaviour, not a singleton.
/// Attach this to the UI object that needs to control AutoPlay.
/// </summary>
public class AutoPlayController : MonoBehaviour
{
    public bool IsAutoPlay
    {
        get => StageManager.Instance != null && StageManager.Instance.IsAutoPlay;
        set
        {
            if (StageManager.Instance != null)
            {
                StageManager.Instance.IsAutoPlay = value;
                Debug.Log($"[AutoPlayController] AutoPlay is now: {value}");
                
                // If we are in Ready state and just turned on AutoPlay, start the stage.
                if (value && StageManager.Instance.CurrentState == GameState.Ready)
                {
                    StageManager.Instance.StartStage();
                }
            }
        }
    }

    /// <summary>
    /// Toggles the AutoPlay state.
    /// </summary>
    public void ToggleAutoPlay()
    {
        IsAutoPlay = !IsAutoPlay;
    }
}
