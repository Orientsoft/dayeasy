using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DayEasy.Utility.Config;

namespace DayEasy.Portal.Services.Config
{
    [Serializable]
    [XmlRoot("impression")]
    [FileName("impression.config")]
    public class ImpressionConfig : ConfigBase
    {
        [XmlArray("categories")]
        [XmlArrayItem("category")]
        public List<ImpressionCategory> Categories { get; set; }
    }

    [Serializable]
    public class ImpressionCategory
    {
        [XmlAttribute("role")]
        public byte Role { get; set; }

        [XmlArray("wants")]
        [XmlArrayItem("item")]
        public List<string> WantList { get; set; }

        [XmlArray("status")]
        [XmlArrayItem("item")]
        public List<string> StatusList { get; set; }

        [XmlArray("features")]
        [XmlArrayItem("item")]
        public List<string> FeatureList { get; set; }
    }
}