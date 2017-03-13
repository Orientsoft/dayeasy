using Autofac;
using DayEasy.Framework;
using DayEasy.Services;
using System;

namespace DayEasy.MigrateTools.Migrate
{
    public abstract class MigrateBase
    {
        protected DayEasyBootstrap Bootstrap { get; private set; }
        protected IContainer Container { get; private set; }
        protected Action<string> Log { get; set; }
        protected MigrateBase(Action<string> logAction = null)
        {
            Log = logAction ?? Console.WriteLine;
            Bootstrap = DayEasyBootstrap.Instance;
            Bootstrap.BuilderHandler += build =>
            {
                build.RegisterGeneric(typeof(DayEasyRepository<>)).As(typeof(IDayEasyRepository<>));
                build.RegisterGeneric(typeof(DayEasyRepository<,>)).As(typeof(IDayEasyRepository<,>));
                build.RegisterGeneric(typeof(Version3Repository<>)).As(typeof(IVersion3Repository<>));
                build.RegisterGeneric(typeof(Version3Repository<,>)).As(typeof(IVersion3Repository<,>));
            };
            Bootstrap.Initialize();
            Container = Bootstrap.Container;
        }
    }
}
