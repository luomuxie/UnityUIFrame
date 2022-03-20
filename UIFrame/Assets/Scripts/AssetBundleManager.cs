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
    //��Դ·��CRC
    public uint m_Crc = 0;
    //����Դ���ļ���
    public string m_AssetName = "";
    //����Դ����AB��
    public string m_AssetBundleName = "";
    //����Դ����
    public List<string> m_DependAssetBundles = null;
    //����Դ�������AB��
    public AssetBundle m_AssetBundle = null;

}
