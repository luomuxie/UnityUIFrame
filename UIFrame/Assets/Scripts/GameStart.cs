using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{

    public AudioSource m_audio;
    private AudioClip m_clip;
    private void Awake()
    {
       AssetBundleManager.Instance.LoadAssetBundleConfig();
       ResourceManager.Instance.Init(this);
    }

    private void Start()
    {

        // m_clip = ResourceManager.Instance.loaadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");//同步加裁
        //m_audio.clip = m_clip;
        //m_audio.Play();
        //ResourceManager.Instance.AsysLoadResource("Assets/GameData/Sounds/senlin.mp3",onLoadFinish,LoadResPrority.RES_MIDDLE);//异步加载
        ResourceManager.Instance.preLoadResource("Assets/GameData/Sounds/senlin.mp3");

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A)){
            // m_audio.Stop();
            // ResourceManager.Instance.ReleaseRsouce(m_clip);
            // m_clip = null;
            m_clip = ResourceManager.Instance.loadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
            m_audio.clip = m_clip;
            m_audio.Play();
        }
        else if (Input.GetKey(KeyCode.D))
        {
             ResourceManager.Instance.ReleaseRsouce(m_clip,true);
             m_clip = null;
            m_audio.clip = null;
        }
    }

    void onLoadFinish(string path,Object obj,object parm1,object parm2,object parm3)
    {
        m_clip = obj as AudioClip;
        m_audio.clip = m_clip;
        m_audio.Play();
    }

    void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
#endif
    }

}
