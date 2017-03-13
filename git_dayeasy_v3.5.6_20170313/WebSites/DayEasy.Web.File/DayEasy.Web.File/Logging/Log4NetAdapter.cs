using DayEasy.Core;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using log4net.Config;
using System.IO;

namespace DayEasy.Web.File.Logging
{
    public class Log4NetAdapter : LoggerAdapterBase
    {
        private const string FileName = "filelog4net.config";

        private static string ConfigPath
        {
            get { return ConfigHelper.GetAppSetting(defaultValue: string.Empty); }
        }

        /// <summary>
        /// 初始化一个<see cref="Log4NetAdapter"/>类型的新实例
        /// </summary>k
        public Log4NetAdapter()
        {
            var configFile = Path.Combine(ConfigPath, FileName);
            if (!System.IO.File.Exists(configFile))
                return;
            log4net.GlobalContext.Properties["WebSite"] = Consts.Website ?? "dayeasy";
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configFile));
        }

        protected override ILog CreateLogger(string name)
        {
            var log = log4net.LogManager.GetLogger(name);
            return new Log4NetLog(log);
        }
    }
}
