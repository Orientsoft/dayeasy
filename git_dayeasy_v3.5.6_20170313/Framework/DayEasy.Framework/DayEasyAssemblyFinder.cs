using DayEasy.Core;
using DayEasy.Core.Reflection;
using DayEasy.Utility;

namespace DayEasy.Framework
{
    public class DayEasyAssemblyFinder : DAssemblyFinder
    {
        public DayEasyAssemblyFinder()
            : base(Consts.AssemblyFinder)
        {
        }

        public static DayEasyAssemblyFinder Instance
        {
            get
            {
                return
                    Singleton<DayEasyAssemblyFinder>.Instance ??
                    (Singleton<DayEasyAssemblyFinder>.Instance = new DayEasyAssemblyFinder());
            }
        }
    }
}
