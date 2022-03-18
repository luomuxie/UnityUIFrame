using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleEditor
{
    public static string ABCONFIGPATH = "Assets/Editor/ABConfig.asset";
    public static Dictionary<string, string> m_allFileDir = new Dictionary<string,string>();

    [MenuItem("Tools/打包")]
    public static void Bulid()
    {
        m_allFileDir.Clear();
        ABConfig config = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        foreach (var item in config.m_allPrefebPath)
        {
            
            Debug.Log(item);
        }
        ABConfig aBConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);

        foreach (ABConfig.FileDirName  vo in config.m_allFileDirName)
        {
            if (m_allFileDir.ContainsKey(vo.ABname))
            {
                Debug.Log("AB 包配置名字重复，请检查");
            }
            else
            {
                m_allFileDir.Add(vo.ABname, vo.Path);
            }
            Debug.Log(vo.ABname +" "+ vo.Path );
        }

        string[] allStr = AssetDatabase.FindAssets("t:Prefab", aBConfig.m_allPrefebPath.ToArray());
        for (int i = 0; i < allStr.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("查找prefab", "prefab:" + path, i * 1.0f / allStr.Length);
        }
        EditorUtility.ClearProgressBar();
    }
}    
   
