using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

public class GameSettings : BaseMonoManager<GameSettings>
{
    public const string XOR_KEY = "woeifh98823skdf98h98h23r98y9hkajhdfujasg7ylnzjhdbu7409";
    private const string DES_KEY_STR = "aabbccdd";
    private const string DES_IV_STR = "eeffgghh";

    public static readonly byte[] DES_KEY = Encoding.UTF8.GetBytes(DES_KEY_STR);
    public static readonly byte[] DES_IV = Encoding.UTF8.GetBytes(DES_IV_STR);

    public bool IsDebug
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }

    public Action<bool> OnApplicationPauseCallback;
    public Action OnApplicationQuitCallback;
    public Action OnApplicationEsc;

    void Awake()
    {
        Apply();
    }

    void Apply()
    {
        LogUtility.Info($"++++++++++++++++++ {Screen.currentResolution}");
        Application.targetFrameRate = 30;
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        // range
        UnityEngine.Random.InitState(DateTime.UtcNow.Millisecond);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnApplicationEsc?.Invoke();
        }
    }

    void OnApplicationPause(bool pause)
    {
        OnApplicationPauseCallback?.Invoke(pause);
    }

    void OnApplicationQuit()
    {
        OnApplicationQuitCallback?.Invoke();
    }

    static public void PlayerPrefsSetBool(string name, bool value)
    {
        PlayerPrefs.SetInt(name, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool PlayerPrefsGetBool(string name)
    {
        return PlayerPrefs.GetInt(name) != 0;
    }
    public static bool AudioGetBool(string name)
    {
        if (PlayerPrefs.GetInt(name, -1) == -1)
            return true;
        return PlayerPrefs.GetInt(name) != 0;
    }
    public static void Reset()
    {
        LogUtility.Info("game reset ......");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static string m_PersistentDataPath;

    public static string PersistentDataPath
    {
        get
        {
            if (string.IsNullOrEmpty(m_PersistentDataPath))
            {
#if UNITY_EDITOR
                m_PersistentDataPath = Path.Combine(Application.dataPath, "../save_data/merge_zoo");
#else
                m_PersistentDataPath = Application.persistentDataPath;
#endif
            }
            return m_PersistentDataPath;
        }
    }
}
