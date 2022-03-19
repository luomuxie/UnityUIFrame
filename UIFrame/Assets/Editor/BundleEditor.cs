using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleEditor
{
    public static string ABCONFIGPATH = "Assets/Editor/ABConfig.asset";
    public static Dictionary<string, string> m_allFileDir = new Dictionary<string,string>();
    public static List<string> m_allFilesAB = new List<string>();
    public static Dictionary<string, List<string>> m_allPrefabPaths = new Dictionary<string, List<string>>();

    [MenuItem("Tools/打包")]
    public static void Bulid()
    {
        m_allFileDir.Clear();
        m_allFilesAB.Clear();
        ABConfig aBConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);

        foreach (ABConfig.FileDirName  vo in aBConfig.m_allFileDirName)
        {
            if (m_allFileDir.ContainsKey(vo.ABname))
            {
                Debug.Log("AB 包配置名字重复，请检查");
            }
            else
            {
                m_allFileDir.Add(vo.ABname, vo.Path);
                m_allFilesAB.Add(vo.Path);
            }
            Debug.Log(vo.ABname +" "+ vo.Path );
        }
        string[] allStr = AssetDatabase.FindAssets("t:Prefab",null);
        for (int i = 0; i < allStr.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("查找prefab", "prefab:" + path, i * 1.0f / allStr.Length);
            if (!isContainAllFileAB(path))
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string[] allDepend = AssetDatabase.GetDependencies(path);
                List<string> allDependPath = new List<string>();
                allDependPath.Add(path);
                for (int j = 0; j < allDepend.Length; j++)
                {
                    Debug.Log(allDepend[j]);
                    if(!isContainAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs")){
                        m_allFilesAB.Add(allDepend[j]);
                        allDependPath.Add((string)allDepend[j]);
                    }

                }

                if (m_allPrefabPaths.ContainsKey(obj.name))
                {
                    Debug.LogError("已存在相同路径"+obj.name);
                }
                else
                {
                    m_allPrefabPaths.Add(obj.name, allDependPath);
                }
            }

        }
        EditorUtility.ClearProgressBar();        

    }
    //是否包含路径
    static bool isContainAllFileAB(string path)
    {
        for (int i = 0; i < m_allFilesAB.Count; i++)
        {
            if(m_allFilesAB[i] == path || path.Contains(m_allFilesAB[i])) return true;
        }
        return false;
    }
}    
   
