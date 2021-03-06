using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LoadResPrority
{
    RES_HIGHT = 0,
    RES_MIDDLE,
    RES_SLOW,
    RES_NUM,
}

public class ResouceObj
{
    public uint m_Crc;
    //是否跳场清除
    public bool m_bClear;
    //实例化出来的GameObject
    public GameObject m_ClondObj;
    public ResourceItem m_ResItem;
    //是否已经放回对象池
    public bool m_Already = false;
    //储存异步加载的GUID
    public long m_Guid = 0;

    //---------------------------------
    //是否放到场景节点下面
    public bool m_SetSceneParent = false;
    public OnAsysncObjFinish m_DealFinish = null;
    public object m_param1 = null, m_param2 = null, m_param3 = null;
    //离线数据
    public OfflineData m_OfflineData = null;
    public void Reset()
    {
        m_Crc = 0;
        m_bClear = false;
        m_ClondObj = null;
        m_ResItem = null;
        m_Already = false;
        m_DealFinish=null;
        m_param1 = null;
        m_param2 = null;
        m_param3 = null;
        m_OfflineData = null;
    }
}

public class AsyncLoadResParm
{
    public List<AsysncCallBack> m_callbackList = new List<AsysncCallBack> ();
    public uint m_Crc;
    public string m_Path;
    public bool m_Sprite = false;
    public LoadResPrority m_Prority = LoadResPrority.RES_SLOW;

    public void Reset()
    {
        m_callbackList.Clear();
        m_Crc = 0;
        m_Path = "";
        m_Sprite = false;
        m_Prority = LoadResPrority.RES_SLOW;

    }
}

public class AsysncCallBack
{
    public OnAsysncFinish m_DealFinish = null;
    public ResouceObj m_ResObj = null;
    //------------------------------------
    //加载完成的回调
    public OnAsysncObjFinish m_DealObjFinish = null;
    //回调参数
    public object m_param1 = null,m_param2 = null, m_param3 = null;
    public void Reset()
    {
        m_DealObjFinish=null; 
        m_param1=null; 
        m_param2=null;
        m_param3=null;

        m_DealFinish = null;
        m_ResObj = null;
    }
}

public delegate void OnAsysncObjFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null);

public delegate void OnAsysncFinish(string path, ResouceObj obj, object param1 = null, object param2 = null, object param3 = null);
public class ResourceManager : Singleton<ResourceManager>
{
    protected long m_Guid = 0;

    public bool m_loadFromAssetBundle = true;
    //缓存引用计数为零的资源列表，达到缓存最大的时候释放列表里面最早没用的资源
    protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();
    //缓存使用的资源列表
    public Dictionary<uint,ResourceItem> m_AssetDic { get; set; } = new Dictionary<uint,ResourceItem>();

    protected MonoBehaviour m_Startmono;
    //正在异步加载的资源列表
    protected List<AsyncLoadResParm>[] m_loadingAssetList = new List<AsyncLoadResParm>[(int)(LoadResPrority.RES_NUM)];
    //正在异步加载的Dic
    protected Dictionary<uint, AsyncLoadResParm> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResParm>();

    protected ClassObjectPool<AsyncLoadResParm> m_AsyncLoadResParmPool = new ClassObjectPool<AsyncLoadResParm>(50);
    protected ClassObjectPool<AsysncCallBack> m_AsysncCallBackPool = new ClassObjectPool<AsysncCallBack>(100);

    public long MAXLOADERSTIME = 20000;


    public void Init(MonoBehaviour mono)
    {
        for (int i = 0; i < (int)LoadResPrority.RES_NUM; i++)
        {
            m_loadingAssetList[i] = new List<AsyncLoadResParm>();
        }
        m_Startmono = mono;
        m_Startmono.StartCoroutine(AsyncLoadCor());
    }

    public long CreateGuid() 
    {
        return m_Guid++;
    }

