using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleEditor
{
    public static string ABCONFIGPATH = "Assets/Editor/ABConfig.asset";

    [MenuItem("Tools/´ò°ü")]
    public static void Bulid()
    {
        ABConfig config = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        foreach (var item in config.m_allPrefebPath)
        {
            Debug.Log(item);
        }

        foreach (ABConfig.FileDirName  vo in config.m_allFileDirName)
        {
            Debug.Log(vo.ABname +" "+ vo.Path );
        }
    }
}    
   
