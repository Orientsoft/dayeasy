using System.Web.Mvc;
using DayEasy.Utility;
using DayEasy.Utility.Logging;

namespace DayEasy.Web.Filters
{
    /// <summary> 得一平台异常处理特性 </summary>
    public class DExceptionAttribute : HandleErrorAttribute
    {
        private readonly ILogger _logger = LogManager.Logger<DExceptionAttribute>();
        private DExceptionAttribute()
        { }
        public static DExceptionAttribute Instance
        {
            get
            {
                return Singleton<DExceptionAttribute>.Instance ??
                       (Singleton<DExceptionAttribute>.Instance = new DExceptionAttribute());
            }
        }

        public override void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                var ex = filterContext.Exception;
                //记录日志
                _logger.Error(ex.Message, ex);
            }
            base.OnException(filterContext);
        }
    }
}
