
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core;
using DayEasy.Core.Cache;
using DayEasy.Core.Dependency;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using System.Collections.Generic;
using System.Linq;
using DayEasy.AutoMapper;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;

namespace DayEasy.Services.Helper
{
    /// <summary> 系统缓存 - Redis </summary>
    public class SystemCache
    {
        private const string CacheRegion = "system";
        private static readonly int[] JuniorSubjects = { 4, 5, 9 };
        private readonly ICache _cache;

        private SystemCache()
        {
            _cache = CacheManager.GetCacher(CacheRegion);
        }
        public static SystemCache Instance
        {
            get { return Singleton<SystemCache>.Instance ?? (Singleton<SystemCache>.Instance = new SystemCache()); }
        }

        /// <summary> 科目缓存 </summary>
        private List<SubjectDto> SubjectList()
        {
            var subjects = _cache.Get<List<SubjectDto>>(Consts.SubjectCacheKey);
            if (!subjects.IsNullOrEmpty())
                return subjects;
            subjects = new List<SubjectDto>();
            var systemContract = CurrentIocManager.Resolve<ISystemContract>();
            var list = systemContract.Subjects().Select(t => new
            {
                t.Id,
                t.SubjectName,
                t.IsLoadFormula,
                t.QTypeIDs
            }).ToList();
            foreach (var item in list)
            {
                var subject = new SubjectDto
                {
                    Id = item.Id,
                    Name = item.SubjectName,
                    IsLoadFormula = item.IsLoadFormula
                };
                if (string.IsNullOrWhiteSpace(item.QTypeIDs))
                    continue;
                subject.QuestionTypes = JsonHelper.JsonList<int>(item.QTypeIDs).ToArray();
                subjects.Add(subject);
            }
            _cache.Set(Consts.SubjectCacheKey, subjects);
            return subjects;
        }

        /// <summary> 题型缓存 </summary>
        public List<QuestionTypeDto> QuestionTypes()
        {
            var types = _cache.Get<List<QuestionTypeDto>>(Consts.QuestionTypeCacheKey);
            if (!types.IsNullOrEmpty())
                return types;
            var typeRepository = CurrentIocManager.Resolve<IDayEasyRepository<TS_QuestionType, int>>();
            types = typeRepository.Where(t => t.Status == (byte)TempStatus.Normal).MapTo<List<QuestionTypeDto>>();
            _cache.Set(Consts.QuestionTypeCacheKey, types);
            return types;
        }

        /// <summary> 获取科目 </summary>
        /// <returns></returns>
        public IDictionary<int, string> Subjects()
        {
            return SubjectList().ToDictionary(k => k.Id, v => v.Name);
        }

        public bool SubjectLoadFormula(int subjectId)
        {
            var subject = SubjectList().FirstOrDefault(t => t.Id == subjectId);
            return subject == null ? false : subject.IsLoadFormula;
        }

        /// <summary> 学段信息 </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public IEnumerable<byte> Stages(int subjectId)
        {
            if (JuniorSubjects.Contains(subjectId))
                return new byte[] { 2, 3 };
            return new byte[] { 1, 2, 3 };
        }

        /// <summary> 科目 </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public string SubjectName(int subjectId)
        {
            var subject = SubjectList().FirstOrDefault(s => s.Id == subjectId);
            if (subject != null)
                return subject.Name;
            return string.Empty;
        }

        /// <summary> 科目题型 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public IEnumerable<QuestionTypeDto> SubjectQuestionTypes(int subjectId)
        {
            var subject = SubjectList().FirstOrDefault(s => s.Id == subjectId);
            if (subject == null || subject.QuestionTypes.IsNullOrEmpty())
                return new List<QuestionTypeDto>();
            return QuestionTypes().Where(t => subject.QuestionTypes.Contains(t.Id));
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}
