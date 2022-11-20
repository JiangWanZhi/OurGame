
using UnityEngine;
using Object = UnityEngine.Object;


public abstract class BasePanel : CoreObject
{
    public GameObject root;
    public abstract WinManager.EWinSortingGroup SortingGroup { get; }
    public abstract string Prefab { get; }
    public void InitPanel() { __OnInitUI(); OnRegister(true); OnInitUI(); }
    protected virtual void __OnInitUI() { }
    protected virtual void OnInitUI() { }
    protected virtual void OnRegister(bool register) { }
    protected override void OnRecycle() { OnRegister(false); Object.Destroy(root); }

    public int GetSortingGroupOrder()
    {
        return (int)SortingGroup * 10;
    }

}

public abstract class BaseWin : BasePanel
{
    public bool IsOpen;
    public bool Opening;
    public bool Closing;
    public MessageBoxOpenAudioPlayable openSfx;
    public MessageBoxCloseAudioPlayable closeSfx;
    public Animator anim;

    public bool OverrideChildrenSorting = true;

    public void StartWin()
    {
        openSfx = root.GetComponent<MessageBoxOpenAudioPlayable>();
        closeSfx = root.GetComponent<MessageBoxCloseAudioPlayable>();
        anim = root.GetComponent<Animator>();
        OnStartWin();
    }

    public void UpdateWin()
    {
        if (Opening && IsInAnim("MainMenu_KeepOpen"))
        {
            Opening = false;
        }

        if (Closing && IsInAnim("MainMenu_KeepClose"))
        {
            Closing = false;
            CloseWin();
        }
    }

    bool EnableAnim()
    {
        return anim != null && anim.isActiveAndEnabled;
    }

    public void Open()
    {
        LogUtility.Info($"basewin open: {Prefab}");
        if (!IsOpen)
        {
            IsOpen = true;
            WinManager.Instance.AddOpenWin(this);
            
        }

        openSfx?.Play();
        root.SetActive(true);
        Opening = true;
        Closing = false;
        if (EnableAnim())
        {
            anim.Play("Open");
        }
        OnOpen();
        root.transform.SetAsLastSibling();
    }

    public void Close()
    {
        LogUtility.Info($"basewin Close: {Prefab}");
        if (!Closing)
        {
            Closing = true;
            Opening = false;
            closeSfx?.Play();
            if (EnableAnim())
            {
                anim.Play("Close");
            }
        }
    }

    void CloseWin()
    {
        LogUtility.Info($"basewin CloseWin: {Prefab}");
        IsOpen = false;
        root.SetActive(false);
        WinManager.Instance.RemoveOpenWin(this);
        OnClose();
        WinManager.Instance.DoOepnWaitINterface();
    }

    public bool IsInAnim(string name)
    {
        if (EnableAnim())
        {
            var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(name);
        }
        return true;
    }

    protected virtual void OnStartWin() { }
    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }
}
