using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="CreateABConfig")]
public class ABConfig : ScriptableObject
{
    public List<string> m_allPrefebPath = new List<string>();
    public List<FileDirName> m_allFileDirName = new List<FileDirName>();

    [System.Serializable]
    public struct FileDirName
    {
        public string ABname;
        public string Path;
    }
         
}
