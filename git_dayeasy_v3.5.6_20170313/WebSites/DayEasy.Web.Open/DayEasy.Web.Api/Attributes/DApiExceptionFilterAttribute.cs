using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using DayEasy.Utility;
using DayEasy.Utility.Logging;

namespace DayEasy.Web.Api.Attributes
{
    public class DApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger _logger = LogManager.Logger<DApiExceptionFilterAttribute>();

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            _logger.Error(actionExecutedContext.Exception);

            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                HttpStatusCode.OK, DResult.Error(actionExecutedContext.Exception.Message));
        }
    }
}
