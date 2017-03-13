
using System;

namespace DayEasy.Models.Open.System
{
    public class MManifestDto : DDto
    {
        public int VersionCode { get; set; }

        /// <summary> 版本号 </summary>
        public string Version { get; set; }

        /// <summary> 更新内容 </summary>
        public string UpgradeInstructions { get; set; }

        /// <summary> 下载链接 </summary>
        public string DownloadUrl { get; set; }

        /// <summary> 更新包大小 </summary>
        public string ApkSize { get; set; }

        /// <summary> 是否强制更新 </summary>
        public bool Mandatory { get; set; }

        /// <summary> 文件MD5码 </summary>
        public Guid Md5 { get; set; }

        /// <summary> 发布时间 </summary>
        public string UpgradeDate { get; set; }
    }
}
