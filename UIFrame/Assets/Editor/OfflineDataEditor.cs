using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OfflineDataEditor
{
    [MenuItem("Assets/������������")]
    public static void AssetCreateOfflineData()
    {
        GameObject[] objs = Selection.gameObjects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("�����������","�����޸�:" + objs[i].name + ".........", 1.0f / objs.Length * i);
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
        Debug.Log("�޸���" + obj.name + "prebab!");
        Resources.UnloadUnusedAssets(); 
        AssetDatabase.Refresh();
    }
}
