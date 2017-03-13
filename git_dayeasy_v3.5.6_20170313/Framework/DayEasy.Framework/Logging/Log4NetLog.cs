using System;
using System.Diagnostics;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using log4net.Core;
using ILogger = log4net.Core.ILogger;

namespace DayEasy.Framework.Logging
{
    public class Log4NetLog : LogBase
    {
        private readonly ILogger _logger;

        public Log4NetLog(ILoggerWrapper wrapper)
        {
            _logger = wrapper.Logger;
        }

        private static StackFrame CurrentStrace()
        {
            var strace = new StackTrace(true);
            for (var i = 0; i < strace.FrameCount; i++)
            {
                var frame = strace.GetFrame(i);
                if (string.IsNullOrWhiteSpace(frame.GetFileName()))
                    continue;
                var method = frame.GetMethod();
                if (method.Name == "OnException" || method.DeclaringType == typeof(LogBase) ||
                    method.DeclaringType == (typeof(LogManager)) ||
                    method.DeclaringType == typeof(Log4NetLog))
                    continue;
                return frame;
            }
            return new StackFrame(6, true);
        }

        private static LogInfo Format(object msgObject, Exception ex = null)
        {
            var frame = CurrentStrace();
            string method = string.Empty, fileInfo = string.Empty;
            if (frame != null)
            {
                method = string.Concat(frame.GetMethod().DeclaringType, " - ", frame.GetMethod().Name);
                fileInfo = string.Format("{0}[{1}]", frame.GetFileName(), frame.GetFileLineNumber());
            }
            if (ex != null && string.IsNullOrWhiteSpace(method))
            {
                method = string.Format("{0} - {1}", ex.TargetSite.DeclaringType, ex.TargetSite.Name);
            }
            string msg;
            if (msgObject == null || msgObject is string)
            {
                msg = (msgObject ?? string.Empty).ToString();
            }
            else
            {
                msg = JsonHelper.ToJson(msgObject);
            }
            var result = new LogInfo
            {
                Method = method,
                File = fileInfo,
                Message = msg,
                Detail = string.Empty
            };
            if (ex != null)
            {
                result.Detail = ex.Format();
            }
            return result;
        }

        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            _logger.Log(typeof(Log4NetLog), ParseLevel(level), Format(message, exception),
                exception);
        }

        public override bool IsTraceEnabled
        {
            get { return _logger.IsEnabledFor(Level.Trace); }
        }

        public override bool IsDebugEnabled
        {
            get { return _logger.IsEnabledFor(Level.Debug); }
        }

        public override bool IsInfoEnabled
        {
            get { return _logger.IsEnabledFor(Level.Info); }
        }

        public override bool IsWarnEnabled
        {
            get { return _logger.IsEnabledFor(Level.Warn); }
        }

        public override bool IsErrorEnabled
        {
            get { return _logger.IsEnabledFor(Level.Error); }
        }

        public override bool IsFatalEnabled
        {
            get { return _logger.IsEnabledFor(Level.Fatal); }
        }

        private Level ParseLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.All:
                    return Level.All;
                case LogLevel.Trace:
                    return Level.Trace;
                case LogLevel.Debug:
                    return Level.Debug;
                case LogLevel.Info:
                    return Level.Info;
                case LogLevel.Warn:
                    return Level.Warn;
                case LogLevel.Error:
                    return Level.Error;
                case LogLevel.Fatal:
                    return Level.Fatal;
                case LogLevel.Off:
                    return Level.Off;
                default:
                    return Level.Off;
            }
        }
    }
}
