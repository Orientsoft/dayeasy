using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core.Cache;
using DayEasy.Utility;
using System;

namespace DayEasy.Paper.Services.Helper.Question
{
    /// <summary> 问题缓存 </summary>
    public class QuestionCache : DEntityCache<QuestionDto>
    {
        public QuestionCache()
            : base("question", TimeSpan.FromDays(60))
        {
        }

        /// <summary> 单例 </summary>
        public static QuestionCache Instance
        {
            get
            {
                return (Singleton<QuestionCache>.Instance ?? (Singleton<QuestionCache>.Instance = new QuestionCache()));
            }
        }

        /// <summary> 设置缓存 </summary>
        /// <param name="questionDto"></param>
        public void Set(QuestionDto questionDto)
        {
            Set(questionDto, questionDto.Id);
        }
    }
}
