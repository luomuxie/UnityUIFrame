using System.Collections.Generic;
using System.Xml.Serialization;

[System.Serializable]
public class AssetBundleConfig 
{
    [XmlElement("ABList")]

    public List<ABBase> ABList { get; set; }
}

[System.Serializable]
public class ABBase
{
    [XmlAttribute("path")]
    public string Path { set; get; }
    [XmlAttribute("Crc")]
    public uint Crc { set; get; }

    [XmlAttribute("ABname")]
    public string ABname { set; get; }

    [XmlAttribute("AssetName")]
    public string AssetName { set; get; }
    [XmlElement("ABDependce")]
    public List<string> ABDependce { set; get; }

}