    void CacheResource(string path,ref ResourceItem item,uint crc,Object obj, int addrefcount = 1)
    {
        //
        if(item == null ){
            Debug.Log("ResourceItem is null,path"+path);
        }

        if (obj == null) {
            Debug.Log("ResourceLoad Fail:" + path);
        }

        item.m_Crc = crc;
        item.m_Obj = obj;
        item.Refcount += addrefcount;
        item.m_lastUseTime = Time.realtimeSinceStartup;
        item.m_Guid = obj.GetInstanceID();
        ResourceItem oldItem = null;
        if (m_AssetDic.TryGetValue(crc, out oldItem))
        {
            m_AssetDic[crc] = item;
        }
        else
        {
            m_AssetDic.Add(crc, item);
        }
    }
    public void ClearCache()
    {
        List<ResourceItem> tempList = new List<ResourceItem> ();
        foreach (ResourceItem item in m_AssetDic.Values)
        {
            if(item.m_isClear)
            {
                tempList.Add(item);
            }
        }
        foreach (ResourceItem item in tempList)
        {
            DestoryResourceItem(item, true);
        }
        tempList.Clear();
    }

    /// <summary>
    /// 取消异步加载
    /// </summary>
    /// <returns></returns>
    public bool CancerLoad(ResouceObj res)
    {
        AsyncLoadResParm parm = null;
        if(m_LoadingAssetDic.TryGetValue (res.m_Crc, out parm) && m_loadingAssetList[(int)parm.m_Prority].Contains(parm))
        {
            for (int i = parm.m_callbackList.Count; i>0; i--)
            {
                AsysncCallBack tempCallBack = parm.m_callbackList[i];
                if (tempCallBack != null&& res == tempCallBack.m_ResObj)
                {
                    tempCallBack.Reset();
                    m_AsysncCallBackPool.Recycle(tempCallBack);
                    parm.m_callbackList.Remove(tempCallBack); 
                }
            }

            if (parm.m_callbackList.Count <= 0)
            {
                parm.Reset();
                m_loadingAssetList[(int)parm.m_Prority].Remove(parm);
                m_AsyncLoadResParmPool.Recycle(parm);
                m_LoadingAssetDic.Remove(res.m_Crc);
                return true;
            }
        }
        return false;

    }

    /// <summary>
    /// 预加载：加裁后卸载到缓存中，不清理缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    public void preLoadResource(string path) 
    {
        if (string.IsNullOrEmpty(path))
        {
            return ;

        }
        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            return;
        }
        Object obj = null;
#if UNITY_EDITOR
        if (!m_loadFromAssetBundle)
        {

            item = AssetBundleManager.Instance.FindResourceItem(crc);
            if (item.m_Obj != null)
            {
                obj = item.m_Obj ;
            }
            else
            {
                obj = loadAssetByEditor<Object>(path);
            }

        }
#endif

        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }

        CacheResource(path, ref item, crc, obj);
        item.m_isClear = false;
        ReleaseRsouce(path, false);
    }


    public T loadResource<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;

        }
        uint crc =  CRC32.GetCRC32(path);      
        ResourceItem item = GetCacheResourceItem(crc);
        if(item != null)
        {
            return item.m_Obj as T;
        }
        T obj = null;
#if UNITY_EDITOR
        if (!m_loadFromAssetBundle)
        {
           
            item = AssetBundleManager.Instance.FindResourceItem(crc);
            if (item.m_Obj != null)
            {
                obj = item.m_Obj as T;
            }
            else
            {
                obj = loadAssetByEditor<T>(path);
            }

        }
#endif

        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if(item != null && item.m_AssetBundle != null)
            {
                if(item.m_Obj != null)
                {
                    obj = item.m_Obj as T;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }                
            }
        }

        CacheResource(path,ref item,crc,obj);
        return obj;
    }

    /// <summary>
    /// 同步加载，针对给ObjeManager的接口
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj"></param>
    /// <returns></returns>
    public ResouceObj LoadResource(string path,ResouceObj resObj)
    {
        if(resObj == null)
        {
            return null;
        }

        uint crc = resObj.m_Crc==0?CRC32.GetCRC32(path):resObj.m_Crc; ;
        ResourceItem item = GetCacheResourceItem(crc);
        if(item != null)
        {
            resObj.m_ResItem = item;
            return resObj;
        }

        Object obj = null;
#if UNITY_EDITOR
        if (!m_loadFromAssetBundle)
        {            
            item = AssetBundleManager.Instance.FindResourceItem(resObj.m_Crc);
            if (item.m_Obj != null)
            {
                obj = item.m_Obj as Object;
            }
            else
            {
                obj = loadAssetByEditor<Object>(path);
            }
        }
#endif

        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj as Object;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }

        CacheResource(path,ref item, crc,obj);
        resObj.m_ResItem = item;
        item.m_isClear = resObj.m_bClear;
        return resObj;
    }

