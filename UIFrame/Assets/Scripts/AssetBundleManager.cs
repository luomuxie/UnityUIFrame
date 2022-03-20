using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    public bool LoadAssetBundleConfig()
    {
        
        AssetBundle configAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assetbundleconfig");
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");
        if(textAsset != null)
        {
            Debug.LogError("AssetBundleConfig is no exist!");
            return false;
        }
        MemoryStream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        AssetBundleConfig testSerilize = (AssetBundleConfig)formatter.Deserialize(stream);
        stream.Close();
        return true;
    }
}

public class ResourceItem
{
    //资源路径CRC
    public uint m_Crc = 0;
    //该资源的文件名
    public string m_AssetName = "";
    //该资源所在AB包
    public string m_AssetBundleName = "";
    //该资源依赖
    public List<string> m_DependAssetBundles = null;
    //该资源加载完的AB包
    public AssetBundle m_AssetBundle = null;

}
