using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectManager : Singleton<ObjectManager>
{
    
    public Transform RecyclePoolTrs;
    public Transform SceneTrs;
    protected Dictionary<uint, List<ResouceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResouceObj>>();
    protected ClassObjectPool<ResouceObj> m_ResourceObjClassPool;
    protected Dictionary<int,ResouceObj> m_ResourceObjDic = new Dictionary<int, ResouceObj>();

    public void Init(Transform recycleTrs,Transform sceneTrs)
    {
        RecyclePoolTrs = recycleTrs;
        SceneTrs = sceneTrs;
        m_ResourceObjClassPool = ObjectManager.Instance.getOrCreateClassPool<ResouceObj>(1000);
    }
    /// <summary>
    /// 从对像池中取得对像
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

        resouceObj.m_Already = false;
        return resouceObj.m_ClondObj;
    }


    public void InstantiateObjecteAsync(string path, OnAsysncObjFinish dealFinish, LoadResPrority prority, bool setSceneObject = false, object param1 = null, object param2 = null, object param3 = null,bool bClear = true)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
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
            return;
        }

        resObj = m_ResourceObjClassPool.Spawn(true);
        resObj.m_Crc = crc;
        resObj.m_SetSceneParent = setSceneObject;
        resObj.m_bClear = bClear;
        resObj.m_DealFinish = dealFinish;
        resObj.m_param1 = param1;
        resObj.m_param2 = param2;
        resObj.m_param3 = param3;
        //调用resourceMangage的异步加载接口
        ResourceManager.Instance.AsyncLoadResource(path, resObj, OnLoadResourceObjFinish, prority);
    }

    /// <summary>
    /// 资源加载完成回调
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
            Debug.Log("异步资源加载的资源为空"+path);
#endif
        }
        else
        {
            resObj.m_ClondObj = GameObject.Instantiate(resObj.m_ResItem.m_Obj) as GameObject;
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
