using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.Logging;
using Castle.Services.Logging.Log4netIntegration;
using Castle.Windsor;
using Jarvis.Framework.Shared.IdentitySupport;

namespace Jarvis.ServiceHost.Support
{
    public class Bootstrapper
    {
        private IWindsorContainer _container = new WindsorContainer();
        public Bootstrapper()
        {
            MongoFlatMapper.EnableFlatMapping(); //before any chanche that the driver scan any type.
            ConfigureContainer();
        }

        private void ConfigureContainer()
        {
            _container.AddFacility<LoggingFacility>(f => f.LogUsing(new ExtendedLog4netFactory("log4net.config")));
            _container.Install(
                new DomainInstaller(),
                new ApiInstaller()
            );
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }
    }
}
