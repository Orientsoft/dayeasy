using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Tutor
{
    /// <summary>
    /// 辅导详情
    /// </summary>
    public class TutorDetailDto : DDto
    {
        public TutorDetailDto()
        {
            Contents = new List<TutorContentDto>();
        }

        /// <summary>
        /// 数据库辅导模型
        /// </summary>
        public TutorDto Tutor { get; set; }
        
        /// <summary>
        /// 特性与解法
        /// </summary>
        public string Feature { get; set; }

        /// <summary>
        /// 辅导内容列表
        /// </summary>
        public List<TutorContentDto> Contents { get; set; }
    }
    
    public class TutorContentDto : DDto
    {
        public string ContentId { get; set; }
        public int Sort { get; set; }
        public QuestionDto QItem { get; set; }
        public SimpleVideoDto VideoItem { get; set; }
        public string Content { get; set; }
        public string Remarks { get; set; }
        public byte ContentType { get; set; } 
    }


}
