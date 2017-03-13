using DayEasy.AutoMapper;
using DayEasy.Core;
using DayEasy.Core.Modules;
using DayEasy.Models.Open.System;
using DayEasy.Web.Api.Config;

namespace DayEasy.Web.Api
{
    [DependsOn(typeof(CoreModule))]
    public class OpenModule : DModule
    {
        public override void Initialize()
        {
            AutoMapperHelper.CreateMapper<ManifestInfo, MManifestDto>();

            base.Initialize();
        }
    }
}
