using DayEasy.Core;
using StackExchange.Profiling;
using StackExchange.Profiling.EntityFramework6;
using System;
using System.Reflection;

namespace DayEasy.Web.Portal
{
    public class MvcApplication : DApplication
    {
        public MvcApplication()
            : base(Assembly.GetExecutingAssembly())
        { }

        protected override void Application_Start(object sender, EventArgs e)
        {
            var site = Consts.Config.MainSite;
            Consts.Website = site.Substring(site.IndexOf("//", StringComparison.Ordinal) + 2);
#if DEBUG
            MiniProfilerEF6.Initialize();
#endif
            base.Application_Start(sender, e);
        }

        protected override void Application_BeginRequest(object sender, EventArgs e)
        {
            base.Application_BeginRequest(sender, e);
#if DEBUG
            MiniProfiler.Start();
#endif
        }

        protected override void Application_EndRequest(object sender, EventArgs e)
        {
            base.Application_EndRequest(sender, e);
#if DEBUG
            MiniProfiler.Stop();
#endif
        }
    }
}
