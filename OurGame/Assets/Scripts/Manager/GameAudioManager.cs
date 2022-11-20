using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioName
{
    //bgm
    GameBgm,

}


public class GameAudioManager : BaseMonoManager<GameAudioManager>
{
    public static float bgmFadeInTime = 3.0f;
    public float maxSfxVolume;
    private string bgmName;
    private bool isSoundMute = false;
    private AudioSource bgm;
    private AudioSource fadingOutBgm;
    private AudioSource sfx;

    private Dictionary<AudioName, string> audioNameDictionary = new Dictionary<AudioName, string>();
    private Dictionary<string, string> audioName2 = new Dictionary<string, string>();
    private Dictionary<string, AudioClip> audioClipList = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioSource> loopSfxList = new Dictionary<string, AudioSource>();

    public bool IsSoundMute => isSoundMute;
    public string[] AudioNames =
    {
        "GameBgm",
        "Visit",
        "ResourseFlyToUI_B",
        "UIWindowOpen",
        "UIWindowClose",
        "TouristGate",
        "ClickUI",
        "MergeWanNengWuJian",
        "3to1",
        "5to2",
        "Merge999",
        "ClickItem",
        "WorkBegin",
        "Fog_Clear",
        "PickUpItem",
        "PickPuGold",
        "GetItemFromUIBubble",
        "RunRunRun",
        "OpenBox",
        "LevelUp",
        "item_gogogo",
        "Feed_Throw",
        "Zone_Building",
        "Zone_LevelUp",
        "Zone_MaxLevel",
        "MissionGet",
        "MissionComplate",
        "PickPuGold",
        "ZooGetBoomBoomBoom",
        "BubbleBreak",
        "ClickItem",
        "Letter",
        "ResourseFlyToUI_A",
        "Photo_Tape",
    };
    void Awake()
    {
        gameObject.AddComponent<AudioListener>();
        sfx = gameObject.AddComponent<AudioSource>();
        bgm = gameObject.AddComponent<AudioSource>();
        fadingOutBgm = gameObject.AddComponent<AudioSource>();
        foreach (AudioName item in Enum.GetValues(typeof(AudioName)))
        {
            var name = AudioNames[item.GetHashCode()];
            audioNameDictionary.Add(item, name);
            audioName2.Add(Enum.GetName(typeof(AudioName), item), name);
        }
    }

    void Start()
    {
        UpdateSetting();
    }

    public AudioSource PlayLoopSfx(string name, float volume = 1.0f)
    {
        if (!loopSfxList.TryGetValue(name, out var ret))
        {
            var o = new GameObject("[AudioSource]");
            o.transform.SetParent(transform);
            ret = o.AddComponent<AudioSource>();
            ret.clip = GetAudioClip(name);
            loopSfxList.Add(name, ret);
        }
        ret.volume = volume;
        ret.Play();
        return ret;
    }

    public AudioSource PlayLoopSfx(AudioName ename)
    {
        return PlayLoopSfx(audioNameDictionary[ename]);
    }

    public void StopLoopSfx(AudioName ename)
    {
        var name = audioNameDictionary[ename];
        if (loopSfxList.TryGetValue(name, out var ret))
        {
            ret.Stop();
        }
    }
    private bool UpdateSetting()
    {
        bool ret = false;
        if (GameSettings.PlayerPrefsGetBool(Config.K_SettingConfigString))
        {
            isSoundMute = !GameSettings.PlayerPrefsGetBool(Config.K_SettingConfigString);
            if (bgm != null)
            {
                bgm.mute = !GameSettings.PlayerPrefsGetBool(Config.K_EffectSwitchString);
                ret = bgm.mute;
            }
        }
        return ret;
    }


    public void SetSoundSwitch(bool soundSwitch)
    {
        sfx.mute = !soundSwitch;
        foreach (var audio in loopSfxList)
        {
            audio.Value.mute = !soundSwitch;
        }
        isSoundMute = !soundSwitch;
    }

    public void SetEffectSwitch(bool effectSwitch)
    {
        bgm.mute = !effectSwitch;
    }

    public void PlaySingleAudio(AudioName audioName)
    {
        StartPlayAudio(audioNameDictionary[audioName]);
    }

    public void PlaySingleAudio(string audioName)
    {
        if (IsSoundMute || string.IsNullOrEmpty(audioName))
        {
            return;
        }
        if(audioName2.ContainsKey(audioName))
            audioName = audioName2[audioName];
        StartPlayAudio(audioName);
    }

    private void StartPlayAudio(string audioName)
    {
        sfx.PlayOneShot(GetAudioClip(audioName));
    }

    AudioClip GetAudioClip(string audioName)
    {
        if (!audioClipList.TryGetValue(audioName, out var ret))
        {
            ret = AssetBundleManager.Instance.LoadAsset<AudioClip>(audioName);
            audioClipList.Add(audioName, ret);
        }
        return ret;
    }

    private IEnumerator XFade(float duration, AudioSource a1, AudioSource a2, Action runOnEndFunction)
    {
        var startTime = Time.realtimeSinceStartup;
        var endTime = startTime + duration;
        float a1StartVolume = a1.volume, a2StartVolume = a2.volume;
        a2.volume = 0;

        while (Time.realtimeSinceStartup < endTime)
        {
            var deltaPercent = ((Time.realtimeSinceStartup - startTime) / duration) * 2 - 1;
            if (deltaPercent < 0)
            {
                a1.volume = Mathf.Lerp(a1StartVolume, 0, Mathf.Clamp01(1 + deltaPercent));
            }
            else
            {
                a2.volume = Mathf.Lerp(0, a2StartVolume, Mathf.Clamp01(deltaPercent));
            }
            yield return null;
        }
        a2.volume = a2StartVolume;
        a1.Stop();
        if (runOnEndFunction != null)
        {
            runOnEndFunction();
        }
    }
}