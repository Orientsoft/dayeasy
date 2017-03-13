using DayEasy.Core;
using DayEasy.Core.Modules;
using DayEasy.Utility.Logging;

namespace DayEasy.MongoDb
{
    /// <summary> MongoDB模块 </summary>
    [DependsOn(typeof(CoreModule))]
    public class MongoDbModule : DModule
    {
        private readonly ILogger _logger = LogManager.Logger<MongoDbModule>();
        public override void Initialize()
        {
            _logger.Debug("MongoDb Initialize...");
        }
    }
}