#if UNITY_EDITOR
    protected T loadAssetByEditor<T>(string path)where T : UnityEngine.Object
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif

    /// <summary>
    /// 通过obj增加引用次数
    /// </summary>
    /// <param name="resObj"></param>
    /// <param name="cnt"></param>
    /// <returns></returns>
    public int InCreaseResourceRef(ResouceObj resObj,int cnt = 1)
    {
        return resObj != null? InCreaseResourceRef(resObj.m_Crc,cnt):0;
    }

    /// <summary>
    /// 通过crc增加引用次数
    /// </summary>
    /// <param name="crc"></param>
    /// <param name="cnt"></param>
    /// <returns></returns>
    public int InCreaseResourceRef(uint crc = 0, int cnt = 1)
    {
        ResourceItem item = null;
        if(!m_AssetDic.TryGetValue(crc, out item) || item == null)
        {
            return 0;
        }

        item.Refcount += cnt;
        item.m_lastUseTime = Time.realtimeSinceStartup;
        return item.Refcount;
    }

   /// <summary>
   /// 根据resObj减少引用次数
   /// </summary>
   /// <param name="resObj"></param>
   /// <param name="cnt"></param>
   /// <returns></returns>
    public int DecreaseResourceRef(ResouceObj resObj,int cnt = 1)
    {
        return resObj != null ? DecreaseResourceRef(resObj.m_Crc, cnt) : 0;
    }

    /// <summary>
    /// 根据crc减少引用次数
    /// </summary>
    /// <param name="crc"></param>
    /// <param name="cnt"></param>
    /// <returns></returns>
    public int DecreaseResourceRef(uint crc, int cnt = 1)
    {
        ResourceItem item = null;
        if(m_AssetDic.TryGetValue((uint)crc, out item) && item == null)
        {
            return 0;
        }
        item.Refcount -= cnt;
        return item.Refcount;
    }


    /// <summary>
    /// 根据ResouceObj卸载资源
    /// </summary>
    /// <param name="resObj"></param>
    /// <param name="destroyObj"></param>
    /// <returns></returns>
    public bool ReleaseResource(ResouceObj resObj, bool destroyObj = false)
    {
        if (resObj == null) return false;
        ResourceItem item = null;

        if (!m_AssetDic.TryGetValue(resObj.m_ResItem.m_Crc, out item) || item == null)
        {
            Debug.LogError("AssetDic 里不存在该资源：" + resObj.m_ClondObj.name + "可能释放了多次");
        }
        GameObject.Destroy(resObj.m_ClondObj);
        item.Refcount--;
        DestoryResourceItem(item, destroyObj);
        return true;
    }

    /// <summary>
    /// /// 不需要实例化的资源的卸载，根据path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destroyObj"></param>
    /// <returns></returns>
    public bool ReleaseRsouce(string path, bool destroyObj = false)
    {
        if (string.IsNullOrEmpty(path)) return false;

        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = null;

        if(!m_AssetDic.TryGetValue(crc, out item) || item == null)
        {
            Debug.LogError("AssetDic 里不存在该资源：" +path + "可能释放了多次");
        }
        item.Refcount--;
        DestoryResourceItem(item, destroyObj);
        return true;
    }


    /// <summary>
    /// 不需要实例化的资源的卸载，根据obj
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="destroyObj"></param>
    /// <returns></returns>

    public bool ReleaseRsouce(Object obj, bool destroyObj = false)
    {
        if (obj == null) return false;
        ResourceItem item = null;
        foreach (ResourceItem vo in m_AssetDic.Values)
        {
            if(vo.m_Guid == obj.GetInstanceID())
            {
                item = vo;
            }
        }

        if(item == null)
        {
            Debug.LogError("AssetDic 里不存在该资源：" + obj.name + "可能释放了多次");
            return false;
        }
        item.Refcount--;
        DestoryResourceItem(item,destroyObj);
        return true;

    }

    /// <summary>
    /// 获取缓存区数据
    /// </summary>
    /// <param name="crc"></param>
    /// <param name="addrefcount"></param>
    /// <returns></returns>
    ResourceItem GetCacheResourceItem(uint crc,int addrefcount = 1)
    {
        ResourceItem item = null;
        if(m_AssetDic.TryGetValue(crc, out item))
        {
            if(item != null)
            {
                item.Refcount += addrefcount;
                item.m_lastUseTime = Time.realtimeSinceStartup;
                
            }
        }
        return item;
    }
    
    protected void WashOut()
    {
        //当当前内存大于80%时，清除最早没有使用的资源
    }

    /// <summary>
    /// 回收一个资源
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destoryCache"></param>
    protected void DestoryResourceItem(ResourceItem item,bool destoryCache = false)
    {
        if(item == null || item.Refcount > 0)
        {
            return;
        }

        if (!destoryCache)
        {
            //m_NoRefrenceAssetMapList.InsertToHead(item);
            return;
        }

        if (!m_AssetDic.Remove(item.m_Crc))
        {
            return;
        }
        //释放AB包的引用
        AssetBundleManager.Instance.ReleaseAsset(item);
        //清空资源对应的对象池
        ObjectManager.Instance.ClearPoolObject(item.m_Crc);
        if(item.m_Obj != null)
        {
            item.m_Obj = null;
#if UNITY_EDITOR
            Resources.UnloadUnusedAssets();
#endif
            
        }
    }

    public void AsysLoadResource(string path,OnAsysncObjFinish dealFinish,LoadResPrority prority,object param1 = null,object param2 = null,object param3 = null,uint crc = 0)
    {
        if(crc == 0)
        {
            crc = CRC32.GetCRC32(path);
        }
        ResourceItem item = GetCacheResourceItem(crc);
        if(item != null)
        {
            if(dealFinish != null)
            {
                dealFinish(path,item.m_Obj,param1,param2,param3);
            }
            return;
        }

        //判断是否正在加载
        AsyncLoadResParm parm = null;
        if (!m_LoadingAssetDic.TryGetValue(crc, out parm) && parm == null)
        {
            parm = m_AsyncLoadResParmPool.Spawn(true);
            parm.m_Crc = crc; 
            parm.m_Path = path;
            parm.m_Prority = prority;
            m_LoadingAssetDic.Add(crc,parm);
            m_loadingAssetList[(int)prority].Add(parm);
        }

        AsysncCallBack callBack = m_AsysncCallBackPool.Spawn(true);
        callBack.m_DealObjFinish = dealFinish;
        callBack.m_param1 = param1; 
        callBack.m_param2 = param2;
        callBack.m_param3 = param3;
        parm.m_callbackList.Add(callBack);
    }


    /// <summary>
    /// 针对ObjecteManager的异步加载接口
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj"></param>
    /// <param name="dealFinish"></param>
    /// <param name="prority"></param>
    public void AsyncLoadResource(string path,ResouceObj resObj,OnAsysncFinish dealFinish,LoadResPrority prority)
    {
        ResourceItem item = GetCacheResourceItem(resObj.m_Crc);
        if(item != null)
        {
            resObj.m_ResItem = item;
            if(dealFinish != null)
            {
                dealFinish(path,resObj);
            }
            return;
        }
        AsyncLoadResParm parm = null;
        if(!m_LoadingAssetDic.TryGetValue(resObj.m_Crc,out parm) || parm == null)
        {
            parm = m_AsyncLoadResParmPool.Spawn(true);
            parm.m_Crc = resObj.m_Crc;
            parm.m_Path = path;
            parm.m_Prority = prority;
            m_LoadingAssetDic.Add(resObj.m_Crc,parm);
            m_loadingAssetList[(int) prority].Add(parm); 
        }

        //往回调列表里面添加回调
        AsysncCallBack callBack = m_AsysncCallBackPool.Spawn(true);
        callBack.m_DealFinish = dealFinish;
        callBack.m_ResObj = resObj;
        parm.m_callbackList.Add(callBack);
    }

    /// <summary>
    /// 协程异步加载
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncLoadCor()
    {
        List<AsysncCallBack> callBackList = null;
        long lastYiledTime = System.DateTime.Now.Ticks;
        while (true)
        {
            bool isHaveYield = false;
            for (int i = 0; i < (int) LoadResPrority.RES_NUM; i++)
            {
                List<AsyncLoadResParm> loadingList = m_loadingAssetList[i];
                if (loadingList.Count <= 0) continue;
                AsyncLoadResParm loadingItem = loadingList[0];
                loadingList.RemoveAt(0);
                callBackList = loadingItem.m_callbackList;
                Object obj = null;
                ResourceItem item = null;

#if UNITY_EDITOR
                if (!m_loadFromAssetBundle)
                {
                    obj = loadAssetByEditor<Object>(loadingItem.m_Path);
                    item = AssetBundleManager.Instance.FindResourceItem(loadingItem.m_Crc);
                }
#endif
                if(obj == null)
                {
                    item = AssetBundleManager.Instance.LoadResourceAssetBundle(loadingItem.m_Crc);
                    if(item != null && item.m_AssetBundle != null)
                    {
                        AssetBundleRequest assetBundleRequest = null;
                        if (loadingItem.m_Sprite)
                        {
                            assetBundleRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
                        }
                        else
                        {
                            assetBundleRequest = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
                        }
                         
                        yield return assetBundleRequest;
                        if (assetBundleRequest.isDone)
                        {
                            obj = assetBundleRequest.asset;
                        }
                        lastYiledTime = System.DateTime.Now.Ticks;
                    }

                }

                CacheResource(loadingItem.m_Path,ref item,loadingItem.m_Crc,obj,callBackList.Count);
                for (int j = 0; j < callBackList.Count; j++)
                {
                    AsysncCallBack callBack = callBackList[j];
                    if (callBack != null && callBack.m_DealFinish != null && callBack.m_ResObj != null)
                    {
                        ResouceObj tempResObj = callBack.m_ResObj;
                        tempResObj.m_ResItem = item;
                        callBack.m_DealFinish(loadingItem.m_Path, tempResObj, tempResObj.m_param1, tempResObj.m_param2, tempResObj.m_param3);
                        callBack.m_DealFinish = null;
                        tempResObj = null;

                    }

                    if (callBack != null && callBack.m_DealObjFinish != null)
                    {
                        callBack.m_DealObjFinish(loadingItem.m_Path, obj, callBack.m_param1, callBack.m_param2, callBack.m_param3);
                        callBack.m_DealObjFinish = null;
                    }

                    callBack.Reset();
                    m_AsysncCallBackPool.Recycle(callBack);                        
                }

                obj = null;
                callBackList.Clear();
                m_LoadingAssetDic.Remove(loadingItem.m_Crc);
                loadingItem.Reset();
                m_AsyncLoadResParmPool.Recycle(loadingItem);
                if(System.DateTime.Now.Ticks-lastYiledTime> MAXLOADERSTIME)
                {
                    yield return null;
                    lastYiledTime = System.DateTime.Now.Ticks;
                    isHaveYield = true;
                }
            }

            if (isHaveYield || System.DateTime.Now.Ticks - lastYiledTime > MAXLOADERSTIME)
            {
                lastYiledTime = System.DateTime.Now.Ticks;
                yield return null;
            }

        }
    }
    
}

public class DoubleLinkListNode<T> where T : class,new ()
{
    public DoubleLinkListNode<T> prev = null;
    public DoubleLinkListNode<T> next = null;
    public T t = null;
}

public class DoubleLinkList<T> where T : class,new ()
{
    public DoubleLinkListNode<T> Head = null;
    public DoubleLinkListNode<T> Taild = null;
    protected ClassObjectPool<DoubleLinkListNode<T>> m_DoubleLinkNodePool = ObjectManager.Instance.getOrCreateClassPool<DoubleLinkListNode<T>>(500);
    protected int m_Count = 0;
    public int Count
    {
        get { return m_Count; }
    }

    public DoubleLinkListNode<T> AddToHeader(T t)
    {
        DoubleLinkListNode<T> node = m_DoubleLinkNodePool.Spawn(true);
        //重置对像池对像
        node.prev = null;
        node.next = null;
        return AddToHeader(node);
    }

    public DoubleLinkListNode<T> AddToHeader(DoubleLinkListNode<T> node )
    {
        if(node == null) return null;
        node.prev = null;
        if (Head == null)
        {
            Head = Taild = node;
        }
        else
        {
            node.next = Head;
            Head.prev = node;
            Head = node;
        }
        m_Count++;
        return node;
    }

    public DoubleLinkListNode<T> AddToTail(T t)
    {
        DoubleLinkListNode<T> node = m_DoubleLinkNodePool.Spawn(true);
        node.prev = null;
        node.next = null;
        return AddToTail(node);
    }

