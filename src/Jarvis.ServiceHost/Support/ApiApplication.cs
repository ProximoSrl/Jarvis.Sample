using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;
using Castle.Core.Logging;
using Castle.Windsor;
using Jarvis.Framework.Shared.Domain.Serialization;
using Newtonsoft.Json.Serialization;
using Owin;

namespace Jarvis.ServiceHost.Support
{
    public class ApiApplication
    {
        public static IWindsorContainer Container  = null;
        public void Configuration(IAppBuilder application)
        {
            ConfigureApi(application);
        }

        private void ConfigureApi(IAppBuilder application)
        {
            var config = new HttpConfiguration
            {
                DependencyResolver = new WindsorResolver(Container)
            };

            config.MapHttpAttributeRoutes();
            config.Routes.Add("default", new HttpRoute(
                "api/{controller}/{action}"
            ));

            var jsonFormatter = new JsonMediaTypeFormatter();
            jsonFormatter.SerializerSettings.Converters.Add(new StringValueJsonConverter());
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));

            config.Services.Add(
                typeof(IExceptionLogger),
                new Log4NetExceptionLogger(Container.Resolve<ILoggerFactory>())
            );

            config.EnsureInitialized();

            application.UseWebApi(config);
        }
    }
}