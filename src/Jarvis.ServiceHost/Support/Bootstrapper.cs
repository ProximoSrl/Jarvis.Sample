using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.Logging;
using Castle.Services.Logging.Log4netIntegration;
using Castle.Windsor;
using Jarvis.Framework.Shared.IdentitySupport;
using Microsoft.Owin.Hosting;

namespace Jarvis.ServiceHost.Support
{
    public class Bootstrapper
    {
        private IWindsorContainer _container;
        private IDisposable _webApplication;

        public Bootstrapper()
        {
            MongoFlatMapper.EnableFlatMapping(); //before any chanche that the driver scan any type.
            ConfigureContainer();
        }

        private void ConfigureContainer()
        {
            _container = new WindsorContainer();
            _container.AddFacility<LoggingFacility>(f => f.LogUsing(new ExtendedLog4netFactory("log4net.config")));
            _container.Install(
                new DomainInstaller(),
                new ApiInstaller()
            );

            ApiApplication.Container = _container;
        }

        public void Start(BootstrapperConfig config)
        {
            ConfigureContainer();

            var options = new StartOptions();
            foreach (var uri in config.ServerAddresses)
            {
                options.Urls.Add(uri);
            }

            _webApplication = WebApp.Start<ApiApplication>(options);
        }

        public void Stop()
        {
            if (_webApplication != null)
            {
                _webApplication.Dispose();
                _webApplication = null;
            }

            if (_container != null)
            {
                _container.Dispose();
                ApiApplication.Container = null;
                _container = null;
            }
        }
    }
}
