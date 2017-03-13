
using System.Collections.Generic;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Utility;

namespace DayEasy.Contracts.Management
{
    /// <summary> 知识点管理 </summary>
    public partial interface IManagementContract
    {
        /// <summary> 知识点路径 </summary>
        /// <param name="knowledgeId"></param>
        /// <returns></returns>
        Dictionary<int, string> KnowledgePath(int knowledgeId);
        DResults<TS_Knowledge> KnowledgeSearch(KnowledgeSearchDto searchDto);

        DResult KnowledgeUpdate(KnowledgeDto dto);

        DResult KnowledgeUpdateStatus(int knowledgeId,int status);

        DResult KnowledgeInsert(KnowledgeDto dto);

        /// <summary> 知识点问题数 </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        int KnowledgeQuestionCount(string code);

        /// <summary> 知识点迁移 </summary>
        /// <param name="sourceCode">原编码</param>
        /// <param name="targetCode">新编码</param>
        /// <returns></returns>
        DResult KnowledgeMove(string sourceCode, string targetCode);
    }
}
