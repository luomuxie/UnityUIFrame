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
}
