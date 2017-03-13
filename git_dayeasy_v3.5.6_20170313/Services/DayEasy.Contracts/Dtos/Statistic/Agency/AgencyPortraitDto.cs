using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Statistic.Agency
{
    /// <summary> 学校师生画像 </summary>
    public class AgencyPortraitDto : RefreshDto
    {
        /// <summary> 登录情况 </summary>
        public List<AgencyLoginDto> Logins { get; set; }
        /// <summary> 教师画像 </summary>
        public TeacherPortraitDto Teacher { get; set; }
        /// <summary> 学生画像 </summary>
        public StudentPortraitDto Student { get; set; }
        /// <summary> 登录排名 </summary>
        public List<AgencyUserRankingDto> LoginRank { get; set; }
        /// <summary> 批阅排名 </summary>
        public List<AgencyUserRankingDto> MarkingRank { get; set; }
        /// <summary> 访问排名 </summary>
        public List<AgencyUserRankingDto> VisitRank { get; set; }
    }

    /// <summary> 学校用户登录情况 </summary>
    public class AgencyLoginDto : DDto
    {
        /// <summary> 时间 </summary>
        public DateTime Time { get; set; }
        /// <summary> 学生登录次数 </summary>
        public int Student { get; set; }
        /// <summary> 教师登录次数 </summary>
        public int Teacher { get; set; }
        /// <summary> 总次数 </summary>
        public int Total { get { return Student + Teacher; } }
    }

    /// <summary> 画像基类 </summary>
    public class DPortraitDto : DDto
    {
        /// <summary> 平均登录次数 </summary>
        public float AverageLogin { get; set; }
        /// <summary> 印象/错因分布 </summary>
        public Dictionary<string, int> Portraits { get; set; }
    }

    /// <summary> 教师画像 </summary>
    public class TeacherPortraitDto : DPortraitDto
    {
        /// <summary> 平均阅卷次数 </summary>
        public float AverageMarked { get; set; }
    }

    /// <summary> 学生画像 </summary>
    public class StudentPortraitDto : DPortraitDto
    {
        /// <summary> 平均考试次数 </summary>
        public float AverageExam { get; set; }

        /// <summary> 平均新增错题 </summary>
        public float AverageError { get; set; }
    }

    /// <summary> 用户排名 </summary>
    public class AgencyUserRankingDto : DUserDto
    {
        /// <summary> 排名 </summary>
        public int Rank { get; set; }
        /// <summary> 数量 </summary>
        public int Count { get; set; }
    }
}
