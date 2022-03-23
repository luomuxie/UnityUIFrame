using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectManager : Singleton<ObjectManager>
{
    //public ClassObjectPool<ResourceTest> test = ObjectManager.Instance.getOrCreateClassPool<ResourceTest>(100);


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
