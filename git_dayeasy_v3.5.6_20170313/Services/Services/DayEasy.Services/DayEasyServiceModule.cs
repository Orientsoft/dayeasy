using DayEasy.Core;
using DayEasy.Core.Modules;
using DayEasy.EntityFramework;
using DayEasy.Utility.Logging;

namespace DayEasy.Services
{
    /// <summary> 得一服务项目模块 </summary>
    [DependsOn(typeof(CoreModule))]
    public class DayEasyServiceModule : DModule
    {
        private readonly ILogger _logger = LogManager.Logger<DayEasyServiceModule>();
        public override void PreInitialize()
        {
            _logger.Debug("DayEasyServiceModule PreInitialize...");
            DatabaseInitializer.Initialize(IocManager.Resolve<IDbContextProvider<DayEasyDbContext>>().DbContext);
            DatabaseInitializer.Initialize(IocManager.Resolve<IDbContextProvider<Version3DbContext>>().DbContext);
        }

        //public override void Initialize()
        //{
        //    var cache = SystemCache.Instance;
        //    var contract = IocManager.Resolve<ISystemContract>();

        //    _logger.Info("设置科目缓存..");
        //    var subjects = contract.Subjects().Select(t => new
        //    {
        //        t.Id,
        //        t.SubjectName,
        //        t.IsLoadFormula,
        //        t.QTypeIDs
        //    }).ToList();
        //    cache.SetSubjectCache(subjects.ToDictionary(k => k.Id, v => v.SubjectName));
        //    cache.SetSubjectFormulaCache(subjects.ToDictionary(k => k.Id, v => v.IsLoadFormula));

        //    _logger.Info("设置科目题型缓存..");
        //    var types = contract.GetQuestionTypes();
        //    var dict = new Dictionary<int, List<QuestionTypeDto>>();
        //    foreach (var subject in subjects)
        //    {
        //        if (string.IsNullOrWhiteSpace(subject.QTypeIDs))
        //            dict.Add(subject.Id, new List<QuestionTypeDto>());
        //        else
        //        {
        //            var ids = subject.QTypeIDs.JsonToObject<int[]>();
        //            dict.Add(subject.Id, types.Where(t => ids.Contains(t.Id)).ToList());
        //        }
        //    }
        //    cache.SetSubjectQuestionTypes(dict);
        //    base.Initialize();
        //}
    }
}
