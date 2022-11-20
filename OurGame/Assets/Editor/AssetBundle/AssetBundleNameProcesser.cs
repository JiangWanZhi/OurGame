using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

class AssetBundleNameProcesser : AssetPostprocessor
{
    static string[] skipTypeList = { ".xlsx", ".meta", ".cs", ".unity" };
    static AssetBundleSetting _setting;
    static string filePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../save_data", AssetBundleBuildTool.Asset_list_Path));

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (_setting == null)
            _setting = AssetDatabase.LoadAssetAtPath<AssetBundleSetting>(AssetBundleSetting.Path);

        if (importedAssets.Length == 0 && movedAssets.Length == 0 && deletedAssets.Length == 0)
            return;

        AssetDatabase.StartAssetEditing();
        if (!File.Exists(filePath))
        {
            AssetBundleBuildTool.CreateDirectory(Path.GetDirectoryName(filePath));
            var tempManifest = new AssetManifest();
            File.WriteAllText(filePath, LitJson.JsonMapper.ToJson(tempManifest));
        }

        string jsonStr = File.ReadAllText(filePath);
        var _assetManifest = JsonMapper.ToObject<AssetManifest>(jsonStr);

        bool changed = ProcessAssets(_assetManifest, movedFromAssetPaths, true) ||
        ProcessAssets(_assetManifest, deletedAssets, true) ||
        ProcessAssets(_assetManifest, importedAssets, false) ||
        ProcessAssets(_assetManifest, movedAssets, false);
        if (changed)
        {
            string _text = JsonMapper.ToJson(_assetManifest);
            File.WriteAllText(filePath, _text);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        AssetDatabase.StopAssetEditing();
    }

    static bool ProcessAssets(AssetManifest _assetManifest, string[] assets, bool removeOrAdd)
    {
        bool ret = false;
        foreach (var assetName in assets)
        {
            if (CheckSkip(assetName))
                continue;

            if (removeOrAdd)
                RemoveAssetInfo(assetName, _assetManifest);
            else
                SetAssetBundleName(assetName, _assetManifest);
            ret = true;
        }
        return ret;
    }

    static string CalBundleName(string assetName, DefaultAsset[] bundleList)
    {
        string ret = string.Empty;
        foreach (var asset in bundleList)
        {
            var _path = AssetDatabase.GetAssetPath(asset);
            if (PathInclude(assetName, _path))
            {
                ret = Path.GetFileName(_path).ToLower();
                break;
            }
        }
        return ret;
    }

    static void SetAssetBundleName(string assetName, AssetManifest _assetManifest)
    {
        if (_setting == null)
            return;

        string bundleName = CalBundleName(assetName, _setting.fragment0BundleList);
        if (string.IsNullOrEmpty(bundleName))
            bundleName = CalBundleName(assetName, _setting.fragment1BundleList);

        if (!string.IsNullOrEmpty(bundleName))//如果在配置列表内
        {
            foreach (var item in _setting.bundleSelfTypeList)
            {
                if (assetName.EndsWith(item))
                {
                    bundleName = Path.GetFileNameWithoutExtension(assetName).ToLower();
                    break;
                }
            }
        }

        var a = AssetImporter.GetAtPath(assetName);
        a.assetBundleName = bundleName;

        if (string.IsNullOrEmpty(bundleName))
            return;

        var fileName = Path.GetFileNameWithoutExtension(assetName).ToLower();
        AddAssetInfo(fileName, a.assetBundleName, _assetManifest);
    }

    static bool PathInclude(string path1, string path2)
    {
        var pathArr1 = path1.Split('/');
        var pathArr2 = path2.Split('/');

        if (pathArr1.Length < pathArr2.Length)
        {
            return false;
        }

        for (int i = 0; i < pathArr2.Length; i++)
        {
            if (pathArr1[i] != pathArr2[i])
            {
                return false;
            }
        }

        return true;
    }

    static void AddAssetInfo(string name, string bundleName, AssetManifest _assetManifest)
    {
        bool newAssetInfo = true;
        if (_assetManifest.AssetList == null)
        {
            _assetManifest.AssetList = new List<AssetInfo>();
        }

        foreach (var assetInfo in _assetManifest.AssetList)
        {
            if (string.Equals(name, assetInfo.name) && string.Equals(bundleName, assetInfo.bundleName))
            {
                newAssetInfo = false;
            }

            if (string.Equals(name, assetInfo.name) && !string.Equals(bundleName, assetInfo.bundleName))
            {
                Debug.LogError($"\"{name}\" exit in {AssetBundleBuildTool.Asset_list_Path}! old bundleName:\"{assetInfo.bundleName}\",new bunldeName:\"{bundleName}\"");
                assetInfo.bundleName = bundleName;
                return;
            }
        }

        if (newAssetInfo)
        {
            var assetInfo = new AssetInfo() { name = name, bundleName = bundleName };
            _assetManifest.AssetList.Add(assetInfo);
        }
    }

    static void RemoveAssetInfo(string assetName, AssetManifest _assetManifest)
    {
        if (_assetManifest.AssetList == null)
        {
            return;
        }

        var name = Path.GetFileNameWithoutExtension(assetName).ToLower();

        AssetInfo tempAssetInfo = null;
        foreach (var assetInfo in _assetManifest.AssetList)
        {
            if (string.Equals(name, assetInfo.name))
            {
                tempAssetInfo = assetInfo;
                break;
            }
        }
        if (tempAssetInfo != null)
        {
            _assetManifest.AssetList.Remove(tempAssetInfo);
        }
    }

    static bool CheckSkip(string assetName)
    {
        for (int i = 0; i < skipTypeList.Length; i++)
        {
            if (assetName.EndsWith(skipTypeList[i]))
            {
                return true;
            }
        }

        return false;
    }
}
