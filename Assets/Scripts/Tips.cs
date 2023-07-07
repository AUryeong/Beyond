using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "Tip", menuName = "CodeA", order = 0)]
public class Tips : ScriptableObject
{
    public List<Tip> tips;
}

[System.Serializable]
public class Tip
{
    [XmlAttribute("Id")]
    public string id;
    
    [XmlElement("Lore")]
    public string lore;
}