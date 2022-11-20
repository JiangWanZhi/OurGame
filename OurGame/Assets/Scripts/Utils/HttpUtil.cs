using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Http工具类
/// </summary>
public class HttpUtil : BaseMonoManager<HttpUtil>
{
    //Http请求返回的数据
    public delegate void HttpCallBack(String result);

    /// <summary>
    /// Http的Get请求
    /// </summary>
    public void Get(string url, HttpCallBack callback)
    {
        StartCoroutine(SendGet(url, callback));
    }

    /// <summary>
    /// Http的Post请求
    /// </summary>
    public void Post(string url, string postData, HttpCallBack callback)
    {
        byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(postData);
        StartCoroutine(SendPost(url, postBytes, callback));
    }

    /// <summary>
    /// 协程发送Get请求
    /// </summary>
    IEnumerator SendGet(string url, HttpCallBack callback)
    {
        var www = new WWW(url);
        yield return www;
        if (www.error == null) {
            callback(www.text);
        }
        else {
            Debug.LogError($"Http的Get请求出错：{www.error}");
            callback("Error");
        }
        www.Dispose();
    }

    /// <summary>
    /// 协程发送Post请求
    /// </summary>
    IEnumerator SendPost(string url, byte[] postData, HttpCallBack callback)
    {
        var www = new WWW(url, postData);
        yield return www;
        if (www.error == null) {
            callback(www.text);
        }
        else {
            Debug.LogError("Http的Post请求出错：" + www.error);
            callback("Error");
        }
        www.Dispose();
    }
}