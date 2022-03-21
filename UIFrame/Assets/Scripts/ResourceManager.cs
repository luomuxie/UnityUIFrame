using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>
{
    
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


}

