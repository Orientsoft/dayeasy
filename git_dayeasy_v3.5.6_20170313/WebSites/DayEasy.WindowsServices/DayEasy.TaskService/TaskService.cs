using Autofac;
using DayEasy.AsyncMission.Jobs;
using DayEasy.Core;
using DayEasy.Core.Cache;
using DayEasy.Framework;
using DayEasy.Services;
using System.Reflection;
using System.ServiceProcess;

namespace DayEasy.TaskService
{
    public partial class TaskService : ServiceBase
    {
        public TaskService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Consts.Website = "task.service";
            var bootstrap = DayEasyBootstrap.Instance;
            bootstrap.BuilderHandler += build =>
            {
                build.RegisterGeneric(typeof(DayEasyRepository<>)).As(typeof(IDayEasyRepository<>));
                build.RegisterGeneric(typeof(DayEasyRepository<,>)).As(typeof(IDayEasyRepository<,>));
                build.RegisterGeneric(typeof(Version3Repository<>)).As(typeof(IVersion3Repository<>));
                build.RegisterGeneric(typeof(Version3Repository<,>)).As(typeof(IVersion3Repository<,>));
            };
            bootstrap.Initialize(Assembly.GetExecutingAssembly());
            //去除runtime缓存
            CacheManager.RemoveProvider(CacheLevel.First);
            Setup.Start();
        }

        protected override void OnStop()
        {
            Setup.ShutDown();
        }
    }
}
