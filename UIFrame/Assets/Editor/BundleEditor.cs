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

    [MenuItem("Tools/���")]
    public static void Bulid()
    {
        m_allFileDir.Clear();
        m_allFilesAB.Clear();
        m_allPrefabPaths.Clear();

        ABConfig aBConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);

        foreach (ABConfig.FileDirName  vo in aBConfig.m_allFileDirName)
        {
            if (m_allFileDir.ContainsKey(vo.ABname))
            {
                Debug.Log("AB �����������ظ�������");
            }
            else
            {
                m_allFileDir.Add(vo.ABname, vo.Path);
                m_allFilesAB.Add(vo.Path);
            }
            //Debug.Log(vo.ABname +" "+ vo.Path );
        }
        string[] allStr = AssetDatabase.FindAssets("t:Prefab",null);
        for (int i = 0; i < allStr.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("����prefab", "prefab:" + path, i * 1.0f / allStr.Length);
            if (!isContainAllFileAB(path))
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string[] allDepend = AssetDatabase.GetDependencies(path);
                List<string> allDependPath = new List<string>();
                allDependPath.Add(path);
                for (int j = 0; j < allDepend.Length; j++)
                {
                    //Debug.Log(allDepend[j]);
                    if(!isContainAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs")){
                        m_allFilesAB.Add(allDepend[j]);
                        allDependPath.Add((string)allDepend[j]);
                    }

                }

                if (m_allPrefabPaths.ContainsKey(obj.name))
                {
                    Debug.LogError("�Ѵ�����ͬ·��"+obj.name);
                }
                else
                {
                    m_allPrefabPaths.Add(obj.name, allDependPath);
                }
            }

        }

        foreach (string name in m_allFileDir.Keys)
        {
            setABName(name, m_allFileDir[name]);
        }

        
        foreach (string name in m_allPrefabPaths.Keys)
        {
            setABName(name, m_allPrefabPaths[name]);
        }

        string[] oldABNames =   AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldABNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
            EditorUtility.DisplayProgressBar("���AB����", "���֣�" + oldABNames[i], i * 1f / oldABNames.Length);
        }
        
        EditorUtility.ClearProgressBar();        

    }
    static void setABName(string name,string path)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if(assetImporter == null)
        {
            Debug.Log("�Ҳ�����Ӧ�ļ�:"+path);
        }
        else
        {
            assetImporter.assetBundleName = name;
        }
    }

    static void setABName(string name,List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            setABName(name,paths[i]);
        }
    }
    
    

    //�Ƿ����·��
    static bool isContainAllFileAB(string path)
    {
        for (int i = 0; i < m_allFilesAB.Count; i++)
        {
            if(m_allFilesAB[i] == path || path.Contains(m_allFilesAB[i])) return true;
        }
        return false;
    }
}    
   
