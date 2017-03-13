using DayEasy.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Dtos.Group
{

    /// <summary>
    /// 导入信息传输对象
    /// </summary>
    [Serializable]
    public  class ImportMessageDto:DDto
    {
        public ImportMessageDto()
        {
           
        }
        public string Message { get; set; }
        public int MessageCount { get; set; }
        public string RepeatMessage { get; set; }
        public int RepeatCount { get; set; }
        public string[] RepeatUsers { get; set; }
    }
}
