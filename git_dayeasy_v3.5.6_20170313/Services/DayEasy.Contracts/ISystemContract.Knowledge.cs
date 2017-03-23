using System.Collections.Generic;
using DayEasy.Contracts.Dtos;
using DayEasy.Utility;
using DayEasy.Contracts.Dtos.ErrorQuestion;
using DayEasy.Contracts.Dtos.Question;

namespace DayEasy.Contracts
{
    /// <summary> 系统业务模块 - 知识点相关业务 </summary>
    public partial interface ISystemContract
    {
        /// <summary> 获取知识点 </summary>
        /// <param name="knowledgeDto"></param>
        /// <returns></returns>
        List<KnowledgeDto> Knowledges(SearchKnowledgeDto knowledgeDto);

        /// <summary> 获取知识点 </summary>
        /// <param name="kpIds"></param>
        /// <returns></returns>
        List<KnowledgeDto> Knowledges(List<int> kpIds);

        /// <summary> 获取知识点树 </summary>
        /// <param name="knowledgeDto"></param>
        /// <returns></returns>
        DResults<TreeDto> KnowledgeTrees(SearchKnowledgeDto knowledgeDto);

        Dictionary<string, string> KnowledgePath(string code);
    }  
}