    public DoubleLinkListNode<T> AddToTail(DoubleLinkListNode<T> node)
    {
       if(node==null) return null;
        if (Taild == null)
        {
            Head = Taild = node;
        }
        else
        {
            node.prev = Taild;
            Taild.next = node;
            Taild = node;
        }
        m_Count++;
        return node;
    }

    /// <summary>
    /// 移除某节点
    /// </summary>
    /// <param name="node"></param>
    public void RemoveNode(DoubleLinkListNode<T> node)
    {
        if (node == null) return;
        if(node == Head)
        {
            Head = node.next;
        }
        if(node == Taild)
        {
            Taild = node.prev;
        }

        if(node.prev != null)
        {
            node.prev.next = node.next;
        }
        if(node.next != null)
        {
            node.next.prev = node.prev;
        }

        node.prev = node.next = null;
        node.t = null;
        m_DoubleLinkNodePool.Recycle(node);
        m_Count--;
    }
    /// <summary>
    /// 把节点移到头部
    /// </summary>
    /// <param name="node"></param>

    public void MoveToHead(DoubleLinkListNode<T> node)
    {
        if(node == null || node == Head) return;
        if(node.prev == null || node.next == null) return;
        if(node == Taild)
        {
            Taild = node.prev;

        }
        if(node.prev != null)
        {
            node.prev.next = node.next;
        }
        if(node.next != null)
        {
            node.next.prev = node.prev;
        }

        node.prev = null;
        node.next = Head;
        Head.prev = node;
        Head = node;
        if(Taild == null)
        {
            Taild = Head;
        }

    }

}

public class CMapList<T> where T : class,new ()
{
    DoubleLinkList<T> m_DLink = new DoubleLinkList<T>();
    Dictionary<T,DoubleLinkListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkListNode<T>> ();

    ~CMapList()
    {

    }

    public void Clear()
    {
        while(m_DLink.Taild != null)
        {
            Remove(m_DLink.Taild.t);
        }

    }

    /// <summary>
    /// 插入一个节点到表头
    /// </summary>
    /// <param name="t"></param>
    public void InsertToHead(T t)
    {
        DoubleLinkListNode<T> node = null;
        if(m_FindMap.TryGetValue(t, out node) && node!= null)
        {
            m_DLink.AddToHeader(node);
            return;
        }
        m_DLink.AddToHeader(t);
        m_FindMap.Add(t,m_DLink.Head);
    }

    /// <summary>
    /// 从链表尾部弹出一个节点
    /// </summary>
    public void Pop()
    {
        if(m_DLink.Taild != null)
        {
            Remove(m_DLink.Taild.t);
        }
    }

    /// <summary>
    /// 移除节点
    /// </summary>
    /// <param name="t"></param>
    public void Remove(T t)
    {
        DoubleLinkListNode<T> node = null;
        if(!m_FindMap.TryGetValue(t,out node) || node == null)
        {
            return;
        }
        m_DLink.RemoveNode(node); 
        m_FindMap.Remove(t);
    }

    /// <summary>
    /// 获取尾部节点
    /// </summary>
    /// <returns></returns>
    public T Back()
    {
        return m_DLink.Taild == null ? null : m_DLink.Taild.t;
    }

    public int Size()
    {
        return m_FindMap.Count;
    }

    /// <summary>
    /// 查找是否存在该节点
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool isFind(T t)
    {
        DoubleLinkListNode<T> node = null;
        if(!m_FindMap.TryGetValue((T)t,out node) || node == null)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 刷新某个节点，把节点移动到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Reflesh(T t)
    {
        DoubleLinkListNode<T> node = null;
        if(!m_FindMap.TryGetValue(t,out node)|| node == null)
        {
            return false;
        }
        m_DLink.MoveToHead(node);
        return true;

    }
}

