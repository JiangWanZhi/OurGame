using System.Collections.Generic;
using UnityEngine;
using System;


public class ParamedEventDispatcher<T>
{
    private Dictionary<string, Action<T>> mEventList = new Dictionary<string, Action<T>>();
    public void RegisterEvent(string name, Action<T> callback, bool register)
    {
        if (register)
        {
            if (!mEventList.ContainsKey(name))
                mEventList.Add(name, callback);
            else
                mEventList[name] += callback;
        }
        else {
            if(mEventList.ContainsKey(name))
                mEventList[name] -= callback;
        }
            
    }

    public void Dispatch(string name, T value)
    {
        if (mEventList.TryGetValue(name, out var _callbackList))
        {
            _callbackList?.Invoke(value);
        }
    }
}

public class EventDispatcher
{
    static Dictionary<string, Action> mEventList = new Dictionary<string, Action>();
    static ParamedEventDispatcher<string> mStringEventDispatcher = new ParamedEventDispatcher<string>();
    static ParamedEventDispatcher<float> mFloatEventDispatcher = new ParamedEventDispatcher<float>();
    static ParamedEventDispatcher<Vector3> mVector3EventDispatcher = new ParamedEventDispatcher<Vector3>();
    static ParamedEventDispatcher<bool> mBoolEventDispatcher = new ParamedEventDispatcher<bool>();
    static ParamedEventDispatcher<long> mLongEventDispatcher = new ParamedEventDispatcher<long>();

    public static void RegisterEvent(string name, Action callback, bool register)
    {
        if (register)
        {
            if (!mEventList.ContainsKey(name))
                mEventList.Add(name, callback);
            else
                mEventList[name] += callback;
        }
        else
            if(mEventList.ContainsKey(name))
                mEventList[name] -= callback;
    }

    public static void Dispatch(string name)
    {
        if (mEventList.TryGetValue(name, out var _callbackList))
        {
            _callbackList?.Invoke();
        }
    }

    public static void RegisterEvent(string name, Action<string> callback, bool register)
    {
        mStringEventDispatcher.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, string value)
    {
        mStringEventDispatcher.Dispatch(name, value);
        Dispatch(name);
    }
    
    public static void RegisterEvent(string name, Action<Vector3> callback, bool register)
    {
        mVector3EventDispatcher.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, Vector3 value)
    {
        mVector3EventDispatcher.Dispatch(name, value);
        Dispatch(name);
    }

    public static void RegisterEvent(string name, Action<bool> callback, bool register)
    {
        mBoolEventDispatcher.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, bool value)
    {
        mBoolEventDispatcher.Dispatch(name, value);
        Dispatch(name);
    }

    public static void RegisterEvent(string name, Action<float> callback, bool register)
    {
        mFloatEventDispatcher.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, float value)
    {
        mFloatEventDispatcher.Dispatch(name, value);
        Dispatch(name);
    }

    public static void RegisterEvent(string name, Action<long> callback, bool register)
    {
        mLongEventDispatcher.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, long value)
    {
        mLongEventDispatcher.Dispatch(name, value);
        Dispatch(name);
    }
}

public static class Blackboard
{
    public static bool value_bool;
    public static long value_long;
    public static int value_int;
    public static string value_string;
    public static float value_float;
    public static Vector3 value_vector3;
    public static UIItemFlyManager.TargetType value_target_type;
}
