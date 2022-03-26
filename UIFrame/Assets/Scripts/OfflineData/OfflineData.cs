using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineData:MonoBehaviour
{
    public Rigidbody m_Rigidbody;
    public Collider m_Collider;
    public Transform[] m_AllPoint;
    public int[] m_AllPointChildCnt;
    public bool[] m_allPointActive;
    public Vector3[] m_Pos;
    public Vector3[] m_Scale;
    public Quaternion[] m_Rot;

    public virtual void ResetProp()
    {
        int allPointCnt = m_AllPoint.Length;
        for (int i = 0; i < allPointCnt; i++)
        {
            Transform tempTrs = m_AllPoint[i];
            if (tempTrs != null)
            {
                tempTrs.localPosition = m_Pos[i];
                tempTrs.localRotation = m_Rot[i];
                tempTrs.localScale = m_Scale[i];
                if (m_allPointActive[i])
                {
                    if (!tempTrs.gameObject.activeSelf)
                    {
                        tempTrs.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (tempTrs.gameObject.activeSelf)
                    {
                        tempTrs.gameObject.SetActive(false);
                    }
                }

                if (tempTrs.childCount > m_AllPointChildCnt[i])
                {
                    int childCnt = tempTrs.childCount;
                    for (int j = m_AllPointChildCnt[i];j< childCnt; j++)
                    {
                        GameObject tempObj = tempTrs.GetChild(j).gameObject;
                        if (!ObjectManager.Instance.IsObjectManagerCreate(tempObj))
                        {
                            GameObject.Destroy(tempObj);
                        }

                    }
                }
            }
        }
    }

    /// <summary>
    /// 编辑器下保存初始数据
    /// </summary>
    public virtual void BindData()
    {
        m_Collider = gameObject.GetComponentInChildren<Collider>(true);
        m_Rigidbody = gameObject.GetComponentInChildren<Rigidbody>(true);
        m_AllPoint = gameObject.GetComponentsInChildren<Transform>(true);
        int allPointCnt = m_AllPoint.Length;
        m_AllPointChildCnt = new int[allPointCnt];
        m_allPointActive = new bool[allPointCnt];
        m_Pos = new Vector3[allPointCnt];
        m_Rot = new Quaternion[allPointCnt];
        m_Scale = new Vector3[allPointCnt];
        for (int i = 0; i < allPointCnt; i++)
        {
            Transform temp = m_AllPoint[i] as Transform;
            m_AllPointChildCnt[i] = temp.childCount;
            m_allPointActive[i] = temp.gameObject.activeSelf;
            m_Rot[i] = temp.rotation;
            m_Pos[i] = temp.position;
            m_Scale[i] = temp.localScale;
        }
        
    }
}
