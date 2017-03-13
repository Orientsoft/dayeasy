
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DayEasy.Core.Config;
using DayEasy.Utility;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;

namespace DayEasy.Core
{
    /// <summary> 静态变量 </summary>
    public static class Consts
    {
        /// <summary> 版本号 </summary>
        public const string Version = "3.5.5";

        /// <summary> 用户登录Cookie </summary>
        public const string UserCookieName = "__dayeasy_u";

        /// <summary> 登录错误次数 </summary>
        public const string LoginCountCookieName = "__dayeasy_err_c";

        public const string ContextCachekKey = "__context_key_nd3dc";

        public const string SubjectCacheKey = "__subject_key_gl9ed";
        public const string SubjectFormulaCacheKey = "__subject__formula_key_gl9ed";
        public const string QuestionTypeCacheKey = "__question_type_key_ab342";
        public const string SubjectQuestionTypeCacheKey = "__subject_question_type_key_ab342";
        public const string WebsiteCacheKey = "__website_key_knd98";

        const string AvatarImg = "/image/default/user.jpg";

        /// <summary> 站点 </summary>
        public static string Website { get; set; }

        /// <summary> 程序集查找 </summary>
        public static readonly Func<Assembly, bool> AssemblyFinder =
            t => t.FullName.StartsWith("dayeasy.", StringComparison.CurrentCultureIgnoreCase);

        public static DayEasyConfig Config
        {
            get { return ConfigUtils<DayEasyConfig>.Instance.Get(); }
        }

        #region 频道
        public static List<GroupChannel> Channels
        {
            get
            {
                var config = ConfigUtils<GroupChannelConfig>.Instance.Get();
                if (config == null)
                    return new List<GroupChannel>();
                return config.Channels ?? new List<GroupChannel>();
            }
        }

        public static List<GroupChannel> LeafChannels
        {
            get { return Channels.Where(t => t.IsLeaf).ToList(); }
        }

        public static string Channel(int id)
        {
            var channel = Channels.FirstOrDefault(t => t.Id == id);
            if (channel == null) return string.Empty;
            return channel.Name;
        }

        public static List<GroupChannel> GetChannels(int id)
        {
            var type = id / 100;

            return Channels.Where(t => t.Id >= type * 100 && t.Id < (type + 1) * 100).ToList();
        }
        #endregion

        public static List<SpecialAccount> SpecialAccountList
        {
            get
            {
                var config = ConfigUtils<SpecialAccountConfig>.Instance.Get();
                return config == null ? new List<SpecialAccount>() : config.SpecialAccounts;
            }
        }

        public static List<string> SpecialCodes(SpecialAccountType type)
        {
            var special = SpecialAccountList.FirstOrDefault(t => t.Type == type);
            //没有则所有人都可以
            if (special == null)
                return null;
            return special.UserCodes ?? new List<string>();
        }

        public static bool HasSpecialAuth(this string code, SpecialAccountType type)
        {
            var codes = SpecialCodes(type);
            return codes == null || codes.Contains(code);
        }

        public static string DefaultAvatar(RecommendImageType type = RecommendImageType.UserAvatar)
        {
            var def = string.Concat(Config.FileSite, AvatarImg);
            var config = ConfigUtils<RecommendImageConfig>.Config;
            if (config == null)
                return def;
            var images = config.Recommends.FirstOrDefault(t => t.Type == type);
            if (images == null || images.Images.IsNullOrEmpty())
                return def;
            return images.Images.First().ImageUrl;
        }

        /// <summary> 消息模版 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MessageTemplate Template(int type)
        {
            if (Config == null) return new MessageTemplate();
            return Config.MessageTemplates.FirstOrDefault(t => t.MessageType == type) ?? new MessageTemplate();
        }

        public static MessageTemplate Template(TemplateType type)
        {
            return Template((int)type);
        }

        /// <summary> 模版格式化 </summary>
        /// <param name="template"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static string Format(this MessageTemplate template, object paras)
        {
            if (string.IsNullOrWhiteSpace(template.Template)) return string.Empty;
            var reg = new Regex("\\{([a-z0-9_]+)\\}", RegexOptions.IgnoreCase);
            if (paras == null)
                return reg.Replace(template.Template, string.Empty);
            var type = paras.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
            var temp = template.Template;
            foreach (Match match in reg.Matches(template.Template))
            {
                var prop =
                    props.FirstOrDefault(
                        t => string.Equals(t.Name, match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                var value = prop == null ? string.Empty : (prop.GetValue(paras) ?? "").ToString();
                temp = temp.Replace(match.Value, value);
            }
            return temp;
        }

        /// <summary> 数据库配置 </summary>
        public static DataBaseConfig DbConfig
        {
            get { return ConfigUtils<DataBaseConfig>.Config; }
        }

        /// <summary> 创建邮件辅助类 </summary>
        public static EmailHelper CreateEmail()
        {
            var email = Config.Email;
            if (email != null)
            {
                return new EmailHelper(email.SenderEmail, email.SenderPwd, email.SenderName, email.SmtpHost, email.SmtpPort,
                    email.UseSsl);
            }
            return null;
        }

        /// <summary> 选项字母集 </summary>
        public static string[] OptionWords
        {
            get
            {
                var list = new List<string>();
                for (var i = 65; i < 91; i++)
                    list.Add(Convert.ToChar(i).ToString(CultureInfo.InvariantCulture));
                return list.ToArray();
            }
        }

        public static DateTime DefaultTime = new DateTime(1970, 1, 1);

        public static DateTime ToDateTime(this long time)
        {
            if (time <= 0) return DefaultTime;
            return new DateTime(DefaultTime.Ticks + time * 10000);
        }

        public static long ToLong(this DateTime time)
        {
            if (time <= DefaultTime) return 0;

            return (time.Ticks - DefaultTime.Ticks) / 10000;
        }

        //单选体
        public static readonly int[] SingleAnswerQType = new[] { 1, 4, 6, 18, 24 };
        //多选
        public static readonly int[] MultiAnswerQType = new[] { 2, 3 };

        //试卷库出卷部分
        public static readonly int[] HasSmallQType = new[] { 4, 6, 18, 26, 27 };//有小问

        public static readonly int[] HasOptionQType = new[] { 1, 2, 3, 4, 6, 18, 24, 26, 27 };//有选项


        /// <summary> 中文数字 </summary>
        public static readonly string[] Chinese =
        {
            "〇", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二",
            "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十"
        };

        public static bool IsCurrentMenu(string url)
        {
            var raw = Utils.RawUrl();
            Uri uri = new Uri(raw),
                currentUri = new Uri(url);
            return uri.LocalPath.IndexOf(currentUri.LocalPath, StringComparison.Ordinal) >= 0;
        }
    }
}
