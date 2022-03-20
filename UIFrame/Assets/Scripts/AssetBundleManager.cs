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
                Debug.Log("重复的crc资源名："+item.m_AssetName+"ad包："+item.m_ABName);

            }
            else
            {
                m_ResourceItemDic.Add(item.m_Crc, item);
            }
        }
        
        return true;
    }

    public ResourceItem LoadResourceAssetBundle(uint crc)
    {
        ResourceItem item = null;
        if(!m_ResourceItemDic.TryGetValue(crc, out item) || item == null)
        {
            Debug.Log(string.Format("LoadResourceAssetBundle error:can not find crc {0} in AssetBundleConfig", crc.ToString()));
            return item;
        }
        if(item.m_AssetBundle != null)
        {
            return item;
        }
        item.m_AssetBundle = LoadAssetBundle(item.m_ABName);
        if(item.m_DependAssetBundles != null)
        {
            for (int i = 0; i < item.m_DependAssetBundles.Count; i++)
            {
                LoadAssetBundle(item.m_DependAssetBundles[i]);
            }
        }
        return item;
    }

    private AssetBundle LoadAssetBundle(string name)
    {
        AssetBundle bundle = null;
        string fullPath = Application.streamingAssetsPath+ "/"+name;
        if (File.Exists(fullPath))
        {
            bundle = AssetBundle.LoadFromFile(fullPath);
        }
        if(bundle == null)
        {
            Debug.Log("Load AssetBundle Error:"+fullPath);
        }
        return bundle;
    }

}

public class AssetBundleItem
{
    public AssetBundle m_AssetBundle;
    public int m_RefCnt;
}

public class ResourceItem
{
    //资源路径CRC
    public uint m_Crc = 0;
    //该资源的文件名
    public string m_AssetName = "";
    //该资源所在AB包
    public string m_ABName = "";
    //该资源依赖
    public List<string> m_DependAssetBundles = null;
    //该资源加载完的AB包
    public AssetBundle m_AssetBundle = null;

}
