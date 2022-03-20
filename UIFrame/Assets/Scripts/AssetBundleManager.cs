using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    public Dictionary<uint, ResourceItem> m_ResourceItemDic = new Dictionary<uint, ResourceItem>();
    public bool LoadAssetBundleConfig()
    {
        
        AssetBundle configAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assetbundleconfig");
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("assetbundleconfig");
        if(textAsset == null)
        {
            Debug.LogError("AssetBundleConfig is no exist!");
            return false;
        }
        MemoryStream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        AssetBundleConfig config = (AssetBundleConfig)formatter.Deserialize(stream);
        stream.Close();

        for (int i = 0; i < config.ABList.Count; i++)
        {
            ABBase vo = config.ABList[i];
            ResourceItem item = new ResourceItem();
            item.m_Crc = vo.Crc;
            item.m_AssetName = vo.AssetName;
            item.m_ABName = vo.ABname;
            item.m_DependAssetBundles = vo.ABDependce;
            if (m_ResourceItemDic.ContainsKey(item.m_Crc))
            {
                Debug.Log("�ظ���crc��Դ����"+item.m_AssetName+"ad����"+item.m_ABName);

            }
            else
            {
                m_ResourceItemDic.Add(item.m_Crc, item);
            }
        }
        
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
    public string m_ABName = "";
    //����Դ����
    public List<string> m_DependAssetBundles = null;
    //����Դ�������AB��
    public AssetBundle m_AssetBundle = null;

}
