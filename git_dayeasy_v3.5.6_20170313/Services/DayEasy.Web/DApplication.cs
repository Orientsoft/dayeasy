using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using DayEasy.Framework;
using DayEasy.Services;
using DayEasy.Utility.Logging;
using DayEasy.Web.Filters;

namespace DayEasy.Web
{
    public abstract class DApplication : HttpApplication
    {
        private readonly ILogger _logger = LogManager.Logger<DApplication>();
        protected DayEasyBootstrap Bootstrap { get; private set; }

        private readonly Assembly _executingAssembly;

        protected DApplication(Assembly executing)
        {
            Bootstrap = DayEasyBootstrap.Instance;
            _executingAssembly = executing;
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.LowercaseUrls = true;

            routes.MapMvcAttributeRoutes();

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            AreaRegistration.RegisterAllAreas();

            routes.MapRoute("share_views", "share/{resourceName}",
                new { controller = "Resource", action = "Index", resourcePath = "Views" });

            routes.MapRoute("Default", "{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }

        protected virtual void Application_Start(object sender, EventArgs e)
        {
            //路由注册
            RegisterRoutes(RouteTable.Routes);

            //MVC依赖注入
            Bootstrap.BuilderHandler += b =>
            {
                b.RegisterGeneric(typeof(DayEasyRepository<>)).As(typeof(IDayEasyRepository<>));
                b.RegisterGeneric(typeof(DayEasyRepository<,>)).As(typeof(IDayEasyRepository<,>));
                b.RegisterGeneric(typeof(Version3Repository<>)).As(typeof(IVersion3Repository<>));
                b.RegisterGeneric(typeof(Version3Repository<,>)).As(typeof(IVersion3Repository<,>));
                //mvc注入
                b.RegisterControllers(_executingAssembly).PropertiesAutowired();
                b.RegisterFilterProvider();
            };
            Bootstrap.Initialize(_executingAssembly);
            _logger.Info("Application_Start...");
            DependencyResolver.SetResolver(new AutofacDependencyResolver(Bootstrap.Container));
            //异常处理
            GlobalFilters.Filters.Add(DExceptionAttribute.Instance);
        }

        protected virtual void Application_End(object sender, EventArgs e)
        {
            _logger.Info("Application_End...");
            Bootstrap.Dispose();
        }

        protected virtual void Session_Start(object sender, EventArgs e)
        {
            _logger.Debug("Session_Start...");
        }

        protected virtual void Session_End(object sender, EventArgs e)
        {
            _logger.Debug("Session_End...");
        }
        protected virtual void Application_BeginRequest(object sender, EventArgs e)
        {
            _logger.Debug("Application_BeginRequest...");
        }

        protected virtual void Application_EndRequest(object sender, EventArgs e)
        {
            _logger.Debug("Application_EndRequest...");
        }

        protected virtual void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            _logger.Debug("Application_AuthenticateRequest...");
        }

        protected virtual void Application_Error(object sender, EventArgs e)
        {
            _logger.Debug("Application_Error...");
        }
    }
}
