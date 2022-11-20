using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Text;

public class DownloadInfo
{
    public string targetVersion;
    public int targetFragment;
    public long totalSize;
    public AssetManifest serverAssetManifest;
    public List<string> downloadList = new List<string>();
    public List<string> finishList = new List<string>();
}

public partial class AssetBundleManager
{
#if UNITY_ANDROID
    public const string BASE_DOWNLOAD_URL = "https://akmcdn.fmz.funminigame.com/android/";
#elif UNITY_IPHONE
    public const string BASE_DOWNLOAD_URL = "https://akmcdn.fmz.funminigame.com/ios/";
#else
    public const string BASE_DOWNLOAD_URL = "https://akmcdn.fmz.funminigame.com/windows/";
#endif
    /// <summary>
    /// download size, total size
    /// </summary>
    public Action OnRetryTimeout;
    public long toDownloadSize;
    public int toDownloadFragment;
    public List<string> toDownload = new List<string>();
    public DownloadInfo CurDownloadInfo;
    private AssetManifest serverAssetManifest;
    private byte[] serverAssetManifestData;
    public bool IsDownLoadFlag;

    public string GetServerAssetURL()
    {
        return $"{BASE_DOWNLOAD_URL}{Application.version}/{ServerAssetVersion}/{AssetListPath}";
    }

    public void SetServerAssetList(byte[] data)
    {
        serverAssetManifestData = data;
        var d = Encoding.UTF8.GetString(ZipHelper.DecryptData(data, true));
        serverAssetManifest = JsonConvert.DeserializeObject<AssetManifest>(d);
    }

    public bool CheckDownloadInfoVersion()
    {
        return CurDownloadInfo != null && CurDownloadInfo.targetVersion == ServerAssetVersion;
    }

    public void SaveDownloadInfo()
    {
        CurDownloadInfo = new DownloadInfo {
            targetVersion = ServerAssetVersion,
            targetFragment = toDownloadFragment,
            serverAssetManifest = serverAssetManifest,
            downloadList = toDownload,
            totalSize = toDownloadSize
        };
        PlayerPrefs.SetString(KEY_FOR_DOWNLOAD_INFO, CurDownloadInfo.ToJson());
    }

    public bool IsDownloadFragment()
    {//游戏内只需要判断下载信息是否为空，因为热更结束会清除下载信息，只有分包下载信息会保留到游戏内。
        return CurDownloadInfo != null;
    }

    public void DownloadNextFragment()
    {
        toDownloadFragment = AssetFragment + 1;
        LogUtility.Info($"[AssetBundleManager] DownloadNextFragment {toDownloadFragment}");
        toDownloadSize = 0;
        toDownload.Clear();
        foreach (var item in m_AssetManifest.BundleList)
        {
            if (item.fragment > AssetFragment && item.fragment <= toDownloadFragment)
            {
                toDownload.Add(item.name);
                toDownloadSize += item.size;
            }
        }
        serverAssetManifest = m_AssetManifest;
        SaveDownloadInfo();
    }

    public bool CompareAssetList()
    {
        return CompareAssetList(AssetFragment);
    }

    public bool CompareAssetList(int fragment)
    {
        toDownloadSize = 0;
        toDownload.Clear();
        var bundleList = m_AssetManifest.BundleList;
        var serverBundleList = serverAssetManifest.BundleList;
        foreach (var item in serverBundleList)
        {
            if (item.fragment > fragment)
            {
                continue;
            }
            bool needDownload = true;
            for (int j = 0; j < bundleList.Count; j++)
            {
                var _a = item;
                var _b = bundleList[j];
                if (_a.name.Equals(_b.name) && _a.hash.Equals(_b.hash))
                {
                    needDownload = false;
                    break;
                }
            }
            if (needDownload)
            {
                toDownload.Add(item.name);
                toDownloadSize += item.size;
            }
        }
        return toDownloadSize > 0;
    }

    private DownloadHandler m_DownloadHandler;
    private long m_TempDownloadSize;

    public long GetNowDownloadSize()
    {
        if (m_DownloadHandler == null)
            return m_TempDownloadSize;
        return m_TempDownloadSize + m_DownloadHandler.data.Length;
    }

    public long GetAllDownloadSize()
    {
        if (CurDownloadInfo == null)
            return m_TempDownloadSize;
        return CurDownloadInfo.totalSize;
    }

    public static bool WaitingConfirm = false;
    public static string DownloadNetError = "DownloadNetError";
    IEnumerator DownloadOne(string path)
    {
        IsDownLoadFlag = true;
        int retryCount = 0;
        string _url = $"{BASE_DOWNLOAD_URL}{Application.version}/{ServerAssetVersion}/{AssetMidPath}/{path}";
        LogUtility.Info($"start download {_url}");
        yield return null;
        while (true)
        {
            UnityWebRequest _web = UnityWebRequest.Get(_url);
            m_DownloadHandler = _web.downloadHandler;
            yield return _web.SendWebRequest();
            if (_web.result == UnityWebRequest.Result.ProtocolError || _web.result == UnityWebRequest.Result.ConnectionError)
            {
                m_DownloadHandler = null;
                LogUtility.Info($"DownloadOne error: {_web.error}");
                retryCount++;
                if (retryCount > 3)
                {
                    retryCount = 0;
                    WaitingConfirm = true;
                    EventDispatcher.Dispatch(DownloadNetError);
                    yield return new WaitUntil(() => { return !WaitingConfirm; });
                }
                continue;
            }
            else
            {
                byte[] _data = _web.downloadHandler.data;
                string _savePath = Path.Combine(TempPath, path);
                FileInfo _fileInfo = new FileInfo(_savePath);
                if (_fileInfo.Exists)
                {
                    _fileInfo.Delete();
                }
                else if (!_fileInfo.Directory.Exists)
                {
                    _fileInfo.Directory.Create();
                }
                File.WriteAllBytes(_savePath, _data);
                m_TempDownloadSize += _data.Length;
                LogUtility.Info($"download {_savePath} success");
                CurDownloadInfo.finishList.Add(path);
                PlayerPrefs.SetString(KEY_FOR_DOWNLOAD_INFO, CurDownloadInfo.ToJson());
                m_DownloadHandler = null;
                break;
            }
        }
    }

