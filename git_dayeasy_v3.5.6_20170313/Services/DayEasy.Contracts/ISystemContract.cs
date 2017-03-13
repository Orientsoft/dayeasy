using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 系统业务模块 </summary>
    public partial interface ISystemContract : IDependency
    {
        /// <summary> 根据parentCode获取地区 </summary>
        /// <param name="parentCode"></param>
        /// <returns></returns>
        IQueryable<TS_Area> Areas(int parentCode);

        /// <summary> 获取所有科目 </summary>
        /// <returns></returns>
        IQueryable<TS_Subject> Subjects();

        /// <summary> 获取科目字典 </summary>
        /// <param name="subjectIds"></param>
        /// <returns></returns>
        IDictionary<int, string> SubjectDict(List<int> subjectIds = null);

        /// <summary> 获取科目下题型 </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        List<QuestionTypeDto> GetQuTypeBySubjectId(int subjectId);

        /// <summary> 获取题型 </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        List<QuestionTypeDto> GetQuestionTypes(List<int> idList = null);

        /// <summary> 获取相关题型 </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<QuestionTypeDto> GetQuestionTypes(int id);

        /// <summary> 更新tag </summary>
        /// <param name="type"></param>
        /// <param name="tags"></param>
        void UpdateTags(TagType type, IEnumerable<string> tags);

        /// <summary> 机构列表 </summary>
        IEnumerable<AgencyDto> AgencyList(StageEnum stage, int areaCode, AgencyType type, string keyword = null);

        /// <summary> 机构搜索 </summary>
        DResults<AgencySearchDto> AgencySearch(string keyword, int level = -1);

        /// <summary> 机构详情 </summary>
        AgencyItemDto VisitAgency(string id, long userId = 0);

        /// <summary> 机构列表 </summary>
        IDictionary<string, string> AgencyList(IEnumerable<string> agencyIds);
    }
}
