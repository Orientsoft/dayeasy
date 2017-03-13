using DayEasy.Core;
using DayEasy.Utility.Logging;
using DayEasy.Web.File.Logging;
using System;

namespace DayEasy.Web.File
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Consts.Website = "file.deyi.com";
            LogManager.AddAdapter(new Log4NetAdapter());
            LogManager.Logger("Global").Info("Application_Start..");
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            LogManager.Logger("Global").Info("Application_Start..");
        }
    }
}