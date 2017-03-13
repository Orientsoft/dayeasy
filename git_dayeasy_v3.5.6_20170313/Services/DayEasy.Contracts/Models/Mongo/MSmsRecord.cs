using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models.Mongo
{
    /// <summary> 短信发送记录 </summary>
    public class MongoSmsRecord : DEntity<string>
    {
        /// <summary> ID </summary>
        public override string Id { get; set; }

        /// <summary> 接收电话 </summary>
        public string Mobile { get; set; }

        /// <summary> 短信内容 </summary>
        public string Message { get; set; }

        /// <summary> 短信接口平台类型：0-云片 </summary>
        public byte Type { get; set; }

        /// <summary> 发送状态：0-成功、2-发送失败 </summary>
        public byte Status { get; set; }

        /// <summary> 添加时间 </summary>
        public DateTime Time { get; set; }

        /// <summary> 平台反馈发送详细 JSON </summary>
        public string Detail { get; set; }
    }
}
