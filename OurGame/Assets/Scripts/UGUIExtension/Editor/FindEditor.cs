using Frame.Util;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FindEditor : MonoBehaviour
{
    private class PrefabData
    {
        public GameObject obj;
        public string className;
        public string name;
        public string path;
        public bool isPrefab = false;
    }
    private static List<PrefabData> mAllData = new List<PrefabData>();
    private static Dictionary<string, GameObject> mPrefabData = new Dictionary<string, GameObject>();
    private static GameObject mRoot;
    private static string fileSavePath = Application.dataPath + "/Scripts/UGUICode/ParseCode";
    private static bool IsWin;
    private static bool IsGenerateClickFun;
    [MenuItem("Assets/GenerateUGUICode_Win")]
    static void GenerateCode()
    {
        IsWin = true;
        IsGenerateClickFun = true;
        mPrefabData.Add("new", Selection.gameObjects[0]);
        StartTask();
    }
    [MenuItem("Assets/GenerateUGUICode_Prefab")]
    static void GenerateCodeNotRegisterButton()
    {
        IsWin = false;
        IsGenerateClickFun = false;
        mPrefabData.Add("new", Selection.gameObjects[0]);
        StartTask();
    }
    private static void StartTask()
    {
        mRoot = null;
        foreach (var item in mPrefabData)
        {
            mRoot = item.Value;
            mPrefabData.Remove(item.Key);
            break;
        }
        if (mRoot != null)
        {
            mAllData.Clear();
            DoGenerateData(mRoot);
            DoGenerateCode();
        }
    }

    private static string TagName = "UIData";
    private static bool IsHaveUnderlineInChild(GameObject obj)
    {
        bool b = false;
        foreach(var item in obj.transform.GetComponentsInChildren<Transform>())
        {
            if(item.CompareTag(TagName))
            {
                b = true;
                break;
            }    
        }
        return b;
    }
    private static void DoGenerateData(GameObject obj)
    {
        int lenth = obj.transform.childCount;
        for (int i = 0; i < lenth; i++)
        {
            var item = obj.transform.GetChild(i).gameObject;
            bool isPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(item);
            if (!item.CompareTag(TagName))
            {
                if (!isPrefab)
                    DoGenerateData(item);
                continue;
            };
            PrefabData data = new PrefabData();
            if (isPrefab)
            {
                var sourceObj = PrefabUtility.GetCorrespondingObjectFromOriginalSource(item);
                if(IsHaveUnderlineInChild(sourceObj))
                {
                    data.isPrefab = true;
                    data.obj = sourceObj;
                    data.className = data.obj.name;
                    mPrefabData[AssetDatabase.GetAssetPath(data.obj)] = data.obj;
                }
                else
                {
                    data.obj = item;
                    data.className = "GameObject";
                    data.isPrefab = false;
                }
            }
            else
            {
                data.obj = item;
                data.isPrefab = false;
                if (item.GetComponent<Button>() != null)
                {
                    data.className = "Button";
                }
                else if (item.GetComponent<Text>() != null)
                {
                    data.className = "Text";
                }
                else if (item.GetComponent<Image>() != null)
                {
                    data.className = "Image";
                }
                else
                {
                    data.className = "GameObject";
                }
                DoGenerateData(data.obj);
            }
            data.name = item.name;
            data.path = GetNodePath(mRoot, item);
            mAllData.Add(data);
        }
    }

    private static string GetNodePath(GameObject parent, GameObject obj)
    {
        string s = "";
        if (obj.transform.parent != null && obj != parent)
            s += GetNodePath(parent, obj.transform.parent.gameObject);
        if (!string.IsNullOrEmpty(s))
            s += "/";
        if (obj != parent)
            s += obj.name;
        return s;
    }

    private static string GetStringTemplate(string className, string bianLiang, string chuShiHua, string fangFa)
    {
        StringBuilder code = new StringBuilder("using UnityEngine;\r\n");
        code.AppendLine($"using UnityEngine.UI;");
        if (IsWin)
            code.AppendLine($"public partial class {className} : BaseWin");
        else
            code.AppendLine($"public partial class {className}");
        code.AppendLine("{");

        if(!IsWin)
        {
            code.AppendLine($"{GetTab(1)}public {className}(GameObject parentNode)");
            code.AppendLine($"{GetTab(1)}{{");
            code.AppendLine($"{GetTab(2)}var root = GameObject.Instantiate(AssetBundleManager.Instance.LoadPrefab(\"{className}\"), parentNode.transform);");
            code.AppendLine($"{GetTab(2)}InitUI(root);");
            code.AppendLine($"{GetTab(1)}}}");

            code.AppendLine($"{GetTab(1)}public {className}(GameObject root,bool rootIsSelf)");
            code.AppendLine($"{GetTab(1)}{{");
            code.AppendLine($"{GetTab(2)}InitUI(root);");
            code.AppendLine($"{GetTab(1)}}}");
        }
        else
        {
            code.AppendLine($"{GetTab(1)}public {className}() {{ }}");
            code.AppendLine($"{GetTab(1)}public override string Prefab => \"{className}\";");
        }

        code.AppendLine($"{bianLiang}");//变量

        if(IsWin)
        {
            code.AppendLine($"{GetTab(1)}protected override void __OnInitUI()");
            code.AppendLine($"{GetTab(1)}{{");
            code.AppendLine($"{chuShiHua}");//初始化
            code.AppendLine($"{GetTab(1)}}}");
        }
        else
        {
            code.AppendLine($"{GetTab(1)}public void InitUI(GameObject root)");
            code.AppendLine($"{GetTab(1)}{{");
            code.AppendLine($"{chuShiHua}");//初始化
            code.AppendLine($"{GetTab(1)}}}");
        }

        code.AppendLine($"{fangFa}");

        code.AppendLine("}");
        return code.ToString();
    }

    private static void DoGenerateCode()
    {
        if (mAllData.Count > 0)
        {
            StringBuilder bianLiang = new StringBuilder();
            bianLiang.AppendLine($"{GetTab(1)}public GameObject _Root;");
            foreach (var item in mAllData)
            {
                bianLiang.AppendLine($"{GetTab(1)}public {item.className} {item.name};");
            }

            StringBuilder chuShiHua = new StringBuilder();
            chuShiHua.AppendLine($"{GetTab(2)}_Root = root;");
            foreach (var item in mAllData)
            {
                if (item.isPrefab)
                {
                    var name = item.name + "Node";
                    chuShiHua.AppendLine($"{GetTab(2)}var {name} = _Root.transform.Find(\"{item.path}\").gameObject;");
                    chuShiHua.AppendLine($"{GetTab(2)}{item.name} = new {item.className}({name},true);");
                }
                else
                {
                    if (item.className == "Button" || item.className == "Text" || item.className == "Image")
                    {
                        chuShiHua.AppendLine($"{GetTab(2)}{item.name} = _Root.transform.Find(\"{item.path}\").GetComponent<{item.className}>();");
                    }
                    else
                    {
                        chuShiHua.AppendLine($"{GetTab(2)}{item.name} = _Root.transform.Find(\"{item.path}\").gameObject;");
                    }
                }
            }

            StringBuilder fangFa = new StringBuilder();
            if (IsGenerateClickFun)
            {
                foreach (var item in mAllData)
                {
                    if (item.className == "Button")
                    {
                        chuShiHua.AppendLine($"{GetTab(2)}{item.name}.onClick.AddListener({item.name}_Click);");
                    }
                }

                foreach (var item in mAllData)
                {
                    if (item.className == "Button")
                    {
                        fangFa.AppendLine($"{GetTab(1)}partial void {item.name}_Click();");
                    }
                }
            }

            string code = GetStringTemplate(mRoot.name, bianLiang.ToString(), chuShiHua.ToString(), fangFa.ToString());
            FileOperate.FileWrite($"{fileSavePath}/{mRoot.name}.cs", code);
            AssetDatabase.Refresh();
        }
        IsGenerateClickFun = false;
        IsWin = false;
        StartTask();
    }

    private static string GetTab(int num)
    {
        string s = "";
        for (int i = 0; i < num; i++)
            s += "\t";
        return s;
    }

    private delegate string delegateString();
    private static string GetIfElse(int tabNum, string condition, delegateString content, delegateString content2 = null)
    {
        StringBuilder s = new StringBuilder();
        s.AppendLine($"{GetTab(tabNum)}if({condition})");
        s.AppendLine($"{GetTab(tabNum)}{{");
        s.AppendLine($"{GetTab(tabNum)}{content()}");
        s.AppendLine($"{GetTab(tabNum)}}}");
        if (content2 != null)
        {
            s.AppendLine($"{GetTab(tabNum)}else");
            s.AppendLine($"{GetTab(tabNum)}{{");
            s.AppendLine($"{GetTab(tabNum)}{content2()}");
            s.AppendLine($"{GetTab(tabNum)}}}");
        }
        s.Remove(s.Length - 2, 2);
        return s.ToString();
    }
}
