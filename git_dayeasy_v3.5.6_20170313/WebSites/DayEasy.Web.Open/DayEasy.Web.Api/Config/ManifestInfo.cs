using System;
using System.ComponentModel;
using System.Xml.Serialization;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;

namespace DayEasy.Web.Api.Config
{
    /// <summary> 版本信息配置 </summary>
    [Serializable]
    [XmlRoot("root")]
    public class ManifestInfo : ConfigBase
    {
        /// <summary>
        /// 版本标识
        /// </summary>
        [XmlElement("version_code")]
        public int VersionCode { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        [XmlElement("version")]
        public string Version { get; set; }

        /// <summary>
        /// 更新内容
        /// </summary>
        [XmlElement("upgrade_instructions")]
        public string UpgradeInstructions { get; set; }

        /// <summary>
        /// 下载链接
        /// </summary>
        [XmlElement("download_url")]
        public string DownloadUrl { get; set; }

        /// <summary>
        /// 更新包大小
        /// </summary>
        [XmlElement("apk_size")]
        public string ApkSize { get; set; }

        /// <summary>
        /// 是否强制更新
        /// </summary>
        [XmlElement("mandatory")]
        public bool Mandatory { get; set; }

        /// <summary>
        /// 文件MD5码
        /// </summary>
        [XmlElement("md5")]
        public Guid Md5 { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        [XmlElement("upgrade_date")]
        public string UpgradeDate { get; set; }

        private static string GetFileName(ManifestType type)
        {
            return string.Format("{0}.config", type.GetText());
        }

        public static ManifestInfo Get(int type)
        {
            var manifestType = (ManifestType)type;
            return ConfigUtils<ManifestInfo>.Instance.Get(GetFileName(manifestType));
        }

        public void Save(int type)
        {
            ConfigUtils<ManifestInfo>.Instance.Set(this, GetFileName((ManifestType)type));
        }

        public void Save(ManifestType type)
        {
            ConfigUtils<ManifestInfo>.Instance.Set(this, GetFileName(type));
        }
    }

    [Flags]
    public enum ManifestType
    {
        /// <summary> 我的得一 </summary>
        [Description("android_dayeasy")]
        AndroidDayeasy = 10,

        /// <summary> 阅卷工具 </summary>
        [Description("marking_tool")]
        MarkingTool = 20,

        /// <summary> 电子课本 </summary>
        [Description("android_ebook")]
        AndroidEBook = 30,
    }
}