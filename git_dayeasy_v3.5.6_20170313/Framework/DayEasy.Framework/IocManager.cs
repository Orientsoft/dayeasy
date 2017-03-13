using System;
using Autofac;
using DayEasy.Core.Dependency;

namespace DayEasy.Framework
{
    /// <summary> 依赖注入管理类 </summary>
    public class IocManager : IIocManager
    {
        private readonly DayEasyBootstrap _bootstrap;

        public IocManager()
        {
            _bootstrap = DayEasyBootstrap.Instance;
        }

        public T Resolve<T>()
        {
            return _bootstrap.Container.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return _bootstrap.Container.Resolve(type);
        }

        public void Register(Type serviceType, Type implementType, bool lifetime = true)
        {
            var builder = new ContainerBuilder();
            if (!lifetime)
                builder.RegisterGeneric(serviceType).As(implementType);
            else
                builder.RegisterGeneric(serviceType).As(implementType).InstancePerLifetimeScope();
            builder.Update(_bootstrap.Container);
        }
    }
}
