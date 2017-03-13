using DayEasy.Core.Dependency;
using DayEasy.Core.Modules;
using DayEasy.Utility.Logging;

namespace DayEasy.Core
{
    public class CoreModule : DModule
    {
        private readonly ILogger _logger = LogManager.Logger<CoreModule>();

        public override void PreInitialize()
        {
            _logger.Info("设置IocManager...");
            CurrentIocManager.IocManager = IocManager;
        }
    }
}
