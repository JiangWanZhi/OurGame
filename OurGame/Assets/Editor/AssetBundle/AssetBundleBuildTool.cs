using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleBuildTool
{
    public const string Asset_list_Path = "asset_list.bytes";
    const string AssetBundlesOutputPathRoot = "AssetBundles";
    const string EncryptPath_AssetBundles = "Encrypt/AssetBundles";
    const string EncryptPath = "Encrypt";

    static string AssetBundlesOutputPath
    {
        get
        {
            return Path.Combine(AssetBundlesOutputPathRoot, GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));
        }
    }
    static AssetManifest _assetManifest = new AssetManifest();
    static AssetBundleSetting _setting;

    static Dictionary<string, int> bundleFragmentDic = new Dictionary<string, int>();


    [MenuItem("Tools/AssetBundle/Set AssetBundleName")]
    public static void SetBundleNameByDirectory()
    {
        if (File.Exists(AssetBundleSetting.Path))
        {
            bundleFragmentDic.Clear();
            _setting = AssetDatabase.LoadAssetAtPath<AssetBundleSetting>(AssetBundleSetting.Path);
            SetBundleNameByDirectory(_setting.fragment0BundleList, 0);
            SetBundleNameByDirectory(_setting.fragment1BundleList, 1);

            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../save_data"));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var filePath = Path.Combine(path, Asset_list_Path);
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
            string _text = LitJson.JsonMapper.ToJson(_assetManifest);
            File.WriteAllText(filePath, _text);

            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    static void SetBundleNameByDirectory(DefaultAsset[] assetList, int fragment)
    {
        foreach (var item in assetList)
        {
            var _path = AssetDatabase.GetAssetPath(item);
            if(string.IsNullOrEmpty(_path)) continue;
            var bundleName = Path.GetFileName(_path).ToLower();
            var _dirInfo = new DirectoryInfo(_path);
            SetBundleNameByDirectory(_dirInfo, bundleName, fragment);
        }
    }

    static void SetBundleNameByDirectory(DirectoryInfo dir, string bundleName, int fragment)
    {
        bundleName = bundleName.ToLower();
        var _files = GetAllFiles(dir);
        foreach (var _fileInfo in _files)
        {
            if (IsMeta(_fileInfo)) continue;
            if (IsXml(_fileInfo)) continue;

            string _abName = _fileInfo.Name.Substring(0, _fileInfo.Name.LastIndexOf('.')).ToLower();
            bool bundleSelf = false;
            for (int i = 0; i < _setting.bundleSelfTypeList.Length; i++)
            {
                if (_fileInfo.Name.EndsWith(_setting.bundleSelfTypeList[i]))
                {
                    bundleSelf = true;
                    break;
                }
            }

            AssetImporter _assetImporter = AssetImporter.GetAtPath(_fileInfo.FullName.Substring(_fileInfo.FullName.IndexOf("Assets")));
            if (_assetImporter == null)
            {
                Debug.LogError(_fileInfo.FullName);
                continue;
            }

            _assetImporter.assetBundleName = bundleSelf ? _abName : bundleName;

            var fileName = Path.GetFileNameWithoutExtension(_fileInfo.Name).ToLower();
            var assetInfo = new AssetInfo() { name = fileName, bundleName = bundleSelf ? _abName : bundleName };
            _assetManifest.AssetList.Add(assetInfo);

            if (!bundleFragmentDic.ContainsKey(assetInfo.bundleName))
            {
                bundleFragmentDic.Add(assetInfo.bundleName, fragment);
            }
        }
    }

    [MenuItem("Tools/AssetBundle/Build AssetBundle")]
    public static void BuildAssetBundles()
    {
        _assetManifest = new AssetManifest();

        SetBundleNameByDirectory();
        
        if (Directory.Exists(AssetBundlesOutputPathRoot)) { Directory.Delete(AssetBundlesOutputPathRoot, true); }
        string _encryptOutputPath = Path.Combine(AssetBundlesOutputPathRoot, EncryptPath_AssetBundles, GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));
        string _streamingPath = Path.Combine(Application.streamingAssetsPath, AssetBundlesOutputPath);
        if (Directory.Exists(_streamingPath)) { Directory.Delete(_streamingPath, true); }
        if (Directory.Exists(_encryptOutputPath)) { Directory.Delete(_encryptOutputPath, true); }

        CreateDirectory($"./{AssetBundlesOutputPath}");

        var abManifest = BuildPipeline.BuildAssetBundles(AssetBundlesOutputPath, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.AssetBundleStripUnityVersion | BuildAssetBundleOptions.DeterministicAssetBundle, EditorUserBuildSettings.activeBuildTarget);

        BuildAssetList(abManifest);

        EncryptDirectory(AssetBundlesOutputPath, _encryptOutputPath);

        CopyDir(_encryptOutputPath, _streamingPath);

        AssetDatabase.Refresh();

        Debug.Log("<color=green>build asset bundle finish</color>");
    }
    
    [MenuItem("Tools/AssetBundle/Copy Full to StreamingPath")]
    public static void CopyFullAsset2StreamingPath()
    {
        string _encryptOutputPath = Path.Combine(AssetBundlesOutputPathRoot, EncryptPath_AssetBundles, GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));
        string _streamingPath = Path.Combine(Application.streamingAssetsPath, AssetBundlesOutputPath);
        if (Directory.Exists(_streamingPath)) { Directory.Delete(_streamingPath, true); }

        if (!Directory.Exists(_encryptOutputPath))
        {
            return;
        }
        
        CopyDirFull(_encryptOutputPath, _streamingPath);
        AssetDatabase.Refresh();
    }
    

    static void EncryptDirectory(string path, string encryptPath)
    {
        CreateDirectory(encryptPath);
        DirectoryInfo dir = new DirectoryInfo(path);
        var _fileInfoList = GetAllFiles(dir);
        foreach (FileInfo _fileInfo in _fileInfoList)
        {
            if (IsManifest(_fileInfo)) continue;

            string _outPath = Path.Combine(encryptPath, _fileInfo.Name);
            string _path = _fileInfo.FullName;

            bool isAssetList = string.Equals(_fileInfo.Name, Asset_list_Path);
            if (isAssetList)
                _outPath = Path.Combine(AssetBundlesOutputPathRoot, EncryptPath, Asset_list_Path);
            
            byte[] _newBuff = ZipHelper.EncryptData(File.ReadAllBytes(_path), isAssetList);
            File.WriteAllBytes(_outPath, _newBuff);
        }
    }

    static void CopyDir(string srcPath, string tarPath)
    {
        try
        {
            CreateDirectory(tarPath);
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            var _fileInfoList = GetAllFiles(dir);
            foreach (FileInfo _fileInfo in _fileInfoList)
            {
                foreach (var item in _assetManifest.BundleList)
                {
                    var fileName = Path.GetFileNameWithoutExtension(_fileInfo.Name).ToLower();
                    if (item.name == fileName && item.fragment == 0)
                    {
                        File.Copy(_fileInfo.FullName, Path.Combine(tarPath, _fileInfo.Name));
                        break;
                    }
                }
            }

            var _path = Path.Combine(AssetBundlesOutputPathRoot, EncryptPath, Asset_list_Path);
            var _targetPath = Path.Combine(Application.streamingAssetsPath, Asset_list_Path);
            if (File.Exists(_targetPath))
            {
                File.Delete(_targetPath);
            }
            File.Copy(_path, _targetPath);
        }
        catch
        {
            Debug.LogError("复制文件夹错误!");
        }
    }
    
    
    static void CopyDirFull(string srcPath, string tarPath)
    {
        try
        {
            CreateDirectory(tarPath);
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            var _fileInfoList = GetAllFiles(dir);
            foreach (FileInfo _fileInfo in _fileInfoList)
            {
                File.Copy(_fileInfo.FullName, Path.Combine(tarPath, _fileInfo.Name));
            }

            var _path = Path.Combine(AssetBundlesOutputPathRoot, EncryptPath, Asset_list_Path);
            var _targetPath = Path.Combine(Application.streamingAssetsPath, Asset_list_Path);
            if (File.Exists(_targetPath))
            {
                File.Delete(_targetPath);
            }
            File.Copy(_path, _targetPath);
        }
        catch
        {
            Debug.LogError("复制文件夹错误!");
        }
    }

    static void BuildAssetList(AssetBundleManifest abManifest)
    {
        BuildAssetBundleList(abManifest);
        string _text = LitJson.JsonMapper.ToJson(_assetManifest);
        File.WriteAllText(Path.Combine(AssetBundlesOutputPath, Asset_list_Path), _text);
    }

    static void BuildAssetBundleList(AssetBundleManifest abManifest)
    {
        var assetBundles = abManifest.GetAllAssetBundles();

        int _index = 0;
        int _count = assetBundles.Length;
        for (int i = 0; i < assetBundles.Length; i++)
        {
            _index++;

            FileInfo _fileInfo = new FileInfo(Path.Combine(AssetBundlesOutputPath, assetBundles[i]));

            bundleFragmentDic.TryGetValue(assetBundles[i], out var fragment);

            var _assetData = new BundleInfo()
            {
                name = _fileInfo.Name,
                //hash = AssetBundles.Utility.MD5(File.ReadAllBytes(_fileInfo.FullName)),
                hash = abManifest.GetAssetBundleHash(assetBundles[i]).ToString(),
                size = _fileInfo.Length,
                fragment = fragment,
                dependencies = new List<string>(abManifest.GetAllDependencies(assetBundles[i]))
            };
            _assetManifest.BundleList.Add(_assetData);

            EditorUtility.DisplayProgressBar("md5", _fileInfo.FullName, _index * 1.0f / _count);
        }

        EditorUtility.ClearProgressBar();
    }

    #region Utility

    public static string GetPlatformFolder(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.StandaloneOSX:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }

    public static void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            CreateDirectory(Path.GetDirectoryName(path));
            Directory.CreateDirectory(path);
        }
    }

    static List<FileInfo> GetAllFiles(DirectoryInfo dir)
    {
        List<FileInfo> fileList = new List<FileInfo>();

        try
        {
            foreach (var _fileInfo in dir.GetFiles())
            {
                fileList.Add(_fileInfo);
            }

            foreach (var directory in dir.GetDirectories())
            {
                fileList.AddRange(GetAllFiles(directory));
            }
        }
        catch
        {
            Debug.LogError($"{dir}" + "-----GetAllFiles Fail!");
        }

        return fileList;
    }

    static bool IsMeta(FileInfo _fileInfo)
    {
        return string.Equals(_fileInfo.Extension, ".meta");
    }
    static bool IsManifest(FileInfo _fileInfo)
    {
        return string.Equals(_fileInfo.Extension, ".manifest");
    }
    static bool IsXml(FileInfo _fileInfo)
    {
        return string.Equals(_fileInfo.Extension, ".xlsx");
    }
    #endregion
}
