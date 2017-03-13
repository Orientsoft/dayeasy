
using System;
using System.Collections.Generic;
using DayEasy.Models.Open.User;
using Newtonsoft.Json;

namespace DayEasy.Models.Open.Group
{
    public class MMemberDto : MUserDto
    {
        public string BusinessCard { get; set; }

        /// <summary> 入圈时间 </summary>
        [JsonIgnore]
        public DateTime AddedTime { get; set; }

        public long Time { get { return ToLong(AddedTime); } }
        /// <summary> 已关联的父母 </summary>
        public IEnumerable<MUserBaseDto> Parents { get; set; }

        public MMemberDto()
        {
            Parents = new List<MUserBaseDto>();
        }
    }
}
