
using System;

namespace DayEasy.Core.Dependency
{
    public interface IIocManager : ILifetimeDependency
    {
        T Resolve<T>();
        object Resolve(Type type);

        void Register(Type serviceType, Type implementType, bool lifetime = true);
    }
}
