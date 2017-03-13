using DayEasy.Utility.Config;
using System;
using System.Xml.Serialization;

namespace DayEasy.Web.ManageMent.Config
{
    [Serializable]
    [XmlRoot("configs")]
    [FileName("index.config")]
    public class IndexConfig : ConfigBase
    {
        [XmlArray("carousels")]
        [XmlArrayItem("item")]
        public string[] Carousels { get; set; }

        [XmlArray("fixeds")]
        [XmlArrayItem("item")]
        public string[] Fixeds { get; set; }

        [XmlArray("sections")]
        [XmlArrayItem("section")]
        public Section[] Sections { get; set; }
    }

    [Serializable]
    public class Section
    {
        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlArray("sources")]
        [XmlArrayItem("item")]
        public string[] Sources { get; set; }

        [XmlArray("tabs")]
        [XmlArrayItem("tab")]
        public Tab[] Tabs { get; set; }
    }

    [Serializable]
    public class Tab
    {
        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlArray("sources")]
        [XmlArrayItem("item")]
        public string[] Sources { get; set; }

        [XmlArray("adverts")]
        [XmlArrayItem("item")]
        public string[] Adverts { get; set; }

        [XmlArray("groups")]
        [XmlArrayItem("group")]
        public Group[] Groups { get; set; }
    }

    [Serializable]
    public class Group
    {
        [XmlAttribute("type")]
        public int Type { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }
    }
}