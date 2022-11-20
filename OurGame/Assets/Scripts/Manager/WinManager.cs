
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;


public class WinManager : BaseMonoManager<WinManager>
{
    public enum EWinSortingGroup
    {
        Scene,
        MainMenu,
        Normal,
        PopUp,
        Tutorial,
        Top,
        Count
    }

    public const string WinChange_Event = "WinChange_Event";
    public Camera UICamera { get; private set; }
    public bool OnlyMainMenuAndTutorialExtra { get; private set; }
    public event Action onOpenWinListChange;
    public string LastOpenMessagebox
    {
        get
        {
            for (int i = (int)EWinSortingGroup.PopUp; i >= (int)EWinSortingGroup.Normal; i--)
            {
                var e = (EWinSortingGroup)i;
                if (OpenWin[e].Count != 0)
                {
                    return OpenWin[e].LastObject().Prefab;
                }
            }
            return string.Empty;
        }
    }
    public bool OnlyConstOrEmptyMessageBox
    {
        get
        {
            var s = LastOpenMessagebox;
            return string.IsNullOrEmpty(s) || s == "Main" || s == "MainMenu";
        }
    }

    public Dictionary<EWinSortingGroup, List<BaseWin>> OpenWin { get; private set; } = new Dictionary<EWinSortingGroup, List<BaseWin>>();
    public Dictionary<EWinSortingGroup, List<BaseWin>> toOpenList = new Dictionary<EWinSortingGroup, List<BaseWin>>();
    private bool updating = false;
    private Dictionary<string, bool> inputModifier = new Dictionary<string, bool>();
    private Dictionary<EWinSortingGroup, Transform> sortingRoot = new Dictionary<EWinSortingGroup, Transform>();
    private List<Type> toAdd = new List<Type>();
    private List<Type> toDestroy = new List<Type>();
    private Dictionary<Type, BasePanel> panelList = new Dictionary<Type, BasePanel>();
    private Dictionary<Type, BaseWin> winList = new Dictionary<Type, BaseWin>();
    private Dictionary<Type, BaseWin> winUpdateList = new Dictionary<Type, BaseWin>();
    private Transform root;
    public EventSystem eventSystem;

    private GameObject _toastCanvas;
    private GameObject _infoCanvas;
    private GameObject _flyItemCanvas;
    private GameObject _pendingCanvas;
    private GameObject _viewPoolRoot;
    private GameObject _tileColorEftRoot;

    protected override void OnInitialize()
    {
        var p = AssetBundleManager.Instance.LoadPrefab("UIROOT");
        root = GameObject.Instantiate(p).transform;
        UICamera = root.GetComponent<Camera>();
        DontDestroyOnLoad(root);
        var c = root.Find("Canvas");
        var t = c.Find("Template").gameObject;
        for (int i = 0; i < (int)EWinSortingGroup.Count; i++)
        {
            var e = (EWinSortingGroup)i;
            var o = GameObject.Instantiate(t, c);
            o.SetActive(true);
            o.name = e.ToString();
            sortingRoot.Add(e, o.transform);

            OpenWin.Add(e, new List<BaseWin>());
            toOpenList.Add(e, new List<BaseWin>());
        }
        var Event = GameObject.Find("EventSystem");
        DontDestroyOnLoad(Event);
        eventSystem = Event.GetComponent<EventSystem>();

        var w = 1800f;
        var h = 1080f;
        var s = c.GetComponent<UnityEngine.UI.CanvasScaler>();
        var f = w / h;
        var f1 = Screen.width / (float)Screen.height;
        s.referenceResolution = new Vector2(w, h) * Mathf.Max(f / f1, 1);
    }

    public T ShowPanel<T>() where T : BasePanel, new()
    {
        var ret = GetPanel<T>();
        ret.root.SetActive(true);
        return ret;
    }
    private void OnDisable()
    {
        if (_toastCanvas != null)
        {
            DestroyImmediate(_toastCanvas);
            _toastCanvas = null;
        }
        if (_infoCanvas != null)
        {
            DestroyImmediate(_infoCanvas);
            _infoCanvas = null;
        }
        if (_flyItemCanvas != null)
        {
            DestroyImmediate(_flyItemCanvas);
            _flyItemCanvas = null;
        }
        if (_pendingCanvas != null)
        {
            DestroyImmediate(_pendingCanvas);
            _pendingCanvas = null;
        }
        if (_tileColorEftRoot != null)
        {
            DestroyImmediate(_tileColorEftRoot);
            _tileColorEftRoot = null;
        }
    }


    public T HidePanel<T>() where T : BasePanel, new()
    {
        var ret = GetPanel<T>();
        ret.root.SetActive(false);
        return ret;
    }

    public T GetPanel<T>() where T : BasePanel, new()
    {
        var t = typeof(T);
        if (!panelList.TryGetValue(t, out var ret))
        {
            LogUtility.Info($"WinManager GetPanel {t}");
            ret = CoreObjectManager.Instance.Request<T>();
            panelList.Add(t, ret);
            var p = AssetBundleManager.Instance.LoadPrefab(ret.Prefab);
            var o = GameObject.Instantiate(p, sortingRoot[ret.SortingGroup]);
            o.name = ret.Prefab;
            o.transform.localScale = Vector3.one;
            ret.root = o;
            var canvas = o.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = ret.GetSortingGroupOrder();
            o.SetActive(false);
            ret.InitPanel();
        }
        return ret as T;
    }

    public T Get<T>() where T : BaseWin, new()
    {
        var t = typeof(T);
        if (!winList.TryGetValue(t, out var ret))
        {
            LogUtility.Info($"WinManager Get {t}");
            ret = GetPanel<T>();
            winList.Add(t, ret);
            if (updating)
                toAdd.Add(t);
            else
                winUpdateList.Add(t, ret);
            ret.StartWin();
        }
        return ret as T;
    }

    public T Open<T>(bool front = false, bool isClickButton = false) where T : BaseWin, new()
    {
        var ret = Get<T>();
        if (!isClickButton && ret.SortingGroup == EWinSortingGroup.Normal && OpenWin[EWinSortingGroup.Tutorial].Count > 0)
        {
            if (WaitTutorialInterfaceList.Contains(ret))
                return ret;
            if (front)
                WaitTutorialInterfaceList.Insert(0, ret);
            else
                WaitTutorialInterfaceList.Add(ret);
            return ret;
        }
        var toOpen = toOpenList[ret.SortingGroup];
        if (ret.IsOpen || toOpen.Contains(ret))
            return ret;

        if (front)
            toOpen.Insert(0, ret);
        else
            toOpen.Add(ret);
        return ret;
    }

    public T Pop<T>() where T : BaseWin, new()
    {
        var ret = Get<T>();
        ret.Open();
        return ret;
    }

    public void AddOpenWin(BaseWin w)
    {
        OpenWin[w.SortingGroup].Add(w);
        int count = w.GetSortingGroupOrder();
        var winList = OpenWin[w.SortingGroup];
        for (int i = 0; i < winList.Count; i++)
        {
            int sortingOrder = winList[i].root.GetComponent<Canvas>().sortingOrder;
            if (count < sortingOrder)
                count = sortingOrder;
        }
        var c = w.root.GetComponent<Canvas>();
        if (c != null)
        {
            c.sortingOrder = count + 1;
            if (w.OverrideChildrenSorting)
            {
                var renderers = w.root.GetComponentsInChildren<Renderer>();
                foreach (var item in renderers)
                {
                    item.sortingOrder = c.sortingOrder;
                }
            }
        }

        RefreshOnlyMainMenuAndTutorialExtra();
    }

    public void RemoveOpenWin(BaseWin w)
    {
        var open = OpenWin[w.SortingGroup];
        for (int i = 0; i < open.Count; i++)
        {
            if (open[i] == w)
            {
                open.RemoveAt(i);
                RefreshOnlyMainMenuAndTutorialExtra();
                return;
            }
        }
        var toOpen = toOpenList[w.SortingGroup];
        for (int i = 0; i < toOpen.Count; i++)
        {
            if (toOpen[i] == w)
            {
                toOpen.RemoveAt(i);
                break;
            }
        }
    }

