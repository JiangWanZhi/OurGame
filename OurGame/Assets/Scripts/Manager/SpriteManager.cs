using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public  class SpriteManager : BaseManager<SpriteManager>
{
    static Dictionary<string, SpriteAtlas> sptireAtlasDict = new Dictionary<string, SpriteAtlas>();
    static Dictionary<string, Sprite> sptireDict = new Dictionary<string, Sprite>();
    
    public static Sprite GetSprite(string name)
    {
        if (sptireDict.TryGetValue(name, out var o) && o != null) return o;
        
        foreach (var item in sptireAtlasDict) {
            var s = item.Value.GetSprite(name);
            if (s == null) continue;
            o = s;
            break;
        }
        if (o != null)
        {
            if (!sptireDict.ContainsKey(name))
                sptireDict.Add(name, o);
            else
                sptireDict[name] = o;
        }
        else {
            LogUtility.Error($"SpriteManager:can't find sprite {name}");
        }
        return o;
    }

    public void LoadAtlas(string name)
    {
        if (!sptireAtlasDict.ContainsKey(name))
        {
            var a = AssetBundleManager.Instance.LoadAsset<SpriteAtlas>(name);
            sptireAtlasDict.Add(name, a);
        }
    }

    public void UnloadAtlas(string name)
    {
        if (!sptireAtlasDict.TryGetValue(name, out var o))
        {
            sptireAtlasDict.Remove(name);
        }
    }
}
