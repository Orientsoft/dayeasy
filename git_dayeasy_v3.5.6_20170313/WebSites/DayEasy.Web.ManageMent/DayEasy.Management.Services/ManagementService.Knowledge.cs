using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Management.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.Management.Services
{
    public partial class ManagementService
    {
        public IDayEasyRepository<TS_Knowledge, int> KnowledgeRepository { private get; set; }

        public IDayEasyRepository<TQ_Question, string> QuestionRepository { private get; set; }

        private string MakeCode(string parentCode)
        {
            return MakeCode(parentCode, 1).FirstOrDefault();
        }

        private List<string> MakeCode(string parentCode, int count)
        {
            var codes =
                KnowledgeRepository.Where(
                    t => t.Code.StartsWith(parentCode) && t.Code.Length == parentCode.Length + 2)
                    .Select(t => t.Code).OrderBy(t => t).ToArray();
            var list = new List<string>();
            var index = 1;
            foreach (string code in codes)
            {
                var num = code.Substring(parentCode.Length).CastTo(1);
                //补缺
                if (num > index)
                {
                    list.Add(parentCode + index.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));
                    if (list.Count == count)
                        break;
                }
                index++;
            }
            index = codes.Length + 1;
            while (list.Count < count)
            {
                list.Add(parentCode + index.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));
                index++;
            }
            return list;
        }

        public Dictionary<int, string> KnowledgePath(int knowledgeId)
        {
            var dict = new Dictionary<int, string>();
            if (knowledgeId <= 0)
                return dict;
            var knowledge = KnowledgeRepository.Load(knowledgeId);
            if (knowledge == null)
                return dict;
            var codeList = new List<string>();
            var code = knowledge.Code;
            while (code.Length > 3)
            {
                code = code.Substring(0, code.Length - 2);
                codeList.Add(code);
            }
            if (codeList.Any())
            {
                dict = KnowledgeRepository.Where(k => codeList.Contains(k.Code))
                    .OrderBy(k => k.Id)
                    .ToDictionary(k => k.Id, v => v.Name);
            }
            dict.Add(knowledgeId, knowledge.Name);
            return dict;
        }

        public DResults<TS_Knowledge> KnowledgeSearch(KnowledgeSearchDto searchDto)
        {
            if (searchDto == null)
                return DResult.Errors<TS_Knowledge>("参数异常~！");
            if (searchDto.SubjectId <= 0 || searchDto.Stage <= 0)
                return DResult.Errors<TS_Knowledge>("参数异常~！");
            Expression<Func<TS_Knowledge, bool>> condition = k =>
                k.SubjectID == searchDto.SubjectId
                && k.Stage == searchDto.Stage;
            if (!string.IsNullOrWhiteSpace(searchDto.Keyword))
            {
                var key = searchDto.Keyword.Trim();
                condition = condition.And(k => k.Code == searchDto.Keyword || k.Name.Contains(key));
            }
            else
            {
                condition = condition.And(k => k.PID == searchDto.ParentId);
            }
            var count = KnowledgeRepository.Count(condition);
            var list = KnowledgeRepository.Where(condition)
                .OrderBy(k => k.Status)
                .ThenBy(k => k.Code)
                .ThenBy(k => k.Sort > 0 ? k.Sort : 99).ToList();
            return DResult.Succ(list, count);
        }

        public DResult KnowledgeUpdate(KnowledgeDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return DResult.Error("参数错误~！");
            if (string.IsNullOrWhiteSpace(dto.Name))
                return DResult.Error("知识点名称不能为空~！");
            if (dto.Sort < 0)
                return DResult.Error("排序必须大于等于0");
            var knowledge = KnowledgeRepository.Load(dto.Id);
            if (knowledge == null)
                return DResult.Error("知识点未找到！");
            knowledge.Name = dto.Name;
            knowledge.Sort = dto.Sort;
            var result = KnowledgeRepository.Update(k => new
            {
                k.Name,
                k.Sort
            }, knowledge);
            return DResult.FromResult(result);
        }

        public DResult KnowledgeUpdateStatus(int knowledgeId, int status)
        {
            if (knowledgeId <= 0)
                return DResult.Error("知识点ID不存在！");
            var knowledge = KnowledgeRepository.Load(knowledgeId);
            if (knowledge == null)
                return DResult.Error("知识点ID不存在！");
            if (knowledge.Status == status)
                return DResult.Error("状态已变更！");
            int result;
            if (status < 0)
            {
                //                if (knowledge.Status != (byte)TempStatus.Delete)
                //                    return DResult.Error("知识点状态不匹配！");
                var count = KnowledgeQuestionCount(knowledge.Code);
                if (count > 0)
                    return DResult.Error("知识点下存在{0}道试题，请先转移！".FormatWith(count));

                result = KnowledgeRepository.Delete(t => t.Code.StartsWith(knowledge.Code));
                return new DResult(true, "成功删除{0}个知识点！".FormatWith(result));
            }
            knowledge.Status = (byte)status;

            TS_Knowledge parent = null;
            if (knowledge.PID > 0)
            {
                parent = KnowledgeRepository.Load(knowledge.PID);
                if (parent != null)
                {
                    if (knowledge.Status == (byte)TempStatus.Normal)
                    {
                        if (!parent.HasChildren)
                            parent.HasChildren = true;
                        else
                            parent = null;
                    }
                    else
                    {
                        Expression<Func<TS_Knowledge, bool>> condition = k =>
                            k.PID == parent.Id
                            && k.Status == (byte)TempStatus.Normal
                            && k.Id != knowledgeId;
                        parent.HasChildren = KnowledgeRepository.Exists(condition);
                    }
                }
            }
            result = UnitOfWork.Transaction(() =>
            {
                KnowledgeRepository.Update(k => new { k.Status }, knowledge);
                if (parent != null)
                    KnowledgeRepository.Update(k => new { k.HasChildren }, parent);
            });
            return DResult.FromResult(result);
        }

        public DResult KnowledgeInsert(KnowledgeDto dto)
        {
            if (dto == null)
                return DResult.Error("参数异常~！");
            if (dto.SubjectId <= 0)
                return DResult.Error("请选择科目！");
            if (dto.Stage <= 0)
                return DResult.Error("请选择学段！");
            if (string.IsNullOrWhiteSpace(dto.Name))
                return DResult.Error("知识点名称不能为空！");
            TS_Knowledge parent = null;
            string code;
            if (dto.ParentId > 0)
            {
                parent =
                    KnowledgeRepository.FirstOrDefault(t => t.Id == dto.ParentId);
                if (parent == null)
                    return DResult.Error("父级节点不存在！");

                code = MakeCode(parent.Code);
                if (!parent.HasChildren)
                {
                    parent.HasChildren = true;
                }
                else
                {
                    parent = null;
                }
            }
            else
            {
                code = MakeCode(string.Concat(dto.Stage, dto.SubjectId.ToString().PadLeft(2, '0')));
            }
            var newKp = new TS_Knowledge
            {
                Name = dto.Name,
                FullPinYin = string.Empty,
                SimplePinYin = string.Empty,
                Code = code,
                PID = dto.ParentId,
                Sort = dto.Sort,
                Stage = (byte)dto.Stage,
                SubjectID = dto.SubjectId,
                Status = (byte)TempStatus.Normal,
                HasChildren = false
            };

            var result = UnitOfWork.Transaction(() =>
            {
                KnowledgeRepository.Insert(newKp);
                if (parent != null)
                {
                    KnowledgeRepository.Update(k => new
                    {
                        k.HasChildren
                    }, parent);
                }
            });
            return DResult.FromResult(result);
        }

        public int KnowledgeQuestionCount(string code)
        {
            return QuestionRepository.Count(t =>
                t.KnowledgeIDs != null
                && t.KnowledgeIDs.Contains("\"" + code));
        }

        public DResult KnowledgeMove(string sourceCode, string targetCode)
        {
            var knowledge = KnowledgeRepository.FirstOrDefault(t => t.Code == targetCode);
            if (knowledge == null)
                return DResult.Error("目标知识点不存在！");
            var source = KnowledgeRepository.FirstOrDefault(t => t.Code == sourceCode);
            if (source == null)
                return DResult.Error("知识点不存在！");
            if (sourceCode.Substring(0, 3) != targetCode.Substring(0, 3))
                return DResult.Error("不同学段/学科下的知识点不能转移！");
            return KnowledgeMover.Instance.AddMove(sourceCode, source.Name, targetCode, knowledge.Name);
        }
    }
}
