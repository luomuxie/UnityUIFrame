using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectManager : Singleton<ObjectManager>
{
    
    public Transform RecyclePoolTrs;
    public Transform SceneTrs;
    //�����
    protected Dictionary<uint, List<ResouceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResouceObj>>();    
    //�ݴ�ResObj��Dic
    protected Dictionary<int,ResouceObj> m_ResourceObjDic = new Dictionary<int, ResouceObj>();
    //ResourceObj��������
    protected ClassObjectPool<ResouceObj> m_ResourceObjClassPool;
    //�����첽��guid����ResourceObj,���ж��Ƿ������첽����
    protected Dictionary<long, ResouceObj> m_AysncResObjs = new Dictionary<long, ResouceObj>();

    public void Init(Transform recycleTrs,Transform sceneTrs)
    {
        RecyclePoolTrs = recycleTrs;
        SceneTrs = sceneTrs;
        m_ResourceObjClassPool = ObjectManager.Instance.getOrCreateClassPool<ResouceObj>(1000);
    }
    /// <summary>
    /// ��ն����
    /// </summary>
    public void ClearCache()
    {
        List<uint> tempList = new List<uint>();
        foreach (uint key in m_ObjectPoolDic.Keys)
        {
            List<ResouceObj> st = m_ObjectPoolDic[key];
            for (int i = st.Count; i >= 0; i--)
            {
                ResouceObj resObj = st[i];
                if(!System.Object.ReferenceEquals(resObj, null) && resObj.m_bClear)
                {
                    GameObject.Destroy(resObj.m_ClondObj);
                    m_ResourceObjDic.Remove(resObj.m_ClondObj.GetInstanceID());
                    resObj.Reset();
                    m_ResourceObjClassPool.Recycle(resObj);
                }
            }

            if (st.Count <= 0)
            {
                tempList.Add(key);
            }
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            uint temp = tempList[i];
            if (m_ObjectPoolDic.ContainsKey(temp))
            {
                m_ObjectPoolDic.Remove(tempList[i]);
            }            
        }
        tempList.Clear();
    }

    /// <summary>
    /// ���ĳ��Դ�ڶ�����е���������
    /// </summary>
    /// <param name="crc"></param>
    public void ClearPoolObject(uint crc)
    {
        List<ResouceObj> st = null;
        if (!m_ObjectPoolDic.TryGetValue(crc, out st) || st == null)
        {
            return;
        }

        for (int i = st.Count-1; i >=0; i++)
        {
            ResouceObj resouceObj = st[i];
            if (resouceObj.m_bClear)
            {
                st.Remove(resouceObj);
                int tempID = resouceObj.m_ClondObj.GetInstanceID();
                GameObject.Destroy(resouceObj.m_ClondObj);
                resouceObj.Reset();
                m_ResourceObjDic.Remove(tempID);
                m_ResourceObjClassPool.Recycle(resouceObj);
            }
        }
        if (st.Count <= 0)
        {
            m_ObjectPoolDic.Remove(crc);
        }
        st.Clear();
        
    }
    /// <summary>
    /// �Ӷ������ȡ�ö���
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    protected ResouceObj GetObjectFromPool(uint crc)
    {
        List<ResouceObj> st = null;
        if((m_ObjectPoolDic.TryGetValue(crc, out st) || st!= null)&& st.Count > 0)
        {
            ResourceManager.Instance.InCreaseResourceRef(crc);
            ResouceObj resObj = st[0]; 
            st.RemoveAt(0);
            GameObject obj = resObj.m_ClondObj;
            if(!System.Object.ReferenceEquals(obj, null))
            {
#if UNITY_EDITOR

                if (obj.name.EndsWith("(Recycle)"))
                {
                    obj.name = obj.name.Replace("(Recycle)", "");
                }
#endif
            }

            return resObj;
        }
        return null;
    }

    public void CanceLoad(long guid)
    {
        ResouceObj resouceObj = null;
        if(m_AysncResObjs.TryGetValue(guid, out resouceObj) && ResourceManager.Instance.CancerLoad(resouceObj))
        {
            m_AysncResObjs.Remove(guid);
            m_ResourceObjClassPool.Recycle(resouceObj);
        }
    }

    /// <summary>
    /// �Ƿ����������
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public bool IsingAsyncLoad(long guid)
    {
        return m_AysncResObjs[guid] != null;
    }

    /// <summary>
    /// �ö����Ƿ����ش���
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool IsObjectManagerCreate(GameObject obj)
    {
        ResouceObj resObj = m_ResourceObjDic[obj.GetInstanceID()];
        return  resObj != null;
    }


    /// <summary>
    /// Ԥ����
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cnt"></param>
    /// <param name="clear"></param>

    public void preloadGameObject(string path,int cnt = 1,bool clear = false)
    {
        List<GameObject> tempGameObjecteList = new List<GameObject>();
        for (int i = 0; i < cnt; i++)
        {
            GameObject obj = InstantiateObject(path, false, clear);
            tempGameObjecteList.Add(obj);
        }

        for (int i = 0; i < cnt; i++)
        {
            GameObject obj = tempGameObjecteList[i];
            ReleaseObject(obj); ;
            obj = null;
        }
        tempGameObjecteList.Clear();
    }

    /// <summary>
    /// ͬ������
    /// </summary>
    /// <param name="path"></param>
    /// <param name="bClear"></param>
    /// <returns></returns>
    public GameObject InstantiateObject(string path,bool setSceneObj = false, bool bClear = true)
    {
        uint crc = CRC32.GetCRC32(path);
        ResouceObj resouceObj = GetObjectFromPool(crc);
        if(resouceObj == null)
        {
            resouceObj = m_ResourceObjClassPool.Spawn(true);
            resouceObj.m_Crc = crc;
            resouceObj.m_bClear = bClear;
            //ResourceManager�ṩ���ط���
            ResourceManager.Instance.InCreaseResourceRef(resouceObj);
            resouceObj = ResourceManager.Instance.LoadResource(path,resouceObj);
            if(resouceObj.m_ResItem.m_Obj != null)
            {
                resouceObj.m_ClondObj = GameObject.Instantiate(resouceObj.m_ResItem.m_Obj) as GameObject;
            }
        }
        if (setSceneObj)
        {
            resouceObj.m_ClondObj.transform.SetParent(SceneTrs, false);            
        }
        int tempID = resouceObj.m_ClondObj.GetInstanceID();
        if (!m_ResourceObjDic.ContainsKey(tempID))
        {
            m_ResourceObjDic.Add(tempID, resouceObj);
        }

        resouceObj.m_Already = false;
        return resouceObj.m_ClondObj;
    }


    public long InstantiateObjecteAsync(string path, OnAsysncObjFinish dealFinish, LoadResPrority prority, bool setSceneObject = false, object param1 = null, object param2 = null, object param3 = null,bool bClear = true)
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }
        uint crc = CRC32.GetCRC32(path); 
        ResouceObj resObj = GetObjectFromPool(crc);
        if (resObj != null)
        {
            if (setSceneObject)
            {
                resObj.m_ClondObj.transform.SetParent(SceneTrs,false);
            }
            if(dealFinish != null)
            {
                dealFinish(path,resObj.m_ClondObj,param1,param2,param3);
            }
            return 0;
        }
        long guid = ResourceManager.Instance.CreateGuid();
        resObj = m_ResourceObjClassPool.Spawn(true);
        resObj.m_Crc = crc;
        resObj.m_SetSceneParent = setSceneObject;
        resObj.m_bClear = bClear;
        resObj.m_DealFinish = dealFinish;
        resObj.m_param1 = param1;
        resObj.m_param2 = param2;
        resObj.m_param3 = param3;
        resObj.m_Guid = guid;
        //�����첽������Dic
        m_AysncResObjs.Add(guid, resObj);
        //����resourceMangage���첽���ؽӿ�
        ResourceManager.Instance.AsyncLoadResource(path, resObj, OnLoadResourceObjFinish, prority);
        return guid;
    }

    /// <summary>
    /// ��Դ������ɻص�
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    void OnLoadResourceObjFinish(string path,ResouceObj resObj,object param1 = null, object param2 = null,object param3 = null)
    {
        if (resObj == null) return;
        if (resObj.m_ResItem.m_Obj == null)
        {
#if UNITY_EDITOR
            Debug.Log("�첽��Դ���ص���ԴΪ��"+path);
#endif
        }
        else
        {
            resObj.m_ClondObj = GameObject.Instantiate(resObj.m_ResItem.m_Obj) as GameObject;
        }

        //������ɣ������ڼ��ص��첽���Ƴ�
        if (m_AysncResObjs.ContainsKey(resObj.m_Guid))
        {
            m_AysncResObjs.Remove(resObj.m_Guid);
        }
        if (resObj.m_ClondObj != null && resObj.m_SetSceneParent)
        {
            resObj.m_ClondObj.transform.SetParent(SceneTrs,false);
        }

        if(resObj.m_DealFinish != null)
        {
            int tempID = resObj.m_ClondObj.GetInstanceID();
            if (!m_ResourceObjDic.ContainsKey(tempID))
            {
                m_ResourceObjDic.Add(tempID, resObj);
            }
            resObj.m_DealFinish(path,resObj.m_ClondObj,resObj.m_param1,resObj.m_param2,resObj.m_param3);
        }
    }

    /// <summary>
    /// ������Դ
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="maxCachCnt"></param>
    /// <param name="destoryCache"></param>
    /// <param name="recycleParent"></param>
    public void ReleaseObject(GameObject obj, int maxCachCnt = -1, bool destoryCache = false,bool recycleParent = true)
    {
        if (obj == null) return;
        ResouceObj resObj = null;
        int tempID = obj.GetInstanceID();
        if(!m_ResourceObjDic.TryGetValue(tempID,out resObj))
        {
            Debug.Log(obj.name + "������ObjeManager�����ģ�");
        }
        if(resObj == null)
        {
            Debug.LogError("�����ResouceObj Ϊ�գ�");
        }
        if (resObj.m_Already)
        {
            Debug.LogError("�ö����Ѿ��Żض�����ˣ�����Լ��Ƿ�����");
            return;
        }
#if UNITY_EDITOR
        obj.name += "(Recycle)";
#endif
        if(maxCachCnt == 0)
        {
            //���Żض����
            m_ResourceObjDic.Remove(tempID);
            ResourceManager.Instance.ReleaseResource(resObj,destoryCache);
            resObj.Reset();
            m_ResourceObjClassPool.Recycle(resObj);
        }
        else
        {
            //���յ������
            List<ResouceObj> st = null;
            if(!m_ObjectPoolDic.TryGetValue(resObj.m_Crc, out st) || st == null)
            {
                st = new List<ResouceObj>();
                m_ObjectPoolDic.Add(resObj.m_Crc,st); 
            }
            if (resObj.m_ClondObj)
            {
                if (recycleParent)
                {
                    resObj.m_ClondObj.transform.SetParent(RecyclePoolTrs);
                }
                else
                {
                    resObj.m_ClondObj.SetActive(false);
                }
            }
            if (maxCachCnt<0 || st.Count < maxCachCnt)
            {
                st.Add(resObj);
                resObj.m_Already = true;
                ResourceManager.Instance.DecreaseResourceRef(resObj);
            }
            else
            {
                m_ResourceObjDic.Remove(tempID) ;
                ResourceManager.Instance.ReleaseResource(resObj, destoryCache);
                resObj.Reset();
                m_ResourceObjClassPool.Recycle(resObj);
            }
        }
    }


    protected Dictionary<Type, object> m_classPoolDic = new Dictionary<Type, object>();

    public ClassObjectPool<T> getOrCreateClassPool<T>(int cnt) where T:class,new()
    {
        Type type = typeof(T);
        
        if(!m_classPoolDic.TryGetValue(type, out var outObj) || outObj == null)
        {
            ClassObjectPool<T> newPool = new ClassObjectPool<T>(cnt);
            m_classPoolDic.Add(type, newPool);
            return newPool;
        }
        return outObj as ClassObjectPool<T>;
    }

}
