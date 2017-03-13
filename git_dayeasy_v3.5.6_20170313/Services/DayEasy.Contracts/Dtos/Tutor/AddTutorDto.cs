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
    /// 添加辅导前台交互数据类
    /// </summary>
    public class AddTutorDto : DDto
    {
        public AddTutorDto()
        {
            Kps = new List<NameDto>();
            Tags = new List<string>();
        }

        public string EditId { get; set; }
        public string Profile { get; set; }
        public string Title { get; set; }
        public byte Diff { get; set; }
        public string Author { get; set; }
        public byte Grade { get; set; }
        public List<NameDto> Kps { get; set; }
        public List<string> Tags { get; set; }
        public int Subject { get; set; }
        public string Description { get; set; }
        public string SolveContent { get; set; }
        public List<ContentsDto> Contents { get; set; }
    }

    /// <summary>
    /// 辅导数据
    /// </summary>
    public class TutorDataDto : AddTutorDto
    {
        public List<QuestionDto> Questions { get; set; }
        public List<SimpleVideoDto> Videos { get; set; }
    }

    /// <summary>
    /// 前天交互辅导内容类
    /// </summary>
    public class ContentsDto : DDto
    {
        public int Sort { get; set; }
        public byte Type { get; set; }
        public string Remarks { get; set; }
        public string Detail { get; set; }
    }

    /// <summary>
    /// 简单视频dto
    /// </summary>
    public class SimpleVideoDto : DDto
    {
        public string VideoId { get; set; }
        public string FrontCover { get; set; }
        public string VideoName { get; set; }
        public string VideoUrl { get; set; }
    }
}