    /// <summary>
    /// 获取Toast根节点
    /// </summary>
    /// <returns></returns>
    public GameObject GetToastCanvasRoot()
    {
        if (_toastCanvas != null) return _toastCanvas;

        _toastCanvas = Instantiate(AssetBundleManager.Instance.LoadPrefab("ToastCanvas"), sortingRoot[EWinSortingGroup.Top]);
        var canvas = _toastCanvas.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 997;
        return _toastCanvas;
    }

    /// <summary>
    /// 获取InfoWin根节点
    /// </summary>
    /// <returns></returns>
    public GameObject GetInfoCanvasRoot()
    {
        if (_infoCanvas != null) return _infoCanvas;

        _infoCanvas = Instantiate(AssetBundleManager.Instance.LoadPrefab("InfoWin"), sortingRoot[EWinSortingGroup.Scene]);
        _infoCanvas.transform.localScale = Vector3.one;
        return _infoCanvas;
    }

    /// <summary>
    /// 获取物品飞根节点
    /// </summary>
    /// <returns></returns>
    public GameObject GetFlyItemCanvasRoot()
    {
        if (_flyItemCanvas != null) return _flyItemCanvas;

        _flyItemCanvas = Instantiate(AssetBundleManager.Instance.LoadPrefab("FlyItemCanvas"), sortingRoot[EWinSortingGroup.Top]);
        _flyItemCanvas.transform.localScale = Vector3.one;
        var canvas = _flyItemCanvas.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 998;
        return _flyItemCanvas;
    }

    /// <summary>
    /// 获取物品飞根节点
    /// </summary>
    /// <returns></returns>
    public GameObject GetPendingCanvas()
    {
        if (_pendingCanvas != null) return _pendingCanvas;

        _pendingCanvas = Instantiate(AssetBundleManager.Instance.LoadPrefab("PendingCanvas"), sortingRoot[EWinSortingGroup.Top]);
        _pendingCanvas.transform.localScale = Vector3.one;
        var canvas = _pendingCanvas.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 999;
        return _pendingCanvas;
    }

    /// <summary>
    /// 获取对象池根节点
    /// </summary>
    /// <returns></returns>
    public GameObject GetViewPoolRoot()
    {
        if (_viewPoolRoot != null) return _viewPoolRoot;

        _viewPoolRoot = Instantiate(AssetBundleManager.Instance.LoadPrefab("ViewPoolManager"), sortingRoot[EWinSortingGroup.Top].parent);
        _viewPoolRoot.transform.localScale = Vector3.one;
        return _viewPoolRoot;
    }

    /// <summary>
    /// 获取tileColor根节点
    /// </summary>
    /// <returns></returns>
    public GameObject GetTileColorEftRoot()
    {
        if (_tileColorEftRoot != null) return _tileColorEftRoot;

        _tileColorEftRoot = Instantiate(AssetBundleManager.Instance.LoadPrefab("TileColorEffectManager"), sortingRoot[EWinSortingGroup.Top].parent);
        _tileColorEftRoot.transform.localScale = Vector3.one;
        return _tileColorEftRoot;
    }



    public void Update()
    {
        for (int i = 0; i < (int)EWinSortingGroup.Count; i++)
        {
            var e = (EWinSortingGroup)i;
            if (OpenWin[e].Count == 0)
            {
                if (toOpenList[e].Count > 0)
                {
                    var w = toOpenList[e][0];
                    if (!w.IsOpen)
                    {
                        toOpenList[e].RemoveAt(0);
                        w.Open();
                    }
                    break;
                }
            }
        }
        updating = true;
        foreach (var item in winUpdateList)
        {
            item.Value.UpdateWin();
        }
        updating = false;
        foreach (var item in toAdd)
        {
            LogUtility.Info($"WinManager toAdd {item}");
            winUpdateList.Add(item, winList[item]);
        }
        toAdd.Clear();
        foreach (var item in toDestroy)
        {
            winUpdateList.Remove(item);
            LogUtility.Info($"WinManager toDestroy {item}");
            DestroyWin(winList[item]);
        }
        toDestroy.Clear();
        //物理返回会带来界面逻辑问题，且7.2版本无此功能计划，暂时屏蔽，待后续版本再加入并处理。
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    CloseLastOpenMessagebox();
        //}
    }

    public void CloseLastOpenMessagebox()
    {
        for (int i = (int)EWinSortingGroup.PopUp; i >= (int)EWinSortingGroup.Normal; i--)
        {
            var e = (EWinSortingGroup)i;
            if (OpenWin[e].Count != 0)
            {
                Close(OpenWin[e].LastObject());
            }
        }
    }

    public void CloseAll()
    {
        foreach (var item in toOpenList)
        {
            item.Value.Clear();
        }
        for (int i = (int)EWinSortingGroup.PopUp; i >= (int)EWinSortingGroup.MainMenu; i--)
        {
            var e = (EWinSortingGroup)i;
            foreach (var item in OpenWin[e])
            {
                item.Close();
            }
        }
    }

    public void Close(BaseWin w)
    {
        w.Close();
    }

    public void Close<T>() where T : BaseWin, new()
    {
        var w = Get<T>();
        var t = typeof(T);
        if (w.IsOpen)
            w.Close();
        else
        {
            foreach (var item in toOpenList)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var sitem = item.Value[i];
                    if (t == sitem.GetType())
                    {
                        item.Value.RemoveAt(i);
                        return;
                    }
                }
            }
        }
    }

    public void DestroyWin<T>() where T : BaseWin, new()
    {
        var t = typeof(T);
        if (winList.ContainsKey(t))
        {
            for (int i = 0; i < toAdd.Count; i++)
            {
                if (t == toAdd[i])
                {
                    toAdd.RemoveAt(i);
                    break;
                }
            }
            DestroyWin(Get<T>());
        }
    }

    public void DestroyWin(BaseWin w)
    {
        if (updating)
        {
            toDestroy.Add(w.GetType());
        }
        else
        {
            OpenWin[w.SortingGroup].Remove(w);
            RefreshOnlyMainMenuAndTutorialExtra();
            panelList.Remove(w.GetType());
            winList.Remove(w.GetType());
            winUpdateList.Remove(w.GetType());
            w.Destroy();
        }
    }

    public void SetInputState(string key, bool value)
    {
        if (inputModifier.ContainsKey(key))
        {
            if (inputModifier[key] == value)
            {
                return;
            }
            inputModifier[key] = value;
        }
        else
        {
            inputModifier.Add(key, value);
        }

        bool result = true;
        foreach (var item in inputModifier)
        {
            if (item.Value == false)
            {
                result = false;
                break;
            }
        }
        eventSystem.enabled = result;
    }

    private void RefreshOnlyMainMenuAndTutorialExtra()
    {
        onOpenWinListChange?.Invoke();
        EventDispatcher.Dispatch(WinChange_Event);

        //for (int i = (int)EWinSortingGroup.Normal; i <= (int)EWinSortingGroup.PopUp; i++)
        //{
        //    foreach (var item in OpenWin[(EWinSortingGroup)i])
        //    {
        //        if (item.Prefab != MainMenuStateName.TutorialExtraMessageBox)
        //        {
        //            OnlyMainMenuAndTutorialExtra = false;
        //            return;
        //        }
        //    }
        //}

        OnlyMainMenuAndTutorialExtra = true;
    }

    private List<BaseWin> WaitTutorialInterfaceList = new List<BaseWin>();
    public void DoOepnWaitINterface()
    {
        if (OpenWin[EWinSortingGroup.Tutorial].Count == 0)
        {
            foreach (var item in WaitTutorialInterfaceList)
            {
                var toOpen = toOpenList[item.SortingGroup];
                toOpen.Add(item);
            }
            WaitTutorialInterfaceList.Clear();
        }
    }
}