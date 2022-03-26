using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOfflineData : OfflineData
{
    public Vector2[] m_AnchorMax;
    public Vector2[] m_AnchorMin;
    public Vector2[] m_Pivot;
    public Vector2[] m_sizeDelta;
    public Vector3[] m_AnchoredPos;
    public ParticleSystem[] m_Particles;

    public override void ResetProp()
    {
        int allPointCnt = m_AllPoint.Length;
        for (int i = 0; i < allPointCnt; i++)
        {
            RectTransform tempTrs =  m_AllPoint[i] as RectTransform;
            if(tempTrs != null)
            {
                tempTrs.localPosition = m_Pos[i];
                tempTrs.localRotation = m_Rot[i];
                tempTrs.localScale = m_Scale[i];
                tempTrs.anchorMax = m_AnchorMax[i];
                tempTrs.anchorMin = m_AnchorMin[i];
                tempTrs.pivot = m_Pivot[i];
                tempTrs.sizeDelta = m_sizeDelta[i];
                tempTrs.anchoredPosition3D = m_AnchoredPos[i];
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
                    for (int j = m_AllPointChildCnt[i]; j < childCnt; j++)
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
        int particleCnt = m_Particles.Length;
        for (int i = 0; i < particleCnt; i++)
        {
            m_Particles[i].Clear(true);
            m_Particles[i].Play();
        }
    }

    public override void BindData()
    {
        Transform[] allTrs = gameObject.GetComponentsInChildren<Transform>();
        int allTrsCnt = allTrs.Length;
        for (int i = 0; i < allTrsCnt; i++)
        {
            if(allTrs[i] is RectTransform)
            {
                allTrs[i].gameObject.AddComponent<RectTransform>();
            }
        }

        m_AllPoint = gameObject.GetComponentsInChildren<RectTransform>();
        m_Particles = gameObject.GetComponentsInChildren<ParticleSystem>();
        int allPointCnt = m_AllPoint.Length;
        m_AllPointChildCnt = new int[allPointCnt];
        m_allPointActive = new bool[allPointCnt];
        m_Pos = new Vector3[allPointCnt];
        m_Rot = new Quaternion[allPointCnt];
        m_Scale = new Vector3[allPointCnt];
        m_Pivot = new Vector2[allPointCnt];
        m_AnchorMin = new Vector2[allPointCnt];
        m_AnchorMax = new Vector2[allPointCnt];
        m_sizeDelta = new Vector2[allPointCnt];
        m_AnchoredPos = new Vector3[allPointCnt];
        for (int i = 0;i < allPointCnt; i++)
        {
            RectTransform temp = m_AllPoint[i] as RectTransform;
            m_AllPointChildCnt[i] = temp.childCount;
            m_allPointActive[i] = temp.gameObject.activeSelf;
            m_Rot[i] = temp.rotation;
            m_Pos[i] = temp.position;
            m_Scale[i] = temp.localScale;

            m_Pivot[i] = temp.pivot;
            m_AnchorMax[i] = temp.anchorMax;
            m_AnchorMin[i] = temp.anchorMin;
            m_sizeDelta[i] = temp.sizeDelta;
            m_AnchoredPos[i] = temp.anchoredPosition3D;

        }             
    }
}
