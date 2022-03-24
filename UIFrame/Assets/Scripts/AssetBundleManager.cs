using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    public Dictionary<uint, ResourceItem> m_ResourceItemDic = new Dictionary<uint, ResourceItem>();
    public Dictionary<uint,AssetBundleItem> m_AssetBundleDic = new Dictionary<uint,AssetBundleItem>();
    public ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.getOrCreateClassPool<AssetBundleItem>(100);
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

    /// <summary>
    /// 根据名字加载单assetbundle
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private AssetBundle LoadAssetBundle(string name)
    {
        uint crc = CRC32.GetCRC32(name);
        AssetBundleItem item;
        if(!m_AssetBundleDic.TryGetValue(crc,out item))
        {
            AssetBundle bundle = null;
            string fullPath = Application.streamingAssetsPath + "/" + name;
            if (File.Exists(fullPath))
            {
                bundle = AssetBundle.LoadFromFile(fullPath);
            }
            if (bundle == null)
            {
                Debug.Log("Load AssetBundle Error:" + fullPath);
            }

            item = m_AssetBundleItemPool.Spawn(true);
            item.m_AssetBundle = bundle;
            m_AssetBundleDic.Add(crc, item);
        
        }
        item.m_RefCnt++;
        return item.m_AssetBundle;
    }


    /// <summary>
    /// 根据crc查找ResourceItem
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem FindResourceItem(uint crc)
    {
        return m_ResourceItemDic[crc];
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="item"></param>
    public void ReleaseAsset(ResourceItem item)
    {
        if (item == null) return;
        if(item.m_DependAssetBundles != null && item.m_DependAssetBundles.Count > 0)
        {
            for (int i = 0; i < item.m_DependAssetBundles.Count; i++)
            {
                UnLoadAssetBundle(item.m_DependAssetBundles[i]);
            }
        }
        UnLoadAssetBundle(item.m_ABName); 
    }

    private void UnLoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.GetCRC32(name);
        if(m_AssetBundleDic.TryGetValue(crc,out item) && item != null)
        {
            item.m_RefCnt--;
            if(item.m_RefCnt<=0 && item.m_AssetBundle != null)
            {
                item.m_AssetBundle.Unload(true);
                item.Rest();
                m_AssetBundleItemPool.Recycle(item);
                m_AssetBundleDic.Remove(crc);

            }
        }
    }


}

public class AssetBundleItem
{
    public AssetBundle m_AssetBundle;
    public int m_RefCnt;
    public void Rest()
    {
        m_AssetBundle = null;
        m_RefCnt = 0;
    }
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

    //----------------------------------------------------

    //资源对像
    public Object m_Obj = null;

    //资源唯一标识
    public int m_Guid = 0;
    //资源最后所使用的时间
    public float m_lastUseTime = 0.0f;
    //引用计数
    protected int m_RefCount = 0;

    public bool m_isClear = true;
    public int Refcount
    {
        get { return m_RefCount; }
        set {
            m_RefCount = value;
            if(m_RefCount < 0)
            {
                Debug.LogError("rfcount<0"+m_RefCount+","+(m_Obj!= null? m_Obj.name:"name is null"));
            }
        }
    }

}
