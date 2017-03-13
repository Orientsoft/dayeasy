using System;
using System.Web.Http;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Models.Open.Message;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Attributes;

namespace DayEasy.Web.Open.Controllers
{
    /// <summary> 消息接口 </summary>
    [DApi]
    public class MessageController : DApiController
    {
        private IMessageContract _messageContract;
        private readonly ISmsRecordContract _smsContract;
        public const string VcodeCacheKey = "dayeasy_vcode_session_{0}";
        public MessageController(IUserContract userContract, IMessageContract messageContract, ISmsRecordContract smsContract)
            : base(userContract)
        {
            _messageContract = messageContract;
            _smsContract = smsContract;
        }

        [HttpPost]
        public DResult<long> Send(long receiver_id, string message)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [DApiAuthorize(UserRole.Teacher)]
        public DResult<long> BatchSend(string class_id, string message)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public DResults<string> Receive()
        {
            throw new NotImplementedException();
        }

        /// <summary> 发送邮件 </summary>
        [HttpPost]
        public DResult Email(EmailDto dto)
        {
            using (var email = Consts.CreateEmail())
            {
                var result = email.SendEmail(dto.Receiver, dto.Title, dto.Body);
                return result ? DResult.Success : DResult.Error("发送邮件失败！");
            }
        }

        /// <summary> 发送短信验证码 </summary>
        [HttpPost]
        public DResult SendSmsCode(SmsDto dto)
        {
            if (dto.Check)
            {
                var checkResult = UserContract.CheckAccount(dto.Mobile);
                if (!checkResult.Status)
                    return checkResult;
            }
            var rcode = RandomHelper.Random().Next(100000, 999999).ToString();
            var key = VcodeCacheKey.FormatWith(dto.Mobile);
            var timeKey = key + "_time";
            var lastTime = CacheHelper.Get<DateTime>(timeKey);
            if (lastTime != DateTime.MinValue && (Clock.Now - lastTime).TotalMinutes < 1)
                return DResult.Error("一分钟之后才能重新发送！");
            CacheHelper.Add(timeKey, Clock.Now, 1);
            var result = _smsContract.SendVcode(dto.Mobile, rcode);
            if (result.Status)
            {
                CacheHelper.Add(key, rcode, 30);
            }
            return result;
        }
    }
}
