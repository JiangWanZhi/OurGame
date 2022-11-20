using System;
using UnityEngine;

public static class Config
{
    public const string K_EffectSwitchString = "K_EffectSwitchString";
    public const string K_SettingConfigString = "K_SettingConfigString";
    public const string K_SoundSwitchString = "K_SoundSwitchString";

    public static readonly TimeSpan HeartRefillTime = TimeSpan.FromMinutes(3);
    public static TimeSpan WizardEffectTime = TimeSpan.FromSeconds(2);
    public static readonly TimeSpan AdAccelerateTime = TimeSpan.FromSeconds(61);
    public static readonly TimeSpan AdAccelerateTimeMinus = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan ADBubbleRefreshTime = TimeSpan.FromSeconds(3);
    public static Vector2Int ExtraWokerCreatePosition = new Vector2Int(-50, 16);




    //
    // public const float HeartRefillUp = 0.25f;
    // public static readonly int NormalHeartCapacity = 5;
    // public static readonly string heart1 = nameof(heart1);
    // public const int DigMaxCount = 79;
    // public const int SupplyHeartCount = 3;
    // public const string EntityPrefabDirectory = "Entity/Gravity/";
    // public const string AnimalEntitySpineDirectory = "Spine/Animal/";
    // public const string TouristSpineDirectory = "Spine/Tourist/";
    // public const string MapDefault = "MapItem_1";
    // public const int PanOrderRefreshTime = 10;
    // public const string BubblePrefab = "Entity_Bubble";
    // public const string PutSuffix = "_Put";
    // public const string PathNotSuffix = "_PathNot";
    // public static readonly List<string> SuperPre = new List<string> { "TY_WanNengWuJian_L1", "TY_WanNengWuJian_L2", "TY_WanNengWuJian_L3" };
    // public const string SuperEntity = "TY_WanNengWuJian_L4";
    // public const string KuoJianLin_Icon = "ICON_MWJS_KuoJianLin";
    // public const string LvChao_Icon = "ICON_ZJM_LvChao";
    // public const string JinBi_Icon = "PIC_TJ_JinBi";
    // public const string TiLi_Icon = "ICON_ZJM_TiLi";
    // public const int DailyMissionLimit = 7;
    // public const int HeartTutorialValue = 15;
    // public const int WorldLArg = 1000;
    // public const string SettingConfigString = "SettingConfig";
    // public const string SoundSwitchString = "SoundSwitch";
    // public const string EffectSwitchString = "EffectSwitch";
    // public const string VibrationSwitchString = "VibrationSwitch";
    // public static bool IsSuperPre(string id)
    // {
    //     return GameConfig.SuperPre.Contains(id);
    // }
    // public const string BoadMoveRegisterKey = nameof(BoadMoveRegisterKey);
    // public const int VisitBaseGoldPlus = 200;
    // public const int StoreRefreshTimer = 6;
    // public const int ReFreshStoreCost = 99;
    // public const int TranslationID_Settings_MaintainEnded = TranslationID.Settings_MaintainEnded;
    // public const int TranslationID_Settings_HarvestEnded = TranslationID.Settings_HarvestEnded;
    // public const ConditionTutorialStage AnouncementUntilTutorialComplete = ConditionTutorialStage.OpenBox1;
    // public const int ADShopHeartLevel = 4;
    // public const int ADBubbleLevel = 6;
    // public static int FeedOrderCountN = 0;
    // public static int FeedOrderInterval = 30;
    // public static int FeedReOrderInterval = 5;
    // public static int TouristMaxTime = 45;
    // public static int MaxMonthCardRmains = 30;
    // public static int MaxWeekCardRmains = 7;
    // public static int ExtraWorkerUnlockLevel = 9;
    // public static string ExtraWorkerName = "Human_3";
    // public static string WizardWorkerName = "Wizard";
    // public static string ExtraWorkerDialogueID = "Dialogue_173";
    // public static string ExtraWorkerLeaveDialogue = "Dialogue_174";
    // public static float ReorderDiscount = 0.8f;
    // public static int HighLevelRewardPrice = 666;
    // public static int InnerAnnouncementShowLevel = 5;
    // public static int WarActivtiyTimeLimit = 0;
    // public static string WizardRemains = "TY_MoShuShi_L1";
    // public static string WizardRemainsIcon = "TY_MoShuShi_L1";
    // public static int WizardRemainsMail = 1003;
    // public const int WeekCardHeart = 30;
    // public const string HeartChangeEvent = "HeartChangeEvent";
    // public const string GooglePlayUrl = "https://play.google.com/store/apps/details?id=com.minigames.fantasymerge";
}
