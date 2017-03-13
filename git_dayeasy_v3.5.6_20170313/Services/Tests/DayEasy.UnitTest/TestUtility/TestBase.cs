using System;
using Autofac;
using DayEasy.Framework;
using DayEasy.Services;
using DayEasy.Utility.Helper;

namespace DayEasy.UnitTest.TestUtility
{
    public abstract class TestBase
    {
        protected DayEasyBootstrap Bootstrap { get; private set; }
        protected IContainer Container { get; private set; }
        protected TestBase()
        {
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

        protected void WriteJson(object obj)
        {
            Console.WriteLine(JsonHelper.ToJson(obj, NamingType.CamelCase, true));
        }
    }
}
