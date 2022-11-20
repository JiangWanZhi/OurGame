using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemFlyManager : BaseMonoManager<UIItemFlyManager>
{
    public const string EndFly = "EndFly";
    public const string HuiZhang = "ICON_ZLHD01_HuiZhang";
    public class FlyItem
    {
        public string EventName = EndFly;
        public bool Active;
        public float DelayTime;
        public float LifeTime;
        public float TotalTime = 1f;
        public TargetType ItemType;
        public Transform Trans;
        public Image Icon;
        public string IconName;
        public Text Text;
        public int Index;
        public List<Vector3> Curve = new List<Vector3>();
        public AnimationCurve AnimCurve;

        private GameObject particle;

        public void Update()
        {
            if (Active == false || Trans == null || AnimCurve == null) { return; }

            float p = Mathf.Clamp01(LifeTime / TotalTime);
            Trans.position = Bezier(AnimCurve.Evaluate(p));
            if (Math.Abs(p - 1) < 0.01f)
            {
                Active = false;
                Blackboard.value_target_type = ItemType;
                Blackboard.value_int = Index;
                Blackboard.value_string = IconName;
                EventDispatcher.Dispatch(EventName);
                Trans.position = Vector3.zero;
                Trans.gameObject.SetActive(false);
                return;
            }
            if (DelayTime > 0)
            {
                DelayTime -= Time.deltaTime;
            }
            else
            {
                GetParticle();

                if (particle != null && !particle.activeSelf)
                    particle.SetActive(true);
                if (!Trans.gameObject.activeSelf)
                    Trans.gameObject.SetActive(true);
                LifeTime += Time.deltaTime * 2f;//GameConfig.FlyIconSpeed;
            }
        }

        public Vector3 Bezier(float t, int startIndex)
        {
            if (Curve.Count < 3)
                return Curve[0];
            return Vector3.Lerp(Vector3.Lerp(Curve[startIndex], Curve[startIndex + 1], t), Vector3.Lerp(Curve[startIndex + 1], Curve[startIndex + 2], t), t);
        }

        public Vector3 Bezier(float t)
        {
            int segCount = (Curve.Count - 1) / 2;
            float segProgress = t * segCount;
            int segIndex = (int)(segProgress);
            if (t < 1)
            {
                return Bezier(segProgress - segIndex, segIndex * 2);
            }
            else
            {
                return Curve[Curve.Count - 1];
            }
        }


        private void GetParticle()
        {
            if (particle != null || Trans == null) { return; }

            particle = Trans.Find("Content/UI_Mission_Trail")?.gameObject;
        }
    }

    public enum TargetType
    {
        Diamond,//绿钞
        Gold,//金币
        Exp,//经验
        HealtyValue,//钥匙
        Heart,//体力
        MailRewardShoppingBox, // 邮件领取奖励气球
        Handbook,//图鉴
        Photo,//照相机点
        OneMoreShoppingBox,//大游艇没位置气泡
        FlyMission,//飞解锁任务位置
        AdBubbleShoppingBox, // 广告气泡领取奖励气球
        FavourPoint, //好友邮件好感度
        GiftboxShoppingBox1, //礼包界面1气球
        GiftboxShoppingBox2, //礼包界面2气球
        GiftboxShoppingBox3, //礼包界面3气球
        WarActivityIcon,//战令
        Material,//食材
        DailyMissionRewardBalloon, // 每日任务大奖气球
        TransferItem,//物件转移
        RewardBoxIcon,//副本奖励箱

        //未添加点
        Custom,

        //副本
        Replica,//副本入口
        AnimalToSuitcase, //动物到行李箱

    }


    public Camera mainCamera;
    public Camera uiCamera => WinManager.Instance.UICamera;
    public Transform flyItemParent;
    public GameObject genericPrefab;
    public GameObject MainMissionFlyItemPrefab;
    public GameObject photoPrefab;
    public Action<TargetType, double> ItemReachTarget;
    private Dictionary<TargetType, Transform> targetTransformDic = new Dictionary<TargetType, Transform>();
    List<FlyItem> allItems = new List<FlyItem>();


    private GameObject _flyItemWin;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        GetFlyItemWin();
    }

    void Start()
    {
        genericPrefab = AssetBundleManager.Instance.LoadPrefab("FlyItem");
        MainMissionFlyItemPrefab = AssetBundleManager.Instance.LoadPrefab("MainMissionFlyItem");
        photoPrefab = AssetBundleManager.Instance.LoadPrefab("FlyPhoto");
    }

    void Update()
    {
        foreach (var item in allItems)
        {
            if (item.Active)
            {
                item.Update();
            }
        }
    }

    public void AddTarget(TargetType t, Transform transform)
    {
        if (targetTransformDic.ContainsKey(t))
        {
            targetTransformDic[t] = transform;
        }
        else
        {
            targetTransformDic.Add(t, transform);
        }
    }

    public Transform GetTarget(TargetType targetType)
    {
        targetTransformDic.TryGetValue(targetType, out Transform transform);

        return transform;
    }

    public void OnItemReachTarget(TargetType targetType, double delta = 0)
    {
        if (ItemReachTarget != null)
        {
            ItemReachTarget.Invoke(targetType, delta);
        }
    }

    private Vector3 WorldPosToUIPos(Vector3 startPosition)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        startPosition = mainCamera.WorldToScreenPoint(startPosition);
        startPosition = uiCamera.ScreenToWorldPoint(startPosition);
        return startPosition;
    }

    public void DoItemFlyWorldPos(Vector3 startPosition, TargetType targetType, string iconName, int count = 1, float? delay = null)
    {
        startPosition = WorldPosToUIPos(startPosition);
        DoItemFly(startPosition, targetType, iconName, count, delay);
    }

    public void DoItemFlyWorldPos(Vector3 startPosition, Vector3 endUIWorldPosition, string iconName, TargetType type, int count = 1)
    {
        startPosition = WorldPosToUIPos(startPosition);
        StartDoItemFly(startPosition, endUIWorldPosition, iconName, count, type);
    }

    FlyItem GetItem(Sprite sprite, TargetType itemType, int index)
    {
        FlyItem ret = null;
        foreach (var item in allItems)
        {
            if (!item.Active)
            {
                if (itemType == item.ItemType)
                {
                    ret = item;
                    break;
                }
            }
        }
        if (ret == null)
        {
            switch (itemType)
            {
                case TargetType.FlyMission:
                    {
                        var obj = Instantiate(MainMissionFlyItemPrefab, _flyItemWin.transform);
                        obj.transform.localScale = Vector3.one;
                        obj.SetActive(true);

                        var content = obj.transform.Find("Content");
                        ret = new FlyItem
                        {
                            Trans = obj.transform,
                            Icon = content.Find("Icon").GetComponent<Image>(),
                            Text = content.Find("NumberBg").Find("Text").GetComponent<Text>()
                        };
                        break;
                    }
                case TargetType.Photo:
                    {
                        var obj = Instantiate(photoPrefab, _flyItemWin.transform);
                        obj.transform.localScale = Vector3.one;
                        obj.SetActive(true);

                        ret = new FlyItem
                        {
                            Trans = obj.transform,
                            Icon = obj.GetComponentInChildren<Image>()
                        };
                        break;
                    }
                default:
                    {
                        var obj = Instantiate(genericPrefab, _flyItemWin.transform);
                        obj.transform.localScale = Vector3.one;
                        obj.SetActive(true);

                        ret = new FlyItem
                        {
                            Trans = obj.transform,
                            Icon = obj.GetComponentInChildren<Image>()
                        };
                        break;
                    }
            }

            allItems.Add(ret);
        }
        ret.Trans.SetAsLastSibling();
        ret.Trans.gameObject.SetActive(false);
        ret.Index = index;
        ret.LifeTime = 0;
        ret.DelayTime = 0;
        ret.Curve.Clear();

        //暂时
        ret.AnimCurve = null;//CustomProjectSetting.CustomProjectSettingConfig.FlyIcomCurve;
        ret.ItemType = itemType;
        ret.Icon.sprite = sprite;
        if (itemType == TargetType.FlyMission)
        {
            ret.Text.text = Blackboard.value_string;
        }
        ret.Active = true;
        return ret;
    }

    public void DoItemFly(Vector3 startPosition, TargetType targetType, string iconName, int count = 1, float? delay = null)
    {
        var targetPosition = GetTarget(targetType).position;
        StartDoItemFly(startPosition, targetPosition, iconName, count, targetType, delay: delay);
    }

    public void DoItemFly(TargetType startType, Vector3 targetPosition, string iconName, int count = 1)
    {
        var startPosition = GetTarget(startType).position;
        StartDoItemFly(startPosition, targetPosition, iconName, count, startType);
    }

    public void DoItemFly(Vector3 startPosition, Vector3 targetPosition, string iconName, string eventName, float delay = 0)
    {
        var sprite = SpriteManager.GetSprite(iconName);
        startPosition.z = 0;
        targetPosition.z = 0;
        var direction = (startPosition - targetPosition).normalized;
        var o = GetItem(sprite, TargetType.Custom, 0);
        o.DelayTime = delay;
        o.EventName = eventName;
        o.TotalTime = 1;
        o.IconName = iconName;
        o.Curve.Add(startPosition);
        var offset = Quaternion.AngleAxis(GetRangeAngle(), Vector3.forward) * direction * 5;
        o.Curve.Add(startPosition + offset);
        o.Curve.Add(targetPosition);
    }

    private float TotalTimeOffset = 0.3f;
    public List<FlyItem> CurAllFlyObj = new List<FlyItem>();
    public void StartDoItemFly(Vector3 startPosition, Vector3 targetPosition, string iconName, int count, TargetType targetType, float time = 1f, float? delay = null)
    {
        CurAllFlyObj.Clear();
        count = 5;//count > GameConfig.FlyIconMaxNum ? GameConfig.FlyIconMaxNum : count;
        var sprite = SpriteManager.GetSprite(iconName);
        startPosition.z = 0;
        targetPosition.z = 0;
        var direction = (startPosition - targetPosition).normalized;
        var delayTime = delay.HasValue ? delay.Value : GetDelayTime();
        for (int i = 0; i < count; i++)
        {
            var o = GetItem(sprite, targetType, i);
            o.DelayTime = delayTime;
            o.IconName = iconName;
            if (targetType == TargetType.FlyMission)
                o.TotalTime = time;
            else
                o.TotalTime = time + TotalTimeOffset / count * i - 0.3f;
            o.Curve.Add(startPosition);
            var offset = Quaternion.AngleAxis(GetRangeAngle(), Vector3.forward) * direction * 5;
            o.Curve.Add(startPosition + offset);
            o.Curve.Add(targetPosition);
            if (targetType == TargetType.FlyMission)
            {
                var particle = o.Trans.Find("Content/UI_Mission_Trail");
                if (particle != null)
                    particle.gameObject.SetActive(false);
                o.Trans.position = o.Curve[0];
            }
            CurAllFlyObj.Add(o);
        }
    }

    public void ClearDelayTime()
    {
        foreach (var item in UIItemFlyManager.Instance.CurAllFlyObj)
            item.DelayTime = 0;
    }

    private float MaxRandomAngle = 60;
    private float MinRandomAngle = 25;
    private float GetRangeAngle()
    {
        float value = 0;
        while (true)
        {
            value = UnityEngine.Random.Range(-MaxRandomAngle, MaxRandomAngle);
            if ((value >= 0 && value >= MinRandomAngle) ||
                 (value <= 0 && value <= -MinRandomAngle))
                break;
        }
        return value;
    }

    public float FlyIconIntervalTime = 0.6f;
    public float GetDelayTime()
    {
        float maxTime = 0;
        bool isInterval = false;
        foreach (var item in allItems)
        {
            if (item.Active && item.LifeTime < 0.1f)
            {
                maxTime = item.DelayTime > maxTime ? item.DelayTime : maxTime;
                isInterval = true;
            }
        }
        return isInterval ? maxTime + FlyIconIntervalTime : maxTime;
    }



    private void GetFlyItemWin()
    {
        if (_flyItemWin == null)
        {
            _flyItemWin = WinManager.Instance.GetFlyItemCanvasRoot();
        }
    }
}

