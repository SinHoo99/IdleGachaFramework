using UnityEngine;

/// <summary>
/// Base class for objects managed by ObjectPool.
/// </summary>
public class PoolObject : MonoBehaviour
{
    /// <summary>
    /// Returns a specific component of the pool object.
    /// </summary>
    public T ReturnMyComponent<T>() where T : Component
    {
        if (TryGetComponent<T>(out var component))
        {
            return component;
        }

        Debug.LogWarning($"[PoolObject] Component {typeof(T).Name} not found on {gameObject.name}.");
        return null;
    }

    /// <summary>
    /// Optional virtual method called when the object is returned to the pool.
    /// </summary>
    public virtual void OnReturnToPool()
    {
        // Reset basic state if needed
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
