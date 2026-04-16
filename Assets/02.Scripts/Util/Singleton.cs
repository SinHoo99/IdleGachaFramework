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
                // 에디터 모드가 아닐 때만 경고 출력 (선택 사항)
                if (Application.isPlaying)
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (_instance == null && Application.isPlaying) // 게임 실행 중일 때만 새로 생성
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

    /// <summary>
    /// 인스턴스가 존재하는지 확인합니다. 경고를 발생시키지 않습니다.
    /// </summary>
    public static bool HasInstance => _instance != null;

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
