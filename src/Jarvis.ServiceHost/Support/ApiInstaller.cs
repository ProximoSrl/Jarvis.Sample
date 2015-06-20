using System.Web.Http;
using System.Web.Http.Controllers;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Jarvis.ServiceHost.Support
{
    class ApiInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromThisAssembly()
                .BasedOn<IHttpController>()
                .WithServiceSelf()
                .LifestyleTransient()
            );
        }
    }
}