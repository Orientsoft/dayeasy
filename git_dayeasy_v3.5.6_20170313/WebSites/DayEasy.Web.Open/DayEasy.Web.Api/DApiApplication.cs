
using System;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using DayEasy.Framework;
using DayEasy.Services;
using DayEasy.Utility.Logging;
using DayEasy.Web.Api.Attributes;
using DayEasy.Web.Api.Formatters;
using Newtonsoft.Json.Serialization;

namespace DayEasy.Web.Api
{
    public abstract class DApiApplication : HttpApplication
    {
        private readonly ILogger _logger = LogManager.Logger<DApiController>();
        protected DayEasyBootstrap Bootstrap { get; private set; }

        private readonly Assembly _executingAssembly;

        protected DApiApplication(Assembly executing)
        {
            Bootstrap = DayEasyBootstrap.Instance;
            _executingAssembly = executing;
        }

        private static void RegisterRoutes(HttpConfiguration config)
        {
            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("DefaultApi", "v3/{controller}_{action}");
        }

        protected virtual void Application_Start(object sender, EventArgs e)
        {
            //路由注册
            GlobalConfiguration.Configure(RegisterRoutes);

            var config = GlobalConfiguration.Configuration;

            var jsonFormatter = new JsonMediaTypeFormatter
            {
                SerializerSettings = { ContractResolver = new CamelCasePropertyNamesContractResolver() }
            };

            //默认json
            config.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));
            //jsonp格式支持
            //            config.Formatters.Insert(0, new DJsonpFormatter());
            //全局签名验证
            //            config.Filters.Add(new DApiAuthorizeAttribute());

            //MVC依赖注入
            Bootstrap.BuilderHandler += b =>
            {
                b.RegisterGeneric(typeof(DayEasyRepository<>)).As(typeof(IDayEasyRepository<>));
                b.RegisterGeneric(typeof(DayEasyRepository<,>)).As(typeof(IDayEasyRepository<,>));
                b.RegisterGeneric(typeof(Version3Repository<>)).As(typeof(IVersion3Repository<>));
                b.RegisterGeneric(typeof(Version3Repository<,>)).As(typeof(IVersion3Repository<,>));

                b.RegisterApiControllers(_executingAssembly).PropertiesAutowired();
                b.RegisterWebApiFilterProvider(config);
            };
            Bootstrap.Initialize(_executingAssembly);
            _logger.Info("Application_Start...");
            //设置依赖
            config.DependencyResolver = new AutofacWebApiDependencyResolver(Bootstrap.Container);
            //异常处理
            config.Filters.Add(new DApiExceptionFilterAttribute());
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
