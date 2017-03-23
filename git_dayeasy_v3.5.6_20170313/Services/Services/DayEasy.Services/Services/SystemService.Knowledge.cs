using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Services.Services
{
    public partial class SystemService
    {
        public IDayEasyRepository<TS_Knowledge, int> KnowledgeRepository { private get; set; }
        #region 知识点

        public List<KnowledgeDto> Knowledges(SearchKnowledgeDto knowlodgeDto)
        {
            if (knowlodgeDto == null || knowlodgeDto.Stage > 3 || knowlodgeDto.SubjectId < 0 ||
                knowlodgeDto.SubjectId > 40)
            {
                return new List<KnowledgeDto>();
            }
            Expression<Func<TS_Knowledge, bool>> condition =
                u => u.Status == (byte)TempStatus.Normal
                     && u.Stage == knowlodgeDto.Stage
                     && u.SubjectID == knowlodgeDto.SubjectId;
            Expression<Func<TS_Knowledge, int>> orderBy = u => (u.Sort == 0 ? 99 : u.Sort);
            if (knowlodgeDto.ParentId >= 0)
                condition = condition.And(u => u.PID == knowlodgeDto.ParentId);
            if (!string.IsNullOrWhiteSpace(knowlodgeDto.ParentCode))
                condition =
                    condition.And(
                        u =>
                            u.Code.StartsWith(knowlodgeDto.ParentCode) &&
                            u.Code.Length == knowlodgeDto.ParentCode.Length + 2);
            if (knowlodgeDto.Version.HasValue)
                condition = condition.And(u => u.KnowledgeVersion == knowlodgeDto.Version);
            if (knowlodgeDto.IsLast)
                condition = condition.And(u => !u.HasChildren);
            if (!string.IsNullOrWhiteSpace(knowlodgeDto.Keyword))
            {
                condition = condition.And(u => u.Name.Contains(knowlodgeDto.Keyword));
                orderBy = u => u.PID;
            }
            var knowledges = KnowledgeRepository.Where(condition)
                .OrderBy(u => u.KnowledgeVersion)
                .ThenBy(u => u.Name == knowlodgeDto.Keyword ? 0 : 1)
                .ThenBy(orderBy)
                .Skip(knowlodgeDto.Size * knowlodgeDto.Page)
                .Take(knowlodgeDto.Size)
                .MapTo<List<KnowledgeDto>>();
            if (knowlodgeDto.LoadPath)
            {
                var parents = KnowledgeParents(knowledges.Select(k => k.Code));
                foreach (var knowledge in knowledges)
                {
                    //                    knowledge.Parents = KnowledgePath(knowledge.Code);
                    knowledge.Parents = parents.Where(t => knowledge.Code.Contains(t.Key))
                        .OrderBy(t => t.Key.Length)
                        .ToDictionary(k => k.Key, v => v.Value);
                }
            }
            return knowledges;
        }

        public List<KnowledgeDto> Knowledges(List<int> kpIds)
        {
            if (kpIds == null || kpIds.Count < 1)
            {
                return new List<KnowledgeDto>();
            }

            Expression<Func<TS_Knowledge, bool>> condition =
                u => u.Status == (byte)TempStatus.Normal
                     && kpIds.Contains(u.Id);

            Expression<Func<TS_Knowledge, int>> orderBy = u => (u.Sort == 0 ? 99 : u.Sort);
            return KnowledgeRepository.Where(condition)
                    .OrderBy(u => u.KnowledgeVersion)
                    .ThenBy(orderBy)
                    .MapTo<List<KnowledgeDto>>();
        }

        public DResults<TreeDto> KnowledgeTrees(SearchKnowledgeDto knowledgeDto)
        {
            var listResult = Knowledges(knowledgeDto);
            if (!listResult.Any())
                return DResult.Succ(new List<TreeDto>(), 0);
            var list = listResult.Select(
                t => new TreeDto
                {
                    Id = t.Id.ToString(CultureInfo.InvariantCulture),
                    ParentId = (t.ParentId > 0 ? t.ParentId.ToString(CultureInfo.InvariantCulture) : "#"),
                    Text = t.Name,
                    HasChildren = t.IsParent,
                    LiAttr = new Attr
                    {
                        Sort = (t.Sort == 0 ? 99 : t.Sort),
                        Title = t.Code
                    }
                }).ToList();
            return DResult.Succ(list, list.Count());
        }


        public Dictionary<string, string> KnowledgePath(string code)
        {
            var dict = new Dictionary<string, string>();

            var codeList = new List<string>();
            while (code.Length > 3)
            {
                code = code.Substring(0, code.Length - 2);
                codeList.Add(code);
            }
            if (codeList.Any())
            {
                dict = KnowledgeRepository.Where(k => codeList.Contains(k.Code))
                    .OrderBy(k => k.Id)
                    .ToDictionary(k => k.Code, v => v.Name);
            }
            return dict;
        }

        private Dictionary<string, string> KnowledgeParents(IEnumerable<string> codes)
        {
            var dict = new Dictionary<string, string>();
            var parentCodes = new List<string>();
            foreach (var code in codes)
            {
                var item = code;
                while (item.Length > 3)
                {
                    item = item.Substring(0, item.Length - 2);
                    if (!parentCodes.Contains(item))
                        parentCodes.Add(item);
                }
            }
            if (parentCodes.Any())
            {
                dict = KnowledgeRepository.Where(k => parentCodes.Contains(k.Code))
                    .OrderBy(k => k.Id)
                    .ToDictionary(k => k.Code, v => v.Name);
            }
            return dict;
        }
        #endregion
    }
}
