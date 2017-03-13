using DayEasy.AutoMapper;
using DayEasy.Core.Modules;
using DayEasy.Utility.Logging;

namespace DayEasy.UnitTest.TestUtility
{
    [DependsOn(typeof(AutoMapperModule))]
    public class UnitTestModule : DModule
    {
        private readonly ILogger _logger = LogManager.Logger<UnitTestModule>();
        public override void Initialize()
        {
            base.Initialize();
            _logger.Debug("UnitTest Module Initialize...");
        }
    }
}
