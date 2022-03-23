using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>
{

    public bool m_loadFromAssetBundle = true;
    //�������ü���Ϊ�����Դ�б��ﵽ��������ʱ���ͷ��б���������û�õ���Դ
    protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();
    //����ʹ�õ���Դ�б�
    public Dictionary<uint,ResourceItem> m_AssetDic { get; set; } = new Dictionary<uint,ResourceItem>();

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

    public T loaadResource<T>(string path) where T : UnityEngine.Object
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
#if UNITY_EDITOR
    protected T loadAssetByEditor<T>(string path)where T : UnityEngine.Object
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif
    /// <summary>
    /// ����Ҫʵ��������Դ��ж��
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
            Debug.LogError("AssetDic �ﲻ���ڸ���Դ��" + obj.name + "�����ͷ��˶��");
            return false;
        }
        item.Refcount--;
        DestoryResourceItem(item,destroyObj);
        return true;

    }

    /// <summary>
    /// ��ȡ����������
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
        //����ǰ�ڴ����80%ʱ���������û��ʹ�õ���Դ
    }

    /// <summary>
    /// ����һ����Դ
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destoryCache"></param>
    protected void DestoryResourceItem(ResourceItem item,bool destoryCache = false)
    {
        if(item == null || item.Refcount > 0)
        {
            return;
        }

        if (!m_AssetDic.Remove(item.m_Crc))
        {
            return;
        }

        if (!destoryCache)
        {
            m_NoRefrenceAssetMapList.InsertToHead(item);
            return;
        }


        AssetBundleManager.Instance.ReleaseAsset(item);
        if(item.m_Obj != null)
        {
            item.m_Obj = null;
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
        //���ö���ض���
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
    /// �Ƴ�ĳ�ڵ�
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
    /// �ѽڵ��Ƶ�ͷ��
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
    /// ����һ���ڵ㵽��ͷ
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
    /// ������β������һ���ڵ�
    /// </summary>
    public void Pop()
    {
        if(m_DLink.Taild != null)
        {
            Remove(m_DLink.Taild.t);
        }
    }

    /// <summary>
    /// �Ƴ��ڵ�
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
    /// ��ȡβ���ڵ�
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
    /// �����Ƿ���ڸýڵ�
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
    /// ˢ��ĳ���ڵ㣬�ѽڵ��ƶ���ͷ��
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

