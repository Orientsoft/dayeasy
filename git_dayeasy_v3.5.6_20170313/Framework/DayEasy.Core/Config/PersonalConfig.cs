using System;
using System.Xml.Serialization;
using DayEasy.Utility.Config;

namespace DayEasy.Core.Config
{
    [Serializable]
    [XmlRoot("configs")]
    [FileName("personal_pictures.config")]
    public class PersonalConfig : ConfigBase
    {
        [XmlArray("students")]
        [XmlArrayItem("item")]
        public string[] Students { get; set; }
        [XmlArray("teachers")]
        [XmlArrayItem("item")]
        public string[] Teachers { get; set; }
    }
}
