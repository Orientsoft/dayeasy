using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Contracts.Models.Mongo
{
    /// <summary> 成绩通知短信发送记录 </summary>
    public class MongoSmsScoreNotice : DEntity<string>
    {
        /// <summary> ID </summary>
        public override string Id { get; set; }

        /// <summary> 短信记录ID </summary>
        public string SmsRecordId { get; set; }

        /// <summary> 批次号 </summary>
        public string Batch { get; set; }

        /// <summary> 试卷ID </summary>
        public string PaperId { get; set; }

        /// <summary> 已通知手机 </summary>
        public string Mobile { get; set; }
        
        /// <summary> 通知时间 </summary>
        public DateTime Time { get; set; }

    }
}
