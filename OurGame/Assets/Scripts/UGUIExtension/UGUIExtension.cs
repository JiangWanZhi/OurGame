using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class UGUIExtension
{
    public static void SetButtonClick(this GameObject self, UnityAction action)
    {
        self.GetComponent<Button>().onClick.AddListener(action);
    }
    public static void SetButtonClick(this GameObject self, string name, UnityAction action)
    {
        self.FindObject(name).GetComponent<Button>().onClick.AddListener(action);
    }

    public static void SetImage(this GameObject self, Sprite image)
    {
        self.GetComponent<Image>().sprite = image;
    }
    public static void SetText(this GameObject self, string s)
    {
        self.GetComponent<Text>().text = s;
    }

    public static GameObject CreateAssetObjInChild(this GameObject self, string name)
    {
        return GameObject.Instantiate(AssetBundleManager.Instance.LoadPrefab(name), self.transform);
    }

    private static ObjectPool<string, GameObject> CheckChildNumPool = new ObjectPool<string, GameObject>();
    public static GameObject CheckChildNum(this GameObject self, int targetLenth, string createChildName, Action<int, GameObject> foreachFun = null)
    {
        int selfLenth = self.transform.childCount;
        if (selfLenth < targetLenth)
        {
            var num = targetLenth - selfLenth;
            for (int i = 0; i < num; i++)
            {
                GameObject obj = CheckChildNumPool.Request(createChildName);
                if (!obj)
                    self.CreateAssetObjInChild(createChildName);
                else
                {
                    obj.transform.parent = self.transform;
                    obj.transform.localScale = Vector3.one;
                }
            }
        }
        else if (selfLenth > targetLenth)
        {
            var num = selfLenth - targetLenth;
            for (int i = 0; i < num; i++)
            {
                GameObject obj = self.transform.GetChild(selfLenth - 1 - i).gameObject;
                obj.transform.SetParent(null);
                CheckChildNumPool.Recycle(createChildName, obj);
            }
        }
        if (foreachFun != null)
        {
            var length = self.transform.childCount;
            for (int i = 0; i < length; i++)
                foreachFun(i, self.transform.GetChild(i).gameObject);
        }
        return self;
    }

    /// <summary>
    /// 删除所有子节点
    /// </summary>
    /// <param name="self"></param>
    public static void DestroyAllChildes(this Transform self)
    {
        for (var i = self.childCount - 1; i >= 0; i--) {
            Object.Destroy(self.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// 获取某个组件 如果没有该组件 则自动添加
    /// </summary>
    /// <param name="self"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetOrAddComponent<T>(this GameObject self) where T : Component
    {
        if (self.GetComponent<T>() == null) {
            self.AddComponent<T>();
        }
        return self.GetComponent<T>();
    }

    /// <summary>
    /// 设置ui上的浮动箭头
    /// time:-1不自动销毁
    /// </summary>
    private static string ArrowName = "newArrowPrefab";
    public static Action SetUIArrow(this GameObject self,float time, Vector3 scale,Vector3 offset,out GameObject curObj)
    {
        GameObject obj = CheckChildNumPool.Request(ArrowName);
        if (!obj)
            obj = self.CreateAssetObjInChild(ArrowName);
        else
        {
            obj.transform.parent = self.transform;
        }
        curObj = obj;
        obj.SetActive(true);
        obj.transform.localScale = scale;
        obj.transform.localPosition = Vector3.zero + offset;
        Action RecycleFun = () =>
        {
            obj.transform.SetParent(null);
            CheckChildNumPool.Recycle(ArrowName, obj);
            obj = null;
        };
        if (time != -1)
        {
            TimeService.Instance.StartDelayAction(() =>
            {
                RecycleFun();
            }, time);
            return null;
        }
        else
        {
            return RecycleFun;
        }
    }


    public static void SetRectTransformPosition(this GameObject self, Vector2 pos)
    {
        self.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    public static void SetRectTransformSize(this GameObject self, Vector2 size)
    {
        self.GetComponent<RectTransform>().sizeDelta = size;
    }

    public static GameObject FindObject(this GameObject self, string name, int num = 0)
    {
        if (self.name == name) return self;
        int lenth = self.transform.childCount;
        for (int i = 0; i < lenth; i++)
        {
            GameObject obj = self.transform.GetChild(i).gameObject.FindObject(name, num + 1);
            if (obj != null)
                return obj;
        }
        if (num == 0)
            Debug.Log($"{self.name}中FindObject没有找到：{name}");
        return null;
    }

    public static GameObject FindObject(this Transform self, string name)
    {
        return self.gameObject.FindObject(name);
    }

    public static Vector3 WorldToUIWorldPos(this GameObject obj,Vector3 world)
    {
        var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, world);
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(obj.GetComponent<RectTransform>(), screenPoint, WinManager.Instance.UICamera, out var localPoint))
            return localPoint;
        return Vector3.zero;
    }
    public static Vector3 WorldToUILocalPos(this GameObject obj, Vector3 world)
    {
        var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, world);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(obj.GetComponent<RectTransform>(), screenPoint, WinManager.Instance.UICamera, out var localPoint))
            return localPoint;
        return Vector3.zero;
    }
    public static Vector3 ScreenToUILocalPos(this GameObject obj, Vector3 screen)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(obj.GetComponent<RectTransform>(), screen, WinManager.Instance.UICamera, out var localPoint))
            return localPoint;
        return Vector3.zero;
    }

    public static Vector3 UIToWorldPos(this GameObject obj)
    {
        var screenPoint = obj.UIToScreen();
        return Camera.main.ScreenToWorldPoint(screenPoint);
    }

    public static Vector3 UIToScreen(this GameObject obj)
    {
        return RectTransformUtility.WorldToScreenPoint(UIItemFlyManager.Instance.uiCamera, obj.transform.position);
    }

    public struct PageSwitchData
    {
        public string[] ShowObjNameArray;
        public GameObject[] ShowObjArray;
    }

    public static void SetPageButton(this GameObject self, PageSwitchData[] perPageName, string[] buttonName, Action<int> onClickFun = null)
    {
        for (int i = 0; i < perPageName.Length; i++)
        {
            var item = perPageName[i];
            if (item.ShowObjArray == null)
                perPageName[i].ShowObjArray = new GameObject[item.ShowObjNameArray.Length];
            for (int h = 0; h < item.ShowObjNameArray.Length; h++)
            {
                string name = item.ShowObjNameArray[h];
                perPageName[i].ShowObjArray[h] = self.FindObject(name);
                if (perPageName[i].ShowObjArray[h] == null)
                    LogUtility.Error($"{self.name}中没有找到节点：{name}");
            }
        }
        for (int i = 0; i < buttonName.Length; i++)
        {
            Button button = self.FindObject(buttonName[i]).GetComponent<Button>();
            var ii = i;
            button.onClick.AddListener(() =>
            {
                for (int h = 0; h < perPageName.Length; h++)
                {
                    var item = perPageName[h];
                    if (h != ii)
                        for (int k = 0; k < item.ShowObjArray.Length; k++)
                            item.ShowObjArray[k].SetActive(false);
                }
                var item2 = perPageName[ii];
                for (int k = 0; k < item2.ShowObjArray.Length; k++)
                    item2.ShowObjArray[k].SetActive(true);
                onClickFun?.Invoke(i);
            });
        }
    }
}
