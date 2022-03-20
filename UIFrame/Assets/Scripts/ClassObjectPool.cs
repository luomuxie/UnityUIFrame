using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassObjectPool<T> where T : class ,new()
{ 
    protected Stack<T> m_pool = new Stack<T>();
    protected int m_MaxCnt = 0;
    protected int m_NorecycleCnt = 0;

    public ClassObjectPool(int maxCnt)
    {
        m_MaxCnt = maxCnt;
        for (int i = 0; i < maxCnt; i++)
        {
            m_pool.Push(new T());
        }
    }

    /// <summary>
    /// 取出对像池对像
    /// </summary>
    /// <param name="isCreatIfPoolEmpty"></param>
    /// <returns></returns>
    public T Spawn(bool isCreatIfPoolEmpty)
    {
        if(m_pool.Count > 0)
        {
            T tempClass = m_pool.Pop();
            if(tempClass == null)
            {
                if (isCreatIfPoolEmpty)
                {
                    tempClass = new T(); 
                }
            }
            m_NorecycleCnt++;
            return tempClass;
        }
        else
        {
            if (isCreatIfPoolEmpty)
            {
                m_NorecycleCnt++;
                return  new T();
            }
        }
        return null;
    }

    /// <summary>
    /// 回收类对像
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool Recycle(T obj)
    {
        if(obj == null) return false;
        m_NorecycleCnt--;
        if(m_pool.Count>=m_MaxCnt && m_MaxCnt > 0)
        {
            obj = null;
            return false;
        }
        m_pool.Push(obj);

        return true;
    }
}
