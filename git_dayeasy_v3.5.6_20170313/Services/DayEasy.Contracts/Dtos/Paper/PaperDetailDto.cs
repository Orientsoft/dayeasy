using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Paper
{
    /// <summary>
    /// 试卷详情
    /// </summary>
    public class PaperDetailDto : DDto
    {
        public PaperDto PaperBaseInfo { get; set; }

        /// <summary> A卷全客观 </summary>
        public bool AllObjectiveA
        {
            get
            {
                if (PaperSections != null &&
                    !PaperSections.Exists(
                        u => u.PaperSectionType == (byte) PaperSectionType.PaperA && u.Questions == null))
                {
                    return
                        !PaperSections.Exists(
                            u =>
                                u.PaperSectionType == (byte) PaperSectionType.PaperA &&
                                u.Questions.Exists(q => !q.Question.IsObjective));
                }

                return CurrentIocManager.Resolve<IPaperContract>()
                    .AllObjectiveQuestion(PaperBaseInfo.Id, PaperSectionType.PaperA);
            }
        }

        /// <summary> B卷全客观 </summary>
        public bool AllObjectiveB
        {
            get
            {
                if (PaperBaseInfo.PaperType == (byte) PaperType.Normal)
                    return true;

                if (PaperSections != null &&
                    !PaperSections.Exists(
                        u => u.PaperSectionType == (byte) PaperSectionType.PaperB && u.Questions == null))
                {
                    return
                        !PaperSections.Exists(
                            u =>
                                u.PaperSectionType == (byte) PaperSectionType.PaperB &&
                                u.Questions.Exists(q => !q.Question.IsObjective));
                }

                return CurrentIocManager.Resolve<IPaperContract>()
                    .AllObjectiveQuestion(PaperBaseInfo.Id, PaperSectionType.PaperB);
            }
        }

        public List<PaperSectionDto> PaperSections { get; set; }

        public PaperDetailDto()
        {
            PaperSections = new List<PaperSectionDto>();
        }
    }

    public class PaperScoresDto : DDto
    {
        public decimal TScore { get; set; }
        public decimal TScoreA { get; set; }
        public decimal TScoreB { get; set; }
    }

    /// <summary>
    /// 试卷板块
    /// </summary>
    public class PaperSectionDto : DDto
    {
        public string SectionID { get; set; }
        public string Description { get; set; }
        public int Sort { get; set; }
        public int SectionQuType { get; set; }
        public byte PaperSectionType { get; set; }
        public decimal SectionScore { get; set; }
        public List<PaperQuestionDto> Questions { get; set; }

        public PaperSectionDto()
        {
            Questions = new List<PaperQuestionDto>();
        }
    }

    /// <summary> 试卷问题 </summary>
    public class PaperQuestionDto : DDto
    {
        public QuestionDto Question { get; set; }
        public decimal Score { get; set; }
        public int Sort { get; set; }
        public byte PaperSectionType { get; set; }
    }

    /// <summary> 小问分数 </summary>
    public class SmallQuScoreDto : DDto
    {
        public string SmallQId { get; set; }
        public decimal Score { get; set; }
    }
}
