using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Services.Logging.Log4netIntegration;
using Castle.Windsor;
using Jarvis.Framework.Kernel.ProjectionEngine;
using Jarvis.Framework.Shared.IdentitySupport;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence;
using Jarvis.Reservations.Domain.Resource;
using Microsoft.Owin.Hosting;

namespace Jarvis.ServiceHost.Support
{
    public class Bootstrapper
    {
        private IWindsorContainer _container;
        private IDisposable _webApplication;
        private ConcurrentProjectionsEngine _projections;

        public Bootstrapper()
        {
            MongoFlatMapper.EnableFlatMapping(); //before any chanche that the driver scan any type.
        }

        private void ConfigureContainer(BootstrapperConfig config)
        {
            _container = new WindsorContainer();
            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel, true));
            _container.Kernel.Resolver.AddSubResolver(new ArrayResolver(_container.Kernel, true));

            _container.AddFacility<LoggingFacility>(f => f.LogUsing(new ExtendedLog4netFactory("log4net.config")));
            _container.Install(
                new DomainInstaller(config),
                new ApiInstaller()
            );

            ApiApplication.Container = _container;
        }

        public void Start(BootstrapperConfig config)
        {
            ConfigureContainer(config);

            var options = new StartOptions();
            foreach (var uri in config.ServerAddresses)
            {
                options.Urls.Add(uri);
            }
            _projections = _container.Resolve<ConcurrentProjectionsEngine>();
            _projections.Start();

            _webApplication = WebApp.Start<ApiApplication>(options);
        }

        public void Stop()
        {
            if (_projections != null)
            {
                _projections.Stop();
                _projections = null;
            }

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
