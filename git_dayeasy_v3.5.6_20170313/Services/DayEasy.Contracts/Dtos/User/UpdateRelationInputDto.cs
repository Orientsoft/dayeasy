using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    public class UpdateRelationInputDto : DDto
    {
        public string Id { get; set; }
        /// <summary> 开始时间 </summary>
        public DateTime Start { get; set; }
        /// <summary> 结束时间 </summary>
        public DateTime? End { get; set; }
    }
}
