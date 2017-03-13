
using System.Collections.Generic;
using DayEasy.Contracts.Models;

namespace DayEasy.Paper.Services.Helper.Question
{
    /// <summary> 题目转换结果类 </summary>
    public class ConvertResult
    {
//        public bool MarkingUpdate { get; set; }
        public TQ_Question Question { get; set; }
        public List<TQ_SmallQuestion> SmallQuestions { get; set; }
        public List<TQ_Answer> Answers { get; set; }
        public TQ_Analysis Analysis { get; set; }

        public ConvertResult()
        {
            SmallQuestions = new List<TQ_SmallQuestion>();
            Answers = new List<TQ_Answer>();
        }
    }
}
