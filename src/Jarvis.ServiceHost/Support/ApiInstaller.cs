using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using Castle.Core.Logging;
using Castle.DynamicProxy.Internal;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Jarvis.ServiceHost.Support
{
    internal class Dipendenza
    {
    }

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