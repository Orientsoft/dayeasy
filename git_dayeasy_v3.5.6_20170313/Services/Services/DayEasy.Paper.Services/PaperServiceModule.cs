using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Core;
using DayEasy.Core.Modules;

namespace DayEasy.Paper.Services
{
    /// <summary> 试卷模块 </summary>
    [DependsOn(typeof(CoreModule))]
    public class PaperServiceModule : DModule
    {
        public override void Initialize()
        {
            AutoMapperHelper.CreateMapper<PaperDto, NPaperDto>();
            base.Initialize();
        }
    }
}
