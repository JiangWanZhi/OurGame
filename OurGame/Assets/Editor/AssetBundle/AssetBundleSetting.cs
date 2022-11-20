using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetBundleSetting", menuName = "AssetBundleSetting")]
public class AssetBundleSetting : ScriptableObject
{
    public static readonly string Path = "Assets/Editor/AssetBundle/AssetBundleSetting.asset";
    public string[] bundleSelfTypeList;
    public DefaultAsset[] fragment0BundleList;
    public DefaultAsset[] fragment1BundleList;
}
