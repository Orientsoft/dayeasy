

namespace DayEasy.Core.Dependency
{
    public static class CurrentIocManager
    {
        public static IIocManager IocManager { get; internal set; }

        public static T Resolve<T>()
        {
            return IocManager.Resolve<T>();
        }
    }
}
