using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core.Domain.Entities;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Marking
{
    public class MarkingDataDto : DDto
    {
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public string PaperTitle { get; set; }
        public bool IsAb { get; set; }
        public bool AllObjective { get; set; }
        //试卷类型：1-普通卷/A卷、2-B卷
        public int SectionType { get; set; }
        public byte MarkingStatus { get; set; }
        //是否按包批阅
        public bool IsBag { get; set; }
        public string BagId { get; set; }
        //当前批阅题目分组ID
        public string QuestionGroupId { get; set; }
        //当前考试所属圈 协同-同事圈、非协同-班级圈
        public string DeyiGroupId { get; set; }
        public string DeyiGroupName { get; set; }
        public int UnMarkingCount { get; set; }
        //各问题区域
        public string Areas { get; set; }
        //按包批阅 - 包区域
        public string Region { get; set; }
        public List<QuestionSortDto> Questions { get; set; }
        public List<MkPictureDto> Pictures { get; set; }
        /// <summary> 上次批阅 </summary>
        public string LastPicture { get; set; }
        //未提交的学生名单 - 非协同
        public List<string> UnSubmits { get; set; }
        public List<MarkingDataAnswerShareDto> Shares { get; set; }
    }

    public class MarkingDataAnswerShareDto : DDto
    {
        //答案分享ID
        public string Id { get; set; }
        public string QuestionId { get; set; }
        public long StudentId { get; set; }
    }
}
