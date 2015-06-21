using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CommonDomain;
using CommonDomain.Core;
using Jarvis.Framework.Kernel.Engine;
using Jarvis.Framework.Kernel.Engine.Snapshots;
using Jarvis.Framework.Shared.IdentitySupport;
using Jarvis.Framework.Shared.IdentitySupport.Serialization;
using Jarvis.Framework.Shared.Storage;
using Jarvis.NEventStoreEx.CommonDomainEx;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence.EventStore;
using MongoDB.Driver;
using NEventStore;
using NEventStore.Dispatcher;

namespace Jarvis.ServiceHost.Support
{
    class DomainInstaller : IWindsorInstaller
    {
        private readonly BootstrapperConfig _config;

        public DomainInstaller(BootstrapperConfig config)
        {
            _config = config;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            RegisterGlobalComponents(container);
            RegisterMappings(container);
        }

        private void RegisterGlobalComponents(IWindsorContainer container)
        {
            container.Register(
                Component
                    .For<EventStoreFactory>()
                    .LifestyleSingleton(),
                Component
                    .For<IDispatchCommits>()
                    .ImplementedBy<NullDispatcher>(),
                Component
                    .For<IConstructAggregatesEx>()
                    .ImplementedBy<AggregateFactory>(),
                Component
                    .For<IDetectConflicts>()
                    .ImplementedBy<ConflictDetector>()
                    .LifestyleTransient(),
                Component
                    .For<ICounterService>()
                    .ImplementedBy<CounterService>()
                    .DependsOn(Dependency.OnValue<MongoDatabase>(_config.SystemDb))
                    ,
                Component
                    .For<IIdentityManager, IIdentityGenerator, IIdentityConverter, IdentityManager>()
                    .ImplementedBy<IdentityManager>(),
                Component
                    .For<IStoreEvents>()
                    .UsingFactory<EventStoreFactory, IStoreEvents>(f =>
                    {
                        var hooks = container.ResolveAll<IPipelineHook>();

                        return f.BuildEventStore(
                            _config.EventStoreConnectionString,
                            hooks
                        );
                    })
                    .LifestyleSingleton(),
                Component
                    .For<IRepositoryEx, RepositoryEx>()
                    .ImplementedBy<RepositoryEx>()
                    .LifestyleTransient()
            );

            foreach (var assembly in _config.Assemblies)
            {
                container.Register(
                    Classes
                        .FromAssembly(assembly)
                        .BasedOn<AggregateBase>()
                        .WithService.Self()
                        .LifestyleTransient(),
                     Classes
                        .FromAssembly(assembly)
                        .BasedOn<IPipelineHook>()
                        .WithServiceAllInterfaces()
               );
            }
        }

        private void RegisterMappings(IWindsorContainer container)
        {
            var identityManager = container.Resolve<IdentityManager>();

            EnableFlatIdMapping(identityManager);

            foreach (var assembly in _config.Assemblies)
            {
                MessagesRegistration.RegisterAssembly(assembly);
                SnapshotRegistration.AutomapAggregateState(assembly);

                identityManager.RegisterIdentitiesFromAssembly(assembly);
                IdentitiesRegistration.RegisterFromAssembly(assembly);
            }

            // value objects custom mappings
        }

        static void EnableFlatIdMapping(IdentityManager converter)
        {
            EventStoreIdentityBsonSerializer.IdentityConverter = converter;
        }
    }
}
