using UnityEngine;

public class BaseManager<T> where T : new()
{
    private static T _Instance;
    
    public bool Initialized;

    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new T();
            }
            return _Instance;
        }
    }

    public void Initialize()
    {
        // if(Initialized){return;}
        
        OnInitialize();
        // Initialized = true;
    }
    public void Destroy() { OnDestroy(); }

    protected virtual void OnInitialize() { }
    protected virtual void OnDestroy() { }
}


public class BaseMonoManager<T> : MonoBehaviour where T : MonoBehaviour
{
    public bool Initialized;
    private static T _Instance;

    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                var Root = GameObject.Find("ManagerRoot");
                if (Root == null) {
                    Root = new GameObject("ManagerRoot");
                    DontDestroyOnLoad(Root);
                }
                var o = new GameObject($"[{typeof(T).Name}]");
                o.transform.SetParent(Root.transform);
                // DontDestroyOnLoad(o);
                _Instance = o.AddComponent<T>();
            }
            return _Instance;
        }
    }

    public void Initialize() { 
        // if(Initialized){return;}
        
        OnInitialize();
        // Initialized = true;
    }

    public void Destroy()
    {
        OnDestroy();
    }

    protected virtual void OnInitialize() { }
    protected virtual void OnDestroy() { }
}