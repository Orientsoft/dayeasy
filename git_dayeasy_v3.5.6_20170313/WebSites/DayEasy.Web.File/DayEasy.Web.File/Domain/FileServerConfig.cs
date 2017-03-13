using DayEasy.Utility.Config;
using System;
using System.Xml.Serialization;

namespace DayEasy.Web.File.Domain
{
    [Serializable]
    [XmlRoot("root")]
    [FileName("file_server.config")]
    public class FileServerConfig : ConfigBase
    {
        /// <summary> 站点 </summary>
        [XmlElement("site")]
        public string Site { get; set; }

        /// <summary> 默认图片 </summary>
        [XmlElement("defaultImage")]
        public string DefaultImage { get; set; }

        /// <summary> 单目录最多文件数量 </summary>
        [XmlElement("maxFileCount")]
        public int MaxFileCount { get; set; }

        [XmlElement("saveType")]
        public int SaveType { get; set; }

        /// <summary> 图片文件 </summary>
        [XmlElement("image")]
        public FileTypeLimit Image { get; set; }

        /// <summary> 视频文件 </summary>
        [XmlElement("video")]
        public FileTypeLimit Video { get; set; }

        /// <summary> 音频文件 </summary>
        [XmlElement("audio")]
        public FileTypeLimit Audio { get; set; }
        /// <summary> 应用文件 </summary>
        [XmlElement("app")]
        public FileTypeLimit App { get; set; }

        /// <summary> 附件 </summary>
        [XmlElement("attach")]
        public FileTypeLimit Attach { get; set; }
    }

    [Serializable]
    public class FileTypeLimit
    {
        /// <summary> 允许的扩展名列表 </summary>
        [XmlIgnore]
        public string[] Exts { get; set; }

        [XmlAttribute("exts")]
        public string ExtsAttr
        {
            get { return string.Join(";", Exts); }
            set { Exts = string.IsNullOrWhiteSpace(value) ? new string[] { } : value.Split(';'); }
        }

        /// <summary> 上传大小限制(kb) </summary>
        [XmlAttribute("size")]
        public int MaxSize { get; set; }
    }
}