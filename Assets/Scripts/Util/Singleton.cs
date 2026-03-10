using UnityEngine;

/// <summary>
/// A robust generic Singleton pattern for MonoBehaviour.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _isQuitting = false;

    public static T Instance
    {
        get
        {
            if (_isQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindAnyObjectByType(typeof(T));

                    if (_instance == null)
                    {
                        GameObject gameObject = new GameObject(typeof(T).Name);
                        _instance = gameObject.AddComponent<T>();
                        Debug.Log($"[Singleton] An instance of {typeof(T)} was created: {_instance.gameObject.name}");
                    }
                }
                return _instance;
            }
        }
    }

    [SerializeField] protected bool isDontDestroy = false;

    protected virtual void Awake()
    {
        if (IsDuplicates()) return;

        if (isDontDestroy)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected bool IsDuplicates()
    {
        if (_instance == null)
        {
            _instance = this as T;
            return false;
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T)} found on {gameObject.name}. Destroying duplicate.");
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}
