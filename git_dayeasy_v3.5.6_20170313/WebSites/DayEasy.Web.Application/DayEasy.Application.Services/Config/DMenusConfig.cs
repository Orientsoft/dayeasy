using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DayEasy.Utility.Config;

namespace DayEasy.Application.Services.Config
{
    [Serializable]
    [FileName("d_menus.config")]
    [XmlRoot("menus")]
    public class DMenusConfig : ConfigBase
    {
        [XmlArray("groups")]
        [XmlArrayItem("group")]
        public MenuGroup[] Groups { get; set; }

        public DMenusConfig()
        {
            Groups = new MenuGroup[] { };
        }
    }

    [Serializable]
    public class MenuGroup : MenuItem
    {
        public MenuGroup()
        {
            Menus = new List<MenuItem>();
        }
        [XmlArray("menus")]
        [XmlArrayItem("menu")]
        public List<MenuItem> Menus { get; set; }
    }

    [Serializable]
    public class MenuItem
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("url")]
        public string Url { get; set; }

        [XmlAttribute("icon")]
        public string Icon { get; set; }

        [XmlAttribute("info")]
        public string Info { get; set; }

        [XmlAttribute("permission")]
        public long Permission { get; set; }
    }
}
