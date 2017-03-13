using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 考试学生信息 </summary>
    public class ExamStudentDto : StudentRankDto
    {
        /// <summary> 学生ID </summary>
        public long StudentId { get; set; }

        /// <summary> 名字 </summary>
        public string Name { get; set; }

        /// <summary> 学号 </summary>
        public string StudentNo { get; set; }

        /// <summary> 得一号 </summary>
        public string UserCode { get; set; }

        /// <summary> 班级ID </summary>
        public string ClassId { get; set; }

        /// <summary> 班级名称 </summary>
        public string ClassName { get; set; }
        /// <summary> 学校名称 </summary>
        public string AgencyName { get; set; }

        /// <summary> 学科分数 </summary>
        public Dictionary<string, StudentRankDto> ScoreDetails { get; set; }

        /// <summary> 构造函数 </summary>
        public ExamStudentDto()
        {
            ScoreDetails = new Dictionary<string, StudentRankDto>();
        }
    }
}
