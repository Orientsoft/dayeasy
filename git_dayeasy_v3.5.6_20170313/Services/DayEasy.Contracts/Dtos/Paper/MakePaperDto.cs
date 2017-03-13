using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Paper
{
    /// <summary>
    /// 出卷数据结构
    /// </summary>
    public class MakePaperDto : DDto
    {
        public string PaperTitle { get; set; }
        public string PaperType { get; set; }
        public Dictionary<string, string> Kps { get; set; }
        public string[] Tags { get; set; }
        public byte Grade { get; set; }
        public MakePaperScoresDto PScores { get; set; }
        public List<MakePaperSectionDto> PSection { get; set; }

        //操作数据
        public long UserId { get; set; }
        public int SubjectId { get; set; }
        /// <summary> 是否是草稿 </summary>
        public bool IsTemp { get; set; }
        public string ChangeSourceId { get; set; }
    }

    public class MakePaperScoresDto : DDto
    {
        public decimal TScore { get; set; }
        public decimal TScoreA { get; set; }
        public decimal TScoreB { get; set; }
    }

    public class MakePaperSectionDto : DDto
    {
        public int Sort { get; set; }
        public string Description { get; set; }
        public int SectionQuType { get; set; }
        public string PaperSectionType { get; set; }
        public decimal SectionScore { get; set; }
        public List<MakePaperQuestionDto> Questions { get; set; }
    }

    public class MakePaperQuestionDto : DDto
    {
        public int Sort { get; set; }
        public string QuestionID { get; set; }
        public decimal Score { get; set; }
        public List<MakePaperSmallQuestionDto> SmallQuestionScore { get; set; }
    }

    public class MakePaperSmallQuestionDto : DDto
    {
        public string QuestionID { get; set; }
        public decimal Score { get; set; }
    }

    public class MakePaperAnswerDto : DDto
    {
        public string QId { get; set; }
        public string DetailId { get; set; }
        public string Answer { get; set; }
    }


    //******************选择题目dto********************

    /// <summary>
    /// 选择问题实体
    /// </summary>
    public class QuItemDto : DDto
    {
        public string QId { get; set; }
        public int Type { get; set; }
        public decimal? Score { get; set; }
        public int Sort { get; set; }
        public string PaperType { get; set; }
    }

    /// <summary>
    /// 每题好多分
    /// </summary>
    public class SectionPerScoreDto : DDto
    {
        public int QSectionType { get; set; }
        public decimal? PerScore { get; set; }
        public string PaperType { get; set; }
        public int Sort { get; set; }
    }

    /// <summary>
    /// 组卷基础信息
    /// </summary>
    public class PaperBaseDto : DDto
    {
        public PaperBaseDto()
        {
            Stage = (byte) StageEnum.JuniorMiddleSchool;
        }

        /// <summary>
        /// 试卷类型（空白、AB）
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 添加类型（A卷题目或B卷题目）
        /// </summary>
        public string AddType { get; set; }
        /// <summary>
        /// 试卷标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 选择的问题
        /// </summary>
        public List<List<QuItemDto>> ChooseQus { get; set; }
        /// <summary>
        /// 每题分数
        /// </summary>
        public string PerScores { get; set; }
        public int Stage { get; set; }
        public int SubjectId { get; set; }
        /// <summary>
        /// 选题完成跳转地址
        /// </summary>
        public string CompleteUrl { get; set; }
        /// <summary>
        /// 自动出卷的数据
        /// </summary>
        public string AutoData { get; set; }
    }

    /// <summary>
    /// 选择题目数据dto
    /// </summary>
    public class ChooseQuestionDataDto : DDto
    {
        public List<QuItemDto> CurrentChooseQus { get; set; } 
        public List<QuestionDto> Questions { get; set; } 
        public string AutoData { get; set; }
        public List<QuestionTypeDto> QuestionTypes { get; set; } 
        public PaperBaseDto PaperBaseDto { get; set; }
        public bool HasAll { get; set; }
    }

}
