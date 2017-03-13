
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using DayEasy.Utility.Config;

namespace DayEasy.Core.Config
{
    /// <summary> 特殊帐号权限配置 </summary>
    [XmlRoot("root")]
    [FileName("special_account.config")]
    [Serializable]
    public class SpecialAccountConfig : ConfigBase
    {
        /// <summary> 特殊帐号列表 </summary>
        [XmlArray("specials"), XmlArrayItem("item")]
        public List<SpecialAccount> SpecialAccounts { get; set; }

        /// <summary> Constructor </summary>
        public SpecialAccountConfig()
        {
            SpecialAccounts = new List<SpecialAccount>();
        }
    }

    /// <summary> 特殊帐号 </summary>
    [Serializable]
    public class SpecialAccount
    {
        /// <summary> 帐号类型 </summary>
        [XmlAttribute("type")]
        public SpecialAccountType Type { get; set; }

        /// <summary> 用户得一号列表字符 </summary>
        [XmlElement("userCodes")]
        public string UserCodeString
        {
            get { return UserCodes == null ? string.Empty : string.Join(",", UserCodes); }
            set { UserCodes = string.IsNullOrWhiteSpace(value) ? new List<string>() : value.Split(',').ToList(); }
        }

        /// <summary> 用户得一号列表 </summary>
        [XmlIgnore]
        public List<string> UserCodes { get; set; }
    }

    /// <summary> 特殊帐号类型 </summary>
    public enum SpecialAccountType
    {
        /// <summary> 编辑试卷题目 </summary>
        [Description("编辑试卷题目")]
        EditPaperQuestion = 0,

        /// <summary> 创建分享圈 </summary>
        [Description("创建分享圈")]
        CreateShareGroup = 1,

        /// <summary> 扫描管理员 </summary>
        [Description("扫描管理员")]
        ScannerManager = 2
    }
}
