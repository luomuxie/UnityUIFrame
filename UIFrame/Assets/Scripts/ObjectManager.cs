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

        return resouceObj.m_ClondObj;
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
