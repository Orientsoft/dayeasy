using System;
using System.Globalization;
using DayEasy.Contracts;
using DayEasy.Core;
using DayEasy.Core.Config;
using DayEasy.Core.Dependency;
using DayEasy.ThirdPlatform;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;

namespace DayEasy.Web.Account.Helper
{
    internal static class VCodeHelper
    {
        internal static DResult Send(string receive, string code, string sessionKey, TemplateType? type = null)
        {
            if (receive.IsNullOrEmpty()) return DResult.Error("接收地址错误");
            if (code.IsNullOrEmpty()) return DResult.Error("验证码错误");
            var accountCodeContract = CurrentIocManager.Resolve<IAccountCodeContract>();
            var ckSecond = accountCodeContract.Get(receive);
            if (ckSecond != null)
            {
                var dt = Clock.Now;
                ckSecond.Time = Clock.Normalize(ckSecond.Time);
                if (ckSecond.Time.AddMinutes(1) > dt)
                    return DResult.Error("操作过于频繁，请于1分钟后尝试");
                if (ckSecond.Count > 2 && ckSecond.Time.AddHours(1) > dt)
                    return DResult.Error("操作过于频繁，请于1小时后尝试");
            }
            var sendSuccess = false;
            if (receive.As<IRegex>().IsMobile())
            {
                var sendResult = CurrentIocManager.Resolve<ISmsRecordContract>().SendVcode(receive, code);
                if (!sendResult.Status) return DResult.Error("发送失败！");
                sendSuccess = true;
            }
            if (receive.As<IRegex>().IsEmail())
            {
                if (type == null) type = TemplateType.Register;
                using (var emailSender = Consts.CreateEmail())
                {
                    var temp = Consts.Template((int)type);
                    var title = temp.Title;
                    var body = temp.Template;
                    body = body.Replace("{email}", receive).Replace("{rcode}", code.ToString(CultureInfo.InvariantCulture));
                    if (!emailSender.SendEmail(receive, title, body)) return DResult.Error("发送失败");
                    sendSuccess = true;
                }
            }
            if (sendSuccess)
            {
                SessionHelper.Add(sessionKey, code, 30);
                accountCodeContract.Edit(receive);
                return DResult.Success;
            }
            return DResult.Error("未知的接收地址类型");
        }

        internal static DResult Check(string receive, string code, string sessionKey)
        {
            if (receive.IsNullOrEmpty()) return DResult.Error("接收地址错误");
            if (code.IsNullOrEmpty()) return DResult.Error("验证码错误");
            var serverCode = SessionHelper.Get<string>(sessionKey);
            if (!string.Equals(code, serverCode, StringComparison.CurrentCultureIgnoreCase))
                return DResult.Error("验证码错误");

            SessionHelper.Remove(sessionKey);
            CurrentIocManager.Resolve<IAccountCodeContract>().Reset(receive);
            return DResult.Success;
        }
    }
}