using DayEasy.Core;
using DayEasy.Management.Services.Helper;
using StackExchange.Profiling;
using StackExchange.Profiling.EntityFramework6;
using System;
using System.Reflection;

namespace DayEasy.Web.ManageMent
{
    public class MvcApplication : DApplication
    {
        public MvcApplication()
            : base(Assembly.GetExecutingAssembly())
        {
        }

        protected override void Application_Start(object sender, EventArgs e)
        {
            var site = Consts.Config.AdminSite;
            Consts.Website = site.Substring(site.IndexOf("//", StringComparison.Ordinal) + 2);
#if DEBUG
            MiniProfilerEF6.Initialize();
#endif
            base.Application_Start(sender, e);
            KnowledgeMover.Instance.MoveMission();
        }

        protected override void Application_BeginRequest(object sender, EventArgs e)
        {
#if DEBUG
            MiniProfiler.Start();
#endif
            base.Application_BeginRequest(sender, e);
        }

        protected override void Application_EndRequest(object sender, EventArgs e)
        {
#if DEBUG
            MiniProfiler.Stop();
#endif
            base.Application_EndRequest(sender, e);
        }
    }
}
