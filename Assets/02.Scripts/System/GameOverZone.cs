using UnityEngine;

public class GameOverZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Enemy))
        {
            if (StageManager.Instance != null && StageManager.Instance.CurrentState == GameState.Playing)
            {
                // Notify StageManager that an enemy has escaped
                StageManager.Instance.OnEnemyEscaped();
                
                // Return the enemy to the pool
                if (collision.TryGetComponent<Enemy>(out var enemy))
                {
                    ObjectPool.Instance.ReturnObject(Tag.Enemy, enemy);
                }
                else
                {
                    collision.gameObject.SetActive(false);
                }
            }
        }
    }
}
