using DayEasy.Core.Modules;
using DayEasy.Utility.Logging;

namespace DayEasy.Web
{
    public class WebModule : DModule
    {
        private readonly ILogger _logger = LogManager.Logger<WebModule>();
        public override void Initialize()
        {
            base.Initialize();
            _logger.Debug("WebModule Initialize...");
        }
    }
}
