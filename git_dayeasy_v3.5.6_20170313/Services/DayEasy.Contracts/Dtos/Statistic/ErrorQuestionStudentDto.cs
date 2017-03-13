using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain.Entities;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Statistic
{
    /// <summary>
    /// 试卷题目答错的学生
    /// </summary>
    public class ErrorQuestionStudentDto :DDto
    {
        public string QuestionId { get; set; }
        public List<DUserDto> Students { get; set; }
    }
}
