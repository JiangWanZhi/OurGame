using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public static class ArrayExtention
{
    public static bool ArrayContains<T>(this T[] array, T value) where T : UnityEngine.Object
    {
        bool flag = (array != null) && (value != null);
        if (flag)
        {
            flag = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    return true;
                }
            }
        }
        return flag;
    }

    public static bool ArrayEqual<T>(this IList<T> array1, IList<T> array2) where T : UnityEngine.Object
    {
        if ((array2 == null) || (array1 == null))
        {
            return false;
        }
        bool flag = array1.Count == array2.Count;
        if (flag)
        {
            for (int i = 0; (i < array1.Count) && flag; i++)
            {
                if (array1[i] != null)
                {
                    flag &= array1[i] == array2[i];
                }
            }
        }
        return flag;
    }

    public static List<T> Diff<T>(IList<T> array1, IList<T> array2)
    {
        List<T> list = new List<T>();
        foreach (T local in array1)
        {
            if (!array2.Contains(local))
            {
                list.Add(local);
            }
        }
        foreach (T local2 in array2)
        {
            if (!array1.Contains(local2))
            {
                list.Add(local2);
            }
        }
        return list;
    }

    public static T[] EnsureLength<T>(T[] array, int desiredLength, bool ensureAtLeast = false)
    {
        if ((array != null) && !(!ensureAtLeast ? (array.Length != desiredLength) : (array.Length < desiredLength)))
        {
            return array;
        }
        return new T[desiredLength];
    }

    public static T FirstObject<T>(this IList<T> list)
    {
        return ((list.Count <= 0) ? default(T) : list[0]);
    }

    public static T LastObject<T>(this IList<T> list)
    {
        return ((list.Count <= 0) ? default(T) : list[list.Count - 1]);
    }

    public static T RandomObject<T>(this IList<T> list)
    {
        return ((list.Count <= 0) ? default(T) : list[UnityEngine.Random.Range(0, list.Count)]);
    }

    public static K RandomObjectKey<K, V>(this IDictionary<K, V> dic)
    {
        var keyList = new List<K>();
        foreach (var item in dic)
        {
            keyList.Add(item.Key);
        }
        return RandomObject(keyList);
    }

    public static List<K> RandomObjectKey<K, V>(this IDictionary<K, V> dic, int count, bool repeatRandom = false)
    {
        var keyList = new List<K>();
        foreach (var item in dic)
        {
            keyList.Add(item.Key);
        }
        return RandomListObject(keyList, count, repeatRandom);
    }

    public static List<T> RandomListObject<T>(this IList<T> oldList, int count, bool repeatRandom = false)
    {
        if (oldList == null || oldList.Count == 0)
        {
            return null;
        }
        if (!repeatRandom && oldList.Count < count)
        {
            return null;
        }
        var indexList = new List<int>();
        var resultIndex = new List<int>();
        var resultList = new List<T>();
        for (int i = 0; i < oldList.Count; i++)
        {
            indexList.Add(i);
        }
        for (int i = 0; i < count; i++)
        {
            var index = UnityEngine.Random.Range(0, indexList.Count);
            resultIndex.Add(indexList[index]);
            if (!repeatRandom)
            {
                indexList.Remove(indexList[index]);
            }
        }
        foreach (var item in resultIndex)
        {
            resultList.Add(oldList[item]);
        }
        return resultList;
    }

    public static T GetRandomFilterValue<T>(this IList<T> list, ICollection<T> filter)
    {
        T value = RandomObject(list);
        if (filter.Contains(value))
        {
            return list.GetRandomFilterValue(filter);
        }

        return value;
    }
}