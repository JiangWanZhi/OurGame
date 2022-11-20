using System;
using System.Collections;
using UnityEngine;
using System.IO;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class AssetBundleManager
{
    public const string AssetListPath = "asset_list.bytes";
    public const string LoadingProgress = "LoadingProgress";

    static string assetMidPath;
    public static string AssetMidPath
    {
        get
        {
            if (string.IsNullOrEmpty(assetMidPath))
            {
                assetMidPath = $"AssetBundles/{BaseUtility.GetPlatformName()}";
            }
            return assetMidPath;
        }
    }

    static string streamingAssetPath;
    public static string StreamingAssetPath
    {
        get
        {
            if (string.IsNullOrEmpty(streamingAssetPath))
            {
                streamingAssetPath = $"{Application.streamingAssetsPath}/{AssetMidPath}";
            }
            return streamingAssetPath;
        }
    }

    static string persistentDataPath;
    public static string PersistentDataPath
    {
        get
        {
            if (string.IsNullOrEmpty(persistentDataPath))
            {
                persistentDataPath = $"{Application.persistentDataPath}/{AssetMidPath}";
            }
            return persistentDataPath;
        }
    }

    public GameObject LoadPrefab(string assetName)
    {
        return LoadAsset<GameObject>(assetName);
    }

    public byte[] LoadBytes(string assetName)
    {
        return LoadAsset<TextAsset>(assetName).bytes;
    }

    public Material LoadMaterial(string assetName)
    {
        return LoadAsset<Material>(assetName);
    }

    public T LoadAsset<T>(string assetName) where T : Object
    {
#if UNITY_EDITOR
        return LoadAssetEditor<T>(assetName);
#endif
        if (!AssetNameToBundleList.TryGetValue(assetName, out var bundle))
        {
            var bundleName = GetBundleName(assetName);
            bundle = LoadBundle(bundleName);
            AssetNameToBundleList[assetName] = bundle;
        }
        return bundle.LoadAsset<T>(assetName);
    }

    public IEnumerator LoadAllAsset(Action finishCall)
    {
        var _bundleList = m_AssetManifest.BundleList;
        var _totalAssetFileCount = _bundleList.Count;
        var _loadAssetFileCount = 0;
        for (var i = 0; i < _bundleList.Count; i++)
        {
            var _bundleData = _bundleList[i];
            if (bundleList.ContainsKey(_bundleData.name))
            {
                _loadAssetFileCount++;
                continue;
            }
            if (_bundleData.fragment > AssetFragment)
            {
                continue;
            }
            string _path = Path.Combine(PersistentDataPath, _bundleData.name);
            LogUtility.Info($"start load assetBundle {_path}");
            var d = ZipHelper.DecryptData(File.ReadAllBytes(_path));
            AssetBundle _bundle = AssetBundle.LoadFromMemory(d);
            LogUtility.Info($"load assetBundle {_path}");
            bundleList.Add(_bundleData.name, _bundle);
            _loadAssetFileCount++;
            EventDispatcher.Dispatch(LoadingProgress, _loadAssetFileCount / (float)_totalAssetFileCount);
            yield return null;
        }
        LogUtility.Info($"[AssetBundleManager] LoadAllAsset, assetBundle size: {bundleList.Count}");
        finishCall?.Invoke();
    }

    public AssetBundle LoadBundleImpl(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName)) { return null;}
        
        AssetBundle ret = null;
        string _path = Path.Combine(PersistentDataPath, bundleName);
        if (File.Exists(_path))
        {
            var d = ZipHelper.DecryptData(File.ReadAllBytes(_path));
            ret = AssetBundle.LoadFromMemory(d);
        }
        else
        {
            LogUtility.Error($"can not load bundle: {bundleName}  {_path}");
        }
        return ret;
    }

    public AssetBundle LoadBundle(string bundleName)
    {
        if (bundleList.TryGetValue(bundleName, out var bundle)) 
            return bundle;
        
        var info = GetBundleInfo(bundleName);
        bundle = LoadBundleImpl(bundleName);
        bundleList.Add(bundleName, bundle);
        foreach (var item in info.dependencies) {
            LoadBundle(item);
        }
        return bundle;
    }

    public void UnLoadBundle(string bundle, bool unloadAllLoadedObjects)
    {
        if (bundleList.ContainsKey(bundle))
        {
            bundleList[bundle].Unload(unloadAllLoadedObjects);
        }
    }

    public void UnloadAllBundle()
    {
        foreach (var item in bundleList)
        {
            item.Value.Unload(true);
        }
        bundleList.Clear();
        AssetNameToBundleList.Clear();
    }

    public BundleInfo GetBundleInfo(string bundleName)
    {
        BundleInfo ret = null;
        foreach (var item in m_AssetManifest.BundleList)
        {
            if (item.name == bundleName)
            {
                ret = item;
                break;
            }
        }
        return ret;
    }

    public string GetBundleName(string assetName)
    {
        string lowerName = assetName.ToLower();
        foreach (var item in m_AssetManifest.AssetList)
        {
            if (item.name == lowerName)
            {
                return item.bundleName;
            }
        }
        return string.Empty;
    }

    public IEnumerator LoadBundleAsync(string bundle, Action<AssetBundle> call)
    {
        string _path = Path.Combine(PersistentDataPath, bundle);
        if (File.Exists(_path))
        {
            var d = ZipHelper.DecryptData(File.ReadAllBytes(_path));
            AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(d);
            yield return createRequest;
            call?.Invoke(createRequest.assetBundle);
            yield break;
        }
        else
        {
            LogUtility.Error($"can not load bundle: {bundle}");
        }

        call?.Invoke(null);
    }

    public IEnumerator LoadAssetAsync<T>(string assetName, Action<T> call) where T : Object
    {
#if UNITY_EDITOR
        call?.Invoke(LoadAssetEditor<T>(assetName));
        yield break;
#endif
        if (!AssetNameToBundleList.TryGetValue(assetName, out var bundle))
        {
            string bundleName = GetBundleName(assetName);
            if (!bundleList.TryGetValue(bundleName, out bundle))
            {
                yield return StartCoroutine(LoadBundleAsync(bundleName, (b) =>
                 {
                     if (b != null)
                     {
                         bundle = b;
                         bundleList.Add(bundleName, bundle);
                         AssetNameToBundleList[assetName] = bundle;
                     }
                 }));
            }
            else
            {
                AssetNameToBundleList[assetName] = bundle;
            }
        }

        if (bundle != null)
        {
            AssetBundleRequest request = bundle.LoadAssetAsync<T>(assetName);
            yield return request;
            call?.Invoke((T)request.asset);
        }
    }

#if UNITY_EDITOR
    private T LoadAssetEditor<T>(string assetName) where T : Object
    {
        string bundleName = GetBundleName(assetName);
        // load from unity
        string[] _assetPath = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
        if (_assetPath.Length == 0)
        {
            LogUtility.Error($"can not find asset with name {assetName} in {bundleName}");
            return null;
        }
        else
        {
            T _obj = AssetDatabase.LoadAssetAtPath<T>(_assetPath[0]);
            if (_obj == null)
            {
                LogUtility.Error($"can not load asset {assetName} in {bundleName} as {typeof(T)}");
            }
            return _obj;
        }
    }
#endif
}
