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
    }

    private void Start()
    {
        
        m_clip = ResourceManager.Instance.loaadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
        m_audio.clip = m_clip;
        m_audio.Play();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A)){
            m_audio.Stop();
            ResourceManager.Instance.ReleaseRsouce(m_clip,true);
            m_clip = null;
        }
    }

}
