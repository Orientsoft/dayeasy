using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DayEasy.Utility.Config;

namespace DayEasy.Core.Config
{

    /// <summary> 推荐图片 </summary>
    [XmlRoot("root")]
    [FileName("recommend_image.config")]
    [Serializable]
    public class RecommendImageConfig : ConfigBase
    {
        /// <summary> 推荐图片列表 </summary>
        [XmlArray("recommends"), XmlArrayItem("recommend")]
        public List<RecommendImage> Recommends { get; set; }
    }

    /// <summary> 推荐图片列表 </summary>
    [Serializable]
    public class RecommendImage
    {
        /// <summary> 图片类型 </summary>
        [XmlAttribute("type")]
        public RecommendImageType Type { get; set; }

        /// <summary> 图片列表 </summary>
        [XmlArray("images"), XmlArrayItem("item")]
        public List<RecommendImageItem> Images { get; set; }
    }

    /// <summary> 推荐图片实体 </summary>
    public class RecommendImageItem
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary> 标题 </summary>
        [XmlAttribute("title")]
        public string Title { get; set; }

        /// <summary> 排序 </summary>
        [XmlAttribute("sort")]
        public int Sort { get; set; }

        /// <summary> 预览图 </summary>
        [XmlElement("preview")]
        public string PreviewUrl { get; set; }

        /// <summary> 图片url </summary>
        [XmlElement("image")]
        public string ImageUrl { get; set; }
    }

    /// <summary> 推荐图片类型 </summary>
    public enum RecommendImageType
    {
        /// <summary> 用户头像 </summary>
        UserAvatar = 0,

        /// <summary> 圈子Logo </summary>
        GroupLogo = 1,

        /// <summary> 圈子Banner图 </summary>
        GroupBanner = 2,
        /// <summary> 机构Logo </summary>
        AgencyLogo = 3
    }
}
