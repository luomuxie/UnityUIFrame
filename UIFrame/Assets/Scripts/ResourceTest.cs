using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;

public class ResourceTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestLoadAB();
    }

    void TestLoadAB()
    {
        AssetBundle configAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assetbundleconfig");
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");
        MemoryStream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        AssetBundleConfig testSerilize =(AssetBundleConfig) formatter.Deserialize(stream);
        stream.Close();
        string path = "Assets/GameData/Prefabs/Attack.prefab";
        uint crc = CRC32.GetCRC32(path);
        ABBase aBBase = null;
        for (int i = 0; i < testSerilize.ABList.Count; i++)
        {
            if(testSerilize.ABList[i].Crc == crc)
            {
                aBBase = testSerilize.ABList[i];

            }
        }

        //加裁依赖
        for (int i = 0; i < aBBase.ABDependce.Count; i++)
        {
            AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + aBBase.ABDependce[i]);
        }

        //加载AB包
        AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath+"/"+aBBase.ABname);
        //加载模型
        GameObject obj = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>("attack"));

    }
}
