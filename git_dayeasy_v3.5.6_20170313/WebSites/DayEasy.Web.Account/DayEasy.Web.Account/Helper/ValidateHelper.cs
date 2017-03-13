
namespace DayEasy.Web.Account.Helper
{
    internal static class ValidateHelper
    {
        internal const string CheckPassUserIdSession = "checkPassUserId";//找回密码第一步
        internal const string AccountSession = "accountSession";//找回密码第二步
        internal const string CheckedDyCodeSession = "CheckedDyCodeSession";//找回密码第三步
        internal const string FindPwdDynamicCodeSession = "sendFindPwdCode";

        internal const string AccountCodeSession = "deyiAccount-{0}";

        /// <summary> 获取邮箱服务器跳转地址 </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        internal static string GetEmailServer(string email)
        {
            var emailServer = email.Trim().Split('@')[1].Trim().ToLower();
            switch (emailServer)
            {
                case "163.com":
                    return "mail.163.com";
                case "vip.163.com":
                    return "vip.163.com";
                case "126.com":
                    return "mail.126.com";
                case "qq.com":
                case "vip.qq.com":
                case "foxmail.com":
                    return "mail.qq.com";
                case "gmail.com":
                    return "mail.google.com";
                case "sohu.com":
                    return "mail.sohu.com";
                case "tom.com":
                    return "mail.tom.com";
                case "vip.sina.com":
                    return "vip.sina.com";
                case "sina.com.cn":
                case "sina.com":
                    return "mail.sina.com.cn";
                case "yahoo.com.cn":
                case "yahoo.cn":
                    return "mail.cn.yahoo.com";
                case "yeah.net":
                    return "www.yeah.net";
                case "21cn.com":
                    return "mail.21cn.com";
                case "hotmail.com":
                    return "www.hotmail.com";
                case "sogou.com":
                    return "mail.sogou.com";
                case "188.com":
                    return "www.188.com";
                case "139.com":
                    return "mail.10086.cn";
                case "189.cn":
                    return "webmail15.189.cn/webmail";
                case "wo.com.cn":
                    return "mail.wo.com.cn/smsmail";
                default:
                    return "mail." + emailServer;

            }
        }
    }
}