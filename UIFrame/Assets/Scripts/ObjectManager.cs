using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectManager : Singleton<ObjectManager>
{
    
    public Transform RecyclePoolTrs;
    public Transform SceneTrs;
    protected Dictionary<uint, List<ResouceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResouceObj>>();
    protected ClassObjectPool<ResouceObj> m_ResourceObjClassPool = ObjectManager.Instance.getOrCreateClassPool<ResouceObj>(1000);
    protected Dictionary<int,ResouceObj> m_ResourceObjDic = new Dictionary<int, ResouceObj>();

    public void Init(Transform recycleTrs,Transform sceneTrs)
    {
        RecyclePoolTrs = recycleTrs;
        SceneTrs = sceneTrs;
    }
    /// <summary>
    /// 从对像池中取得对像
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    protected ResouceObj GetObjectFromPool(uint crc)
    {
        List<ResouceObj> st = null;
        if(m_ObjectPoolDic.TryGetValue(crc, out st) || st!= null&& st.Count > 0)
        {
            //引用计数待添加
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

    /// <summary>
    /// 同步加载
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
            //ResourceManager提供加载方法
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

        resouceObj.m_Already = true;
        return resouceObj.m_ClondObj;
    }

    /// <summary>
    /// 回收资源
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
            Debug.Log(obj.name + "对像不是ObjeManager创建的！");
        }
        if(resObj == null)
        {
            Debug.LogError("缓存的ResouceObj 为空！");
        }
        if (resObj.m_Already)
        {
            Debug.LogError("该对象已经放回对像池了，检测自己是否引用");
            return;
        }
#if UNITY_EDITOR
        obj.name += "(Recycle)";
#endif
        if(maxCachCnt == 0)
        {
            //不放回对像池
            m_ResourceObjDic.Remove(tempID);
            ResourceManager.Instance.ReleaseResource(resObj,destoryCache);
            resObj.Reset();
            m_ResourceObjClassPool.Recycle(resObj);
        }
        else
        {
            //回收到对像池
            List<ResouceObj> st = null;
            if(m_ObjectPoolDic.TryGetValue(resObj.m_Crc, out st) || st == null)
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
            //感觉这样写不太对呀maxCachCnt<0
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
