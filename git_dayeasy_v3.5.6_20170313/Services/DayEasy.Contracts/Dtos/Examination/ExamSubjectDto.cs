using System;
using System.Collections.Generic;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 考试科目信息 </summary>
    [AutoMapFrom(typeof(TE_SubjectScore))]
    public class ExamSubjectDto : DExamSubjectDto
    {
        public string Id { get; set; }

        /// <summary> 协同批次号 </summary>
        public string JointBatch { get; set; }

        /// <summary> 发起人 </summary>
        public DUserDto Creator { get; set; }

        /// <summary> 同事圈 </summary>
        public DGroupDto Group { get; set; }

        /// <summary> 试卷ID </summary>
        public string PaperId { get; set; }

        /// <summary> 试卷标题 </summary>
        public string PaperTitle { get; set; }

        /// <summary> 试卷编号 </summary>
        public string PaperNo { get; set; }

        /// <summary> 发起时间 </summary>
        public DateTime CreationTime { get; set; }
        /// <summary> 完成阅卷时间 </summary>
        public DateTime FinishedTime { get; set; }

        /// <summary> 总学生人数 </summary>
        public int StudentCount { get; set; }

        /// <summary> 总班级数 </summary>
        public int ClassCount { get; set; }
        /// <summary> A卷平均分 </summary>
        public decimal AverageScoreA { get; set; }
        /// <summary> B卷平均分 </summary>
        public decimal? AverageScoreB { get; set; }
        /// <summary> 平均分 </summary>
        public decimal AverageScore { get; set; }
        /// <summary> 总分 </summary>
        public decimal Score { get; set; }
        /// <summary> A卷总分 </summary>
        public decimal ScoreA { get; set; }
        /// <summary> B卷总分 </summary>
        public decimal ScoreB { get; set; }
        public int PaperACount { get; set; }
        public int PaperBCount { get; set; }

        /// <summary> 班级信息 </summary>

        public List<JointClass> JointClasses { get; set; }

        public ExamSubjectDto()
        {
            JointClasses = new List<JointClass>();
        }
    }

    /// <summary> 协同班级 </summary>
    public class JointClass : DDto
    {
        /// <summary> 班级ID </summary>
        public string ClassId { get; set; }

        /// <summary> 班级名称 </summary>
        public string ClassName { get; set; }

        /// <summary> 学生数量 </summary>
        public int StudentCount { get; set; }
    }
}
