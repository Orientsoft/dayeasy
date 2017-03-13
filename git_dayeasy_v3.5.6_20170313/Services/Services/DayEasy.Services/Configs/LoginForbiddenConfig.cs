using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DayEasy.Utility.Config;

namespace DayEasy.Services.Configs
{
    [Serializable]
    [FileName("loginForbidden.config")]
    [XmlRoot("root")]
    public class LoginForbiddenConfig : ConfigBase
    {
        [XmlArray("forbidden"), XmlArrayItem("item")]
        public List<LoginForbiddenItem> Forbiddens { get; set; }

        [XmlElement("words")]
        public string TipWords { get; set; }

        public LoginForbiddenConfig()
        {
            Forbiddens = new List<LoginForbiddenItem>();
        }
    }

    [Serializable]
    public class LoginForbiddenItem
    {
        [XmlArray("agency"), XmlArrayItem("item")]
        public List<string> AgencyIds { get; set; }

        [XmlArray("user"), XmlArrayItem("item")]
        public List<long> UserIds { get; set; }

        [XmlElement("start")]
        public DateTime Start { get; set; }

        [XmlElement("end")]
        public DateTime End { get; set; }

        public LoginForbiddenItem()
        {
            AgencyIds = new List<string>();
            UserIds = new List<long>();
        }
    }
}
