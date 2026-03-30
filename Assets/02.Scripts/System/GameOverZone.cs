using UnityEngine;

public class GameOverZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Enemy))
        {
            if (StageManager.Instance != null && StageManager.Instance.CurrentState == GameState.Playing)
            {
                StageManager.Instance.FailStage();
            }
        }
    }
}
