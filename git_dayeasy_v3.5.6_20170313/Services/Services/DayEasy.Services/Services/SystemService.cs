using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Config;
using DayEasy.EntityFramework;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Services.Services
{
    public partial class SystemService : DayEasyService, ISystemContract
    {
        public SystemService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }

        public IDayEasyRepository<TS_Area, int> AreaRepository { private get; set; }
        public IDayEasyRepository<TS_Subject, int> SubjectRepository { private get; set; }
        public IDayEasyRepository<TS_QuestionType, int> QTypeRepository { private get; set; }
        public IDayEasyRepository<TS_Tag, int> TagRepository { private get; set; }
        public IDayEasyRepository<TS_Message> MessageRepository { private get; set; }
        public IDayEasyRepository<TU_UserAgency> UserAgencyRepository { private get; set; }
        public IDayEasyRepository<TU_AgencyVisit> AgencyVisitRepository { private get; set; }

        public IVersion3Repository<TS_Agency> AgencyRepository { private get; set; }


        public IQueryable<TS_Area> Areas(int parentCode)
        {
            return AreaRepository.Where(a => a.ParentCode == parentCode).OrderBy(a => a.Sort);
        }

        #region 科目

        public IQueryable<TS_Subject> Subjects()
        {
            return SubjectRepository.Where(s => s.Status == (byte)TempStatus.Normal);
        }

        public IDictionary<int, string> SubjectDict(List<int> subjectIds = null)
        {
            Expression<Func<TS_Subject, bool>> condition = s => s.Status == (byte)TempStatus.Normal;
            if (subjectIds != null && subjectIds.Any())
                condition = condition.And(s => subjectIds.Contains(s.Id));
            return SubjectRepository.Where(condition)
                .ToDictionary(k => k.Id, v => v.SubjectName);
        }

        #endregion

        #region 题型

        public List<QuestionTypeDto> GetQuTypeBySubjectId(int subjectId)
        {
            var item = SubjectRepository.Load(subjectId);
            if (item == null || string.IsNullOrWhiteSpace(item.QTypeIDs))
                return null;
            var typeIds = item.QTypeIDs.JsonToObject<int[]>();
            return QTypeRepository
                .Where(t => typeIds.Contains(t.Id) && t.Status == (byte)TempStatus.Normal)
                .MapTo<List<QuestionTypeDto>>();
        }

        public List<QuestionTypeDto> GetQuestionTypes(List<int> idList = null)
        {
            var types = SystemCache.Instance.QuestionTypes();
            if (idList != null && idList.Any())
                return types.Where(t => idList.Contains(t.Id)).ToList();
            return types;
        }

        public List<QuestionTypeDto> GetQuestionTypes(int id)
        {
            var types = SystemCache.Instance.QuestionTypes();
            var objectiveTypes = new[] { 1, 2, 3 };
            return objectiveTypes.Contains(id)
                ? types.Where(t => objectiveTypes.Contains(t.Id)).ToList()
                : types.Where(t => t.Id == id).ToList();
        }

        #endregion

        #region 更新tag

        /// <summary>
        /// 更新tag
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tags"></param>
        public void UpdateTags(TagType type, IEnumerable<string> tags)
        {
            foreach (string tag in tags)
            {
                var item = TagRepository.SingleOrDefault(t => t.TagType == (byte)type && t.TagName == tag);
                if (item != null)
                {
                    item.UsedCount = item.UsedCount + 1;
                    TagRepository.UpdateAsync(item);
                }
                else
                {
                    TagRepository.InsertAsync(new TS_Tag
                    {
                        TagType = (byte)type,
                        TagName = tag,
                        Status = (byte)NormalStatus.Normal,
                        UsedCount = 1,
                        FullPinYin = Utils.ChsString2Spell(tag),
                        SimplePinYin = Utils.GetSpellCode(tag)
                    });
                }
            }
        }

        #endregion

        public IEnumerable<AgencyDto> AgencyList(StageEnum stage, int areaCode, AgencyType type, string keyword = null)
        {
            Expression<Func<TS_Agency, bool>> condition =
                a =>
                    a.Status == (byte)NormalStatus.Normal && a.Stage == (byte)stage &&
                    a.AreaCode.ToString().StartsWith(areaCode.ToString()) &&
                    a.AgencyType == (byte)type;
            if (!string.IsNullOrWhiteSpace(keyword))
                condition = condition.And(a => a.AgencyName.Contains(keyword));
            var list = AgencyRepository.Where(condition)
                .OrderBy(a => a.Sort).MapTo<List<AgencyDto>>();
            list.ForEach(a =>
            {
                a.Initials = Utils.GetSpellCode(a.Name).Substring(0, 1);
            });
            return list;
        }

        public DResults<AgencySearchDto> AgencySearch(string keyword, int level = -1)
        {
            Expression<Func<TS_Agency, bool>> condition = a => a.Status == (byte)NormalStatus.Normal;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                condition = condition.And(a => a.AgencyName.Contains(keyword));
            }
            if (level >= 0)
            {
                condition = condition.And(a => a.CertificationLevel == level);
            }
            var list = AgencyRepository.Where(condition).OrderBy(a => a.Sort)
                .MapTo<List<AgencySearchDto>>();
            return DResult.Succ(list, -1);
        }

        public AgencyItemDto VisitAgency(string id, long userId)
        {
            var model = AgencyRepository.Load(id);
            if (model == null)
                return null;
            model.VisitCount++;
            AgencyRepository.Update(model, "VisitCount");
            if (userId > 0)
            {
                var visit = AgencyVisitRepository.FirstOrDefault(t => t.AgencyId == id && t.VisitUserId == userId);
                if (visit == null || visit.VisitTime < Clock.Now.AddMinutes(-1))
                {
                    AgencyVisitRepository.Insert(new TU_AgencyVisit
                    {
                        Id = IdHelper.Instance.Guid32,
                        AgencyId = id,
                        VisitUserId = userId,
                        VisitTime = Clock.Now
                    });
                }
                else
                {
                    visit.VisitTime = Clock.Now;
                    AgencyVisitRepository.Update(visit, "VisitTime");
                }
            }
            var dto = model.MapTo<AgencyItemDto>();
            if (string.IsNullOrWhiteSpace(dto.Logo))
                dto.Logo = Consts.DefaultAvatar(RecommendImageType.AgencyLogo);
            var statusList = UserAgencyRepository.Where(t => t.AgencyId == id).Select(t => t.Status);
            dto.TargetCount = statusList.Count(t => t == (byte)UserAgencyStatus.Target);
            dto.UserCount = statusList.Count(t => t != (byte)UserAgencyStatus.Target);
            return dto;
        }

        public IDictionary<string, string> AgencyList(IEnumerable<string> agencyIds)
        {
            return AgencyRepository.Where(a => agencyIds.Contains(a.Id))
                .ToDictionary(k => k.Id, v => v.AgencyName);
        }
    }
}
