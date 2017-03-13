using System;
using System.Xml.Serialization;
using DayEasy.Utility.Config;

namespace DayEasy.Web.ManageMent.Config
{
    [Serializable]
    [XmlRoot("configs")]
    [FileName("home_pictures.config")]
    public class HomeConfig : ConfigBase
    {
        [XmlArray("coverages")]
        [XmlArrayItem("item")]
        public string[] Coverages { get; set; }
        [XmlArray("resources")]
        [XmlArrayItem("item")]
        public string[] Resources { get; set; }

        [XmlElement("example")]
        public ExampleConfig Examples { get; set; }

        public HomeConfig()
        {
            Coverages = new string[] { };
            Resources = new string[] { };
            Examples = new ExampleConfig();
        }
    }

    [Serializable]
    public class ExampleConfig
    {
        [XmlArray("right")]
        [XmlArrayItem("item")]
        public string[] Right { get; set; }

        [XmlArray("left")]
        [XmlArrayItem("item")]
        public string[] Left { get; set; }

        public ExampleConfig()
        {
            Left = new string[] { };
            Right = new string[] { };
        }
    }
}