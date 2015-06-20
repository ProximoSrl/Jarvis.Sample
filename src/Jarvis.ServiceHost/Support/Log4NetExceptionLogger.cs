using System.Web.Http.ExceptionHandling;
using Castle.Core.Logging;

namespace Jarvis.ServiceHost.Support
{
    public class Log4NetExceptionLogger : ExceptionLogger
    {
        private readonly ILoggerFactory _loggerFactory;

        public Log4NetExceptionLogger(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public override void Log(ExceptionLoggerContext context)
        {
            var type = typeof(ApiApplication);
            if (context.ExceptionContext.ControllerContext != null)
            {
                type = context.ExceptionContext.ControllerContext.Controller.GetType();
            }
            var logger = _loggerFactory.Create(type);
            logger.ErrorFormat(context.Exception, "* * * * * * * * * * * *");
        }
    }
}