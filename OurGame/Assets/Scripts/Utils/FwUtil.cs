using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;

/// <summary>
/// 工具类
/// </summary>
public class FwUtil
{
    /// <summary>
    /// 返回json字符串是否有key
    /// </summary>
    public static bool Contains(string json, string key)
    {
        JsonData msgData = JsonMapper.ToObject(json);
        if (((IDictionary)msgData).Contains(key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 安全读取文件的内容以byte[]方式返回
    /// </summary>
    public static byte[] SafeReadAllBytes(string inFile)
    {
        try
        {
            if (string.IsNullOrEmpty(inFile))
            {
                return null;
            }

            if (!File.Exists(inFile))
            {
                return null;
            }
            File.SetAttributes(inFile, FileAttributes.Normal);
            return File.ReadAllBytes(inFile);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(string.Format("SafeReadAllBytes Fail: FilePath = {0}, Error= {1}", inFile, ex.Message));
            return null;
        }
    }

    /// <summary>
    /// 获取本地AB资源打包的生成目录
    /// </summary>
    public static List<string> GetFile(string path, List<string> FileList)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] fil = dir.GetFiles();
        DirectoryInfo[] dii = dir.GetDirectories();
        foreach (FileInfo f in fil)
        {
            long size = f.Length;
            FileList.Add(f.FullName);
        }
        //获取子文件夹内的文件列表，递归遍历
        foreach (DirectoryInfo d in dii)
        {
            GetFile(d.FullName, FileList);
        }
        return FileList;
    }
    
    
    /// <summary>
    /// 根据两个坐标计算其角度
    /// </summary>
    /// <param name="from_"></param>
    /// <param name="to_"></param>
    /// <returns></returns>
    public static float DirToAngle(Vector3 from_, Vector3 to_)
    {
        //两点的x、y值
        float x = from_.x - to_.x;
        float y = from_.y - to_.y;

        //斜边长度
        float hypotenuse = Mathf.Sqrt(Mathf.Pow(x,2f)+Mathf.Pow(y,2f));

        //求出弧度
        float cos = x / hypotenuse;
        float radian = Mathf.Acos(cos);

        //用弧度算出角度    
        float angle = 180 / (Mathf.PI / radian);
          
        if (y < 0)
        {
            angle = -angle;
        }
        else if ((y == 0) && (x < 0))
        {
            angle = 180;
        }
        return angle;
    }
}

//public static class UGUIExtension
//{
//    /// <summary>
//    /// 获取某个组件 如果没有该组件 则自动添加
//    /// </summary>
//    /// <param name="self"></param>
//    /// <typeparam name="T"></typeparam>
//    /// <returns></returns>
//    public static T GetOrAddComponent<T>(this GameObject self) where T : Component
//    {
//        if (self.GetComponent<T>() == null) {
//            self.AddComponent<T>();
//        }
//        return self.GetComponent<T>();
//    }
//}
