using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DayEasy.Utility.Config;

namespace DayEasy.Web.Api.Config
{
    /// <summary> 合作商户配置 </summary>
    [Serializable]
    [XmlRoot("root")]
    [FileName("partners.config")]
    public class PartnerXml : ConfigBase
    {
        [XmlArray("partners")]
        [XmlArrayItem("item")]
        public List<PartnerInfo> Partners { get; set; }
    }

    [Serializable]
    public class PartnerInfo
    {
        [XmlAttribute("key")]
        public string AppKey { get; set; }
        [XmlAttribute("secret")]
        public string AppSecret { get; set; }
        [XmlAttribute("comefrom")]
        public int Comefrom { get; set; }
    }
}
