using DayEasy.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 关联报表 - 数据源 </summary>
    public class UnionSourceDto : DDto
    {
        /// <summary> 科目列表 </summary>
        public Dictionary<int, UnionSubjectDto> Subjects { get; set; }

        /// <summary> 大型考试列表 </summary>
        public Dictionary<string, DExamDto> Exams { get; set; }

        /// <summary> 学生信息 </summary>
        public List<UnionStudentDto> Students { get; set; }

        /// <summary> 参考人数 </summary>
        public int StudentCount { get { return Students?.Count(t => t.TotalScore >= 0) ?? 0; } }

        /// <summary> 总人数 </summary>
        public int Count => Students?.Count ?? 0;

        /// <summary> 构造函数 </summary>
        public UnionSourceDto()
        {
            Subjects = new Dictionary<int, UnionSubjectDto>();
            Exams = new Dictionary<string, DExamDto>();
            Students = new List<UnionStudentDto>();
        }
    }

    /// <summary> 关联报表 - 学生信息 </summary>
    public class UnionStudentDto : DDto
    {
        /// <summary> 学生ID </summary>
        public long Id { get; set; }
        /// <summary> 学生名字 </summary>
        public string Name { get; set; }
        /// <summary> 得一号 </summary>
        public string Code { get; set; }
        /// <summary> 学号 </summary>
        public string StudentNum { get; set; }
        /// <summary> 大型考试Id </summary>
        public string ExamId { get; set; }
        /// <summary> 班级ID </summary>
        public string ClassId { get; set; }
        /// <summary> 班级名称 </summary>
        public string ClassName { get; set; }
        /// <summary> 总分 </summary>
        public decimal TotalScore { get; set; }
        /// <summary> 排名 </summary>
        public int Rank { get; set; }
        /// <summary> 学生各科成绩 </summary>
        public Dictionary<int, UnionStudentSubjectDto> Subjects { get; set; }

        /// <summary> 构造 </summary>
        public UnionStudentDto()
        {
            Subjects = new Dictionary<int, UnionStudentSubjectDto>();
        }
    }

    /// <summary> 关联报表 - 单科成绩 </summary>
    public class UnionStudentSubjectDto : DDto
    {
        /// <summary> 考试批次 </summary>
        public string Batch { get; set; }
        /// <summary> 总分 </summary>
        public decimal Score { get; set; }
        /// <summary> A卷得分 </summary>
        public decimal ScoreA { get; set; }
        /// <summary> B卷得分 </summary>
        public decimal ScoreB { get; set; }
    }

    /// <summary> 考试科目 </summary>
    public class UnionSubjectDto : DDto
    {
        /// <summary> 科目ID </summary>
        public int Id { get; set; }
        /// <summary> 科目 </summary>
        public string Subject { get; set; }

        /// <summary> 试卷Id </summary>
        public string PaperId { get; set; }
        /// <summary> 试卷标题 </summary>
        public string PaperTitle { get; set; }
        /// <summary> 是否AB卷 </summary>
        public bool IsAb { get; set; }
        /// <summary> 总分 </summary>
        public decimal Score { get; set; }
        /// <summary> A卷总分 </summary>
        public decimal ScoreA { get; set; }
        /// <summary> B卷总分 </summary>
        public decimal ScoreB { get; set; }
    }
}
