using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OfflineDataEditor
{
    [MenuItem("Assets/生成离线数据")]
    public static void AssetCreateOfflineData()
    {
        GameObject[] objs = Selection.gameObjects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据","正在修改:" + objs[i].name + ".........", 1.0f / objs.Length * i);
            CreateOfflineData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    public static void CreateOfflineData(GameObject obj)
    {
        OfflineData offlineData = obj.gameObject.AddComponent<OfflineData>();
        if(offlineData == null)
        {
            offlineData = obj.AddComponent<OfflineData>();
        }
        offlineData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了" + obj.name + "prebab!");
        Resources.UnloadUnusedAssets(); 
        AssetDatabase.Refresh();
    }

    [MenuItem("离线数据/生成所有UI")]
    public static void AllCreateUIData()
    {        
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/GameData/Prefabs/UGUI/" });
        for (int i = 0;i < allStr.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("添加UI离线数据", "正在扫描路径:" + prefabPath + ".........", 1.0f / allStr.Length * i);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if(obj == null)
            {
                continue;
            }
            CreateOfflineUIData(obj);
            Debug.Log("UI离线数据全部生成完毕！");
            EditorUtility.ClearProgressBar();
        }


    }

    [MenuItem("Assets/生成UI离线数据")]
    public static void AssetCreateUIOfflineData()
    {
        GameObject[] objs = Selection.gameObjects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据", "正在修改:" + objs[i].name + ".........", 1.0f / objs.Length * i);
            CreateOfflineUIData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }
    public static void CreateOfflineUIData(GameObject obj)
    {
        obj.layer = LayerMask.NameToLayer("UI");
        UIOfflineData uiData = obj.GetComponent<UIOfflineData>();
        if(uiData == null)
        {
            uiData = obj.AddComponent<UIOfflineData>();
        }
        uiData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了" + obj.name + "UI prebab!");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
}
