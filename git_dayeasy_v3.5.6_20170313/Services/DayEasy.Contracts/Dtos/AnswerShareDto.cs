using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos
{
    /// <summary>
    /// 分享答案
    /// </summary>
    public class AnswerShareDto : DDto
    {
        public string Id { get; set; }
        public long UserId { get; set; }

        public string Name { get; set; }
        public string Avatar { get; set; }

        public int Count { get; set; }
    }

    /// <summary>
    /// 同学分享的答案详细
    /// </summary>
    public class AnswerShareDetailDto : DDto
    {
        public string Img { get; set; }
        public int WorshipCount { get; set; }
        public string WorshipName { get; set; }
        public bool HadWorship { get; set; }
        public string StudentName { get; set; }
    }

    /// <summary>
    /// 添加分享方法传输实体类
    /// </summary>
    public class AnswerShareAddModelDto : DDto
    {
        public AnswerShareAddModelDto()
        {
            Status = AnswerShareStatus.Normal;
            GroupId = string.Empty;
        }
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public string QuestionId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string GroupId { get; set; }
        public AnswerShareStatus Status { get; set; }
    }

}
