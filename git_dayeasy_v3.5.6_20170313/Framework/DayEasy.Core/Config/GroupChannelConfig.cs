

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DayEasy.Utility.Config;

namespace DayEasy.Core.Config
{
    /// <summary> 分享圈频道配置 </summary>
    [XmlRoot("root")]
    [FileName("group_channel.config")]
    [Serializable]
    public class GroupChannelConfig : ConfigBase
    {
        [XmlArray("channels"), XmlArrayItem("item")]
        public List<GroupChannel> Channels { get; set; }

        public GroupChannelConfig()
        {
            Channels = new List<GroupChannel>();
        }
    }

    /// <summary> 圈子频道 </summary>
    [Serializable]
    public class GroupChannel
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        /// <summary> 父级Id </summary>
        [XmlAttribute("pid")]
        public int ParentId { get; set; }

        /// <summary> 排序 </summary>
        [XmlAttribute("sort")]
        public int Sort { get; set; }

        /// <summary> 频道级别 </summary>
        [XmlAttribute("level")]
        public int Level { get; set; }

        /// <summary> 是否叶子级 </summary>
        [XmlAttribute("isLeaf")]
        public bool IsLeaf { get; set; }

        /// <summary> 频道名称 </summary>
        [XmlText]
        public string Name { get; set; }

        /// <summary> 描述 </summary>
        [XmlAttribute("desc")]
        public string Desc { get; set; }
    }
}
