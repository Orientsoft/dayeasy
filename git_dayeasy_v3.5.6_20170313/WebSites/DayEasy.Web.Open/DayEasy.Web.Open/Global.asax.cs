using DayEasy.Core;
using DayEasy.Web.Api;
using System;
using System.Reflection;

namespace DayEasy.Web.Open
{
    public class WebApiApplication : DApiApplication
    {
        protected WebApiApplication() :
            base(Assembly.GetExecutingAssembly())
        {
        }

        protected override void Application_Start(object sender, EventArgs e)
        {
            var site = Consts.Config.OpenSite;
            Consts.Website = site.Substring(site.IndexOf("//", StringComparison.Ordinal) + 2);
            base.Application_Start(sender, e);
        }
    }
}
