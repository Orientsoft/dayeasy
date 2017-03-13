using DayEasy.Core;
using System;
using System.Reflection;

namespace DayEasy.Web.Account
{
    public class MvcApplication : DApplication
    {
        protected MvcApplication()
            : base(Assembly.GetExecutingAssembly())
        {
        }

        protected override void Application_Start(object sender, EventArgs e)
        {
            var site = Consts.Config.AccountSite;
            Consts.Website = site.Substring(site.IndexOf("//", StringComparison.Ordinal) + 2);
            base.Application_Start(sender, e);
        }
    }
}
