using System.Diagnostics;
using Autofac;
using DayEasy.Core;
using DayEasy.Core.Cache;
using DayEasy.Core.Dependency;
using DayEasy.EntityFramework;
using DayEasy.Framework.Logging;
using DayEasy.Utility;
using DayEasy.Utility.Logging;
using System.Linq;
using System.Reflection;
using System.Text;
using DayEasy.Utility.Extend;
using RedisCacheProvider = DayEasy.Framework.Cache.RedisCacheProvider;
using RuntimeMemoryCacheProvider = DayEasy.Framework.Cache.RuntimeMemoryCacheProvider;

namespace DayEasy.Framework
{
    public class DayEasyBootstrap : DBootstrap
    {
        private readonly ILogger _logger = LogManager.Logger<DayEasyBootstrap>();
        private DayEasyBootstrap() { }
        public static DayEasyBootstrap Instance
        {
            get
            {
                return (Singleton<DayEasyBootstrap>.Instance ??
                        (Singleton<DayEasyBootstrap>.Instance = new DayEasyBootstrap()));
            }
        }

        public ContainerBuilder Builder { get; private set; }

        private IContainer _container;

        public IContainer Container
        {
            get { return _container ?? (_container = Builder.Build()); }
        }

        public delegate void BuilderAction(ContainerBuilder builderAction);

        public event BuilderAction BuilderHandler;

        public override void Initialize(Assembly executingAssembly = null)
        {
#if DEBUG
            var sb = new StringBuilder();
            sb.AppendLine();
            var watch = new Stopwatch();
            watch.Start();
#endif

            LoggerInit();
            //设置以及缓存
            //CacheManager.SetProvider(CacheLevel.First, new RuntimeMemoryCacheProvider());
            CacheManager.SetProvider(CacheLevel.Second, new RedisCacheProvider());

#if DEBUG

            watch.Stop();
            sb.AppendLine("设置日志适配器，{0}ms".FormatWith(watch.ElapsedMilliseconds));
            watch.Restart();
#endif


            if (executingAssembly == null)
                executingAssembly = Assembly.GetExecutingAssembly();
            IocRegisters(executingAssembly);
            if (BuilderHandler != null)
                BuilderHandler(Builder);
            _container = Builder.Build();
            IocManager = _container.Resolve<IIocManager>();

#if DEBUG
            watch.Stop();
            sb.AppendLine("依赖注入，{0}ms".FormatWith(watch.ElapsedMilliseconds));
            watch.Restart();
#endif

            ModulesInstaller();

#if DEBUG
            watch.Stop();
            sb.AppendLine("项目模块加载器，{0}ms".FormatWith(watch.ElapsedMilliseconds));

            _logger.Info(sb);
#endif
        }

        /// <summary> 注册依赖 </summary>
        /// <param name="executingAssembly"></param>
        public override void IocRegisters(Assembly executingAssembly)
        {
            Builder = new ContainerBuilder();
            //            Builder.RegisterGeneric(typeof(EfRepository<,,>)).As(typeof(IRepository<,,>));
            Builder.RegisterGeneric(typeof(UnitOfWorkDbContextProvider<>))
                .As(typeof(IDbContextProvider<>))
                .InstancePerLifetimeScope();

            var assemblies = DayEasyAssemblyFinder.Instance.FindAll().Union(new[] { executingAssembly }).ToArray();
            Builder.RegisterAssemblyTypes(assemblies)
                .Where(type => typeof(ILifetimeDependency).IsAssignableFrom(type) && !type.IsAbstract)
                .AsSelf() //自身服务，用于没有接口的类
                .AsImplementedInterfaces() //接口服务
                .PropertiesAutowired() //属性注入
                .InstancePerLifetimeScope(); //保证生命周期基于请求

            Builder.RegisterAssemblyTypes(assemblies)
                .Where(type => typeof(IDependency).IsAssignableFrom(type) && !type.IsAbstract)
                .AsSelf() //自身服务，用于没有接口的类
                .AsImplementedInterfaces() //接口服务
                .PropertiesAutowired(); //属性注入
        }

        /// <summary> 初始化日志模块 </summary>
        public override void LoggerInit()
        {
            LogManager.AddAdapter(new Log4NetAdapter());
        }
    }
}