    public static string StartDownload = "StartDownload";
    public static string FinishDownload = "FinishDownload";
    public IEnumerator DownloadAsset(bool InLogin = false)
    {
        LogUtility.Info($"[AssetBundleManager] DownloadAsset");
        EventDispatcher.Dispatch(StartDownload);
        IsDownLoadFlag = true;
        foreach (var item in CurDownloadInfo.finishList)
        {
            foreach (var sitem in CurDownloadInfo.serverAssetManifest.BundleList)
            {
                if (sitem.name == item)
                {
                    m_TempDownloadSize += sitem.size;
                    break;
                }
            }
        }

        bool enquired = false;
        bool enable4GDownload = false;
        foreach (var item in CurDownloadInfo.downloadList)
        {
            if (CurDownloadInfo.finishList.Contains(item))
            {
                continue;
            }
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                if (!enquired)
                {
                    bool waiting = true;
                    enquired = true;
                    //var titleText = Localization.Instance.Get(TranslationID.Settings_tips);
                    //var messageText = Localization.Instance.Get(TranslationID.DownloadRes_DownloadNotWithWIFI);
                    //TipManager.Instance.ShowConfirm(titleText, messageText, (result) =>
                    //{
                    //    if (result)
                    //    {
                    //        enable4GDownload = true;
                    //    }
                    //    else
                    //    {
                    //        if (InLogin)
                    //        {
                    //            Application.Quit();
                    //        }
                    //    }
                    //    waiting = false;
                    //});
                    yield return new WaitUntil(() => { return !waiting; });
                }
                if (!enable4GDownload)
                {
                    IsDownLoadFlag = false;
                    yield return new WaitUntil(() => { return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork; });
                }
            }
            yield return StartCoroutine(DownloadOne(item));
        }
        CopyTempDirectory(TempPath);
        ClearTempDirectory();
        if (AssetVersion != CurDownloadInfo.targetVersion)
        {
            m_AssetManifest = CurDownloadInfo.serverAssetManifest;
            AssetVersion = CurDownloadInfo.targetVersion;
            PlayerPrefs.SetString(KEY_FOR_ASSET_VERSION, AssetVersion);
            string path = Path.Combine(PersistentDataPath, AssetListPath);
            File.WriteAllBytes(path, serverAssetManifestData);
            serverAssetManifestData = null;
        }
        if (AssetFragment != CurDownloadInfo.targetFragment)
        {
            AssetFragment = CurDownloadInfo.targetFragment;
            PlayerPrefs.SetInt(KEY_FOR_ASSET_FRAGMENT, AssetFragment);
        }
        CurDownloadInfo = null;
        m_TempDownloadSize = 0;
        PlayerPrefs.DeleteKey(KEY_FOR_DOWNLOAD_INFO);
        PlayerPrefs.Save();
        EventDispatcher.Dispatch(FinishDownload);
        LogUtility.Info($"[AssetBundleManager] save asset version: {AssetVersion}");
        yield return new WaitForSeconds(1);
    }

    float DownloadProgress
    {
        get
        {
            if (m_DownloadHandler != null)
            {
                long _downloadSize = m_TempDownloadSize + m_DownloadHandler.data.Length;
                return _downloadSize / (float)CurDownloadInfo.totalSize;
            }
            return 0;
        }
    }

    public void ClearTempDirectory()
    {
        LogUtility.Info($"[AssetBundleManager] ClearTempDirectory");
        if (Directory.Exists(TempPath))
        {
            Directory.Delete(TempPath, true);
        }
        Directory.CreateDirectory(TempPath);
    }

    void CopyTempDirectory(string path)
    {
        LogUtility.Info($"[AssetBundleManager] CopyTempDirectory, path: {path}");
        if (!Directory.Exists(path))
        {
            return;
        }

        DirectoryInfo _dirInfo = new DirectoryInfo(path);
        // files
        FileInfo[] _fileList = _dirInfo.GetFiles();
        foreach (FileInfo _fileInfo in _fileList)
        {
            string _tempFile = _fileInfo.FullName;
#if UNITY_EDITOR
            var _targetFile = _fileInfo.FullName.Replace("\\temp", "");
#else
            var _targetFile = _fileInfo.FullName.Replace("/temp", "");
#endif
            LogUtility.Info($"copy {_tempFile} -> {_targetFile}");
            if (File.Exists(_targetFile))
            {
                File.Delete(_targetFile);
            }
            File.Copy(_tempFile, _targetFile);
        }
    }
}
