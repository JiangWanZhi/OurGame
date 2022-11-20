using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using LitJson;
using System.Collections;
using UnityEngine.Networking;

public class AssetInfo
{
    public string name;
    public string bundleName;
}

public class BundleInfo
{
    public string name;
    public string hash;
    public long size;
    public int fragment;
    public List<string> dependencies;
}

public class AssetManifest
{
    public List<AssetInfo> AssetList;
    public List<BundleInfo> BundleList;

    public AssetManifest()
    {
        AssetList = new List<AssetInfo>();
        BundleList = new List<BundleInfo>();
    }
}

public partial class AssetBundleManager : BaseMonoManager<AssetBundleManager>
{
    const string KEY_FOR_ASSET_VERSION = "asset_version";
    const string KEY_FOR_ASSET_FRAGMENT = "asset_fragment";
    const string KEY_FOR_DOWNLOAD_INFO = "download_info";

    public int AssetFragment { get; set; }
    public string AssetVersion { get; private set; }
    public string ServerAssetVersion { get; set; }
    
    private string TempPath { get; set; }
    private Dictionary<string, AssetBundle> bundleList;
    private Dictionary<string, AssetBundle> AssetNameToBundleList;
    private AssetManifest m_AssetManifest;

    protected override void OnInitialize()
    {
        if(Initialized){return;}

        bundleList = new Dictionary<string, AssetBundle>();
        AssetNameToBundleList = new Dictionary<string, AssetBundle>();
        
        TempPath = PersistentDataPath + "/temp";
        AssetVersion = PlayerPrefs.GetString(KEY_FOR_ASSET_VERSION);
        AssetFragment = PlayerPrefs.GetInt(KEY_FOR_ASSET_FRAGMENT);
        if (PlayerPrefs.HasKey(KEY_FOR_DOWNLOAD_INFO))
        {
            CurDownloadInfo = PlayerPrefs.GetString(KEY_FOR_DOWNLOAD_INFO).FromJson<DownloadInfo>();
        }
        LogUtility.Info($"[AssetBundleManager] asset version: {AssetVersion}");
        LogUtility.Info($"[AssetBundleManager] asset fragment: {AssetFragment}");

#if UNITY_EDITOR
        var filePath = Path.GetFullPath(Path.Combine(Application.dataPath, $"../{AssetListPath}"));
        string jsonStr = File.ReadAllText(filePath, Encoding.Default);
        m_AssetManifest = JsonMapper.ToObject<AssetManifest>(jsonStr);
        return;
#endif
        LoadDownloadAssetList();

        Initialized = true;
    }
    
    public bool CheckVersion()
    {
        // 如果服务端下发的资源版本号为特殊值，则默认认为不需要
        if (ServerAssetVersion == "0"){
            LogUtility.Error($"警告!!!    当前服务端未配置资源版本号，默认使用本地资源");
            return false;
        }
        return AssetVersion != ServerAssetVersion;
    }
    
    public void ClearCacheData()
    {
        PlayerPrefs.DeleteKey(KEY_FOR_ASSET_VERSION);
        PlayerPrefs.DeleteKey(KEY_FOR_ASSET_FRAGMENT);
        PlayerPrefs.Save();
    }

    public void LoadDownloadAssetList()
    {
        string _path = Path.Combine(PersistentDataPath, AssetListPath);
        var d = ZipHelper.DecryptData(File.ReadAllBytes(_path),true);
        string _text = Encoding.UTF8.GetString(d);
        m_AssetManifest = JsonConvert.DeserializeObject<AssetManifest>(_text);
        LogUtility.Info($"[AssetBundleManager] load asset list success, size: {m_AssetManifest.BundleList.Count}");
    }

    public IEnumerator DumpStreaming()
    {
        var data = File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, AssetListPath));
        var d = ZipHelper.DecryptData(data, true);
        string _text = Encoding.UTF8.GetString(d);
        AssetManifest am = JsonConvert.DeserializeObject<AssetManifest>(_text);
        string path = Path.Combine(PersistentDataPath, AssetListPath);
        BaseUtility.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, data);

        foreach (var item in am.BundleList)
        {
            // LogUtility.Info($"dump assetBundle {item.name}");
#if FULLDEBUG
            data = File.ReadAllBytes(Path.Combine(StreamingAssetPath, item.name));
            File.WriteAllBytes(Path.Combine(PersistentDataPath, item.name), data);
#else
            if (item.fragment == 0)
            {
                data = File.ReadAllBytes(Path.Combine(StreamingAssetPath, item.name));
                File.WriteAllBytes(Path.Combine(PersistentDataPath, item.name), data);
            }
#endif
        }
        LogUtility.Info($"[AssetBundleManager] DumpStreaming success, size: {am.BundleList.Count}");

        yield return null;
    }

    public IEnumerator DumpStreaming_Android(Action finishCall)
    {
        AssetManifest am;
        using (var _web = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, AssetListPath))) {
            yield return _web.SendWebRequest();
            var d = ZipHelper.DecryptData(_web.downloadHandler.data, true);
            string _text = Encoding.UTF8.GetString(d);
            am = JsonConvert.DeserializeObject<AssetManifest>(_text);
            string path = Path.Combine(PersistentDataPath, AssetListPath);
            BaseUtility.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, _web.downloadHandler.data);
        }
        foreach (var item in am.BundleList) {
            LogUtility.Info($"dump assetBundle {item.name}");
#if FULLDEBUG
            using (var _web = UnityWebRequest.Get(Path.Combine(StreamingAssetPath, item.name)))
            {
                yield return _web.SendWebRequest();
                File.WriteAllBytes(Path.Combine(PersistentDataPath, item.name), _web.downloadHandler.data);
            }
#else
            if (item.fragment == 0)
            {
                using (var _web = UnityWebRequest.Get(Path.Combine(StreamingAssetPath, item.name)))
                {
                    yield return _web.SendWebRequest();
                    File.WriteAllBytes(Path.Combine(PersistentDataPath, item.name), _web.downloadHandler.data);
                }
            }
#endif
        }
        LogUtility.Info($"[AssetBundleManager] DumpStreaming success, size: {am.BundleList.Count}");
        yield return null;
        finishCall?.Invoke();
    }
}
