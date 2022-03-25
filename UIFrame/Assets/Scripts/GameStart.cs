using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{

    public AudioSource m_audio;
    private AudioClip m_clip;
    private GameObject m_obj;
    private void Awake()
    {
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        Transform trs = transform.Find("RecylePoolTrs");
        Transform sceneTrs = transform.Find("SceneTrs");
        ObjectManager.Instance.Init(trs, sceneTrs); 
    }

    private void Start()
    {

        // m_clip = ResourceManager.Instance.loaadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");//同步加裁
        //m_audio.clip = m_clip;
        //m_audio.Play();
        //ResourceManager.Instance.AsysLoadResource("Assets/GameData/Sounds/senlin.mp3",onLoadFinish,LoadResPrority.RES_MIDDLE);//异步加载
        //ResourceManager.Instance.preLoadResource("Assets/GameData/Sounds/senlin.mp3");
        //m_obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab",true);
        ObjectManager.Instance.InstantiateObjecteAsync("Assets/GameData/Prefabs/Attack.prefab", onLoadFinish, LoadResPrority.RES_HIGHT, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)){
            // m_audio.Stop();
            // ResourceManager.Instance.ReleaseRsouce(m_clip);
            // m_clip = null;
            //m_clip = ResourceManager.Instance.loadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
            //m_audio.clip = m_clip;
            //m_audio.Play();
            ObjectManager.Instance.ReleaseObject(m_obj);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            //ResourceManager.Instance.ReleaseRsouce(m_clip,true);
            // m_clip = null;
            //m_audio.clip = null;
            m_obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ObjectManager.Instance.ReleaseObject(m_obj,0,true);
        }
    }

    void onLoadFinish(string path,Object obj,object parm1,object parm2,object parm3)
    {
        /*
        m_clip = obj as AudioClip;
        m_audio.clip = m_clip;
        m_audio.Play();
        */
        m_obj = obj as GameObject;
    }

    void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
#endif
    }

}
