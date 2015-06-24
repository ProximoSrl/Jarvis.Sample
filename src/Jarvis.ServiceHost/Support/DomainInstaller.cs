using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CommonDomain;
using CommonDomain.Core;
using Jarvis.Framework.Kernel.Engine;
using Jarvis.Framework.Kernel.Engine.Snapshots;
using Jarvis.Framework.Kernel.Events;
using Jarvis.Framework.Kernel.ProjectionEngine;
using Jarvis.Framework.Kernel.ProjectionEngine.Client;
using Jarvis.Framework.Kernel.ProjectionEngine.RecycleBin;
using Jarvis.Framework.Shared.IdentitySupport;
using Jarvis.Framework.Shared.IdentitySupport.Serialization;
using Jarvis.Framework.Shared.Messages;
using Jarvis.Framework.Shared.ReadModel;
using Jarvis.Framework.Shared.Storage;
using Jarvis.NEventStoreEx.CommonDomainEx;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence.EventStore;
using MongoDB.Driver;
using NEventStore;
using NEventStore.Dispatcher;

namespace Jarvis.ServiceHost.Support
{
    internal class DomainInstaller : IWindsorInstaller
    {
        private readonly BootstrapperConfig _config;

        public DomainInstaller(BootstrapperConfig config)
        {
            _config = config;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            RegisterMappings(container);
            RegisterEventStore(container);
            RegisterReadModel(container);
        }

        private void RegisterReadModel(IWindsorContainer container)
        {
            var config = new ProjectionEngineConfig
            {
                EventStoreConnectionString = _config.EventStoreConnectionString,
                Slots = _config.EngineSlots,
                PollingMsInterval = _config.PollingMsInterval,
                ForcedGcSecondsInterval = 600,
                DelayedStartInMilliseconds = _config.DelayedStartInMilliseconds
            };

            foreach (var assembly in _config.Assemblies)
            {
                container.Register(
                    Classes
                        .FromAssembly(assembly)
                        .BasedOn<IProjection>()
                        .WithServiceAllInterfaces()
                        .LifestyleSingleton()
                    );
            }

            container.Register(
                Component
                    .For<IHousekeeper>()
                    .ImplementedBy<NullHouseKeeper>(),
                Component
                    .For<INotifyToSubscribers>()
                    .ImplementedBy<NotifyToNobody>(),
                Component
                    .For<ICommitEnhancer>()
                    .ImplementedBy<CommitEnhancer>(),
                Component
                    .For<INotifyCommitHandled>()
                    .ImplementedBy<NullNotifyCommitHandled>(),
                Component
                    .For(typeof (IReader<,>), typeof (IMongoDbReader<,>))
                    .ImplementedBy(typeof (MongoReaderForProjections<,>))
                    .DependsOn(Dependency.OnValue<MongoDatabase>(_config.ReadModelDb)),
                Component
                    .For<IInitializeReadModelDb>()
                    .ImplementedBy<InitializeReadModelDb>(),
                Component
                    .For<IConcurrentCheckpointTracker>()
                    .ImplementedBy<ConcurrentCheckpointTracker>()
                    .DependsOn(Dependency.OnValue<MongoDatabase>(_config.ReadModelDb)),
                Component
                    .For(typeof (ICollectionWrapper<,>), typeof (IReadOnlyCollectionWrapper<,>))
                    .ImplementedBy(typeof (CollectionWrapper<,>))
                    .DependsOn(Dependency.OnValue<MongoDatabase>(_config.ReadModelDb)),
                Component
                    .For<IPollingClient>()
                    .ImplementedBy<PollingClientWrapper>()
                    .DependsOn(Dependency.OnConfigValue("boost", _config.Boost)),
                Component
                    .For<IRebuildContext>()
                    .ImplementedBy<RebuildContext>()
                    .DependsOn(Dependency.OnValue<bool>(RebuildSettings.NitroMode)),
                Component
                    .For<IMongoStorageFactory>()
                    .ImplementedBy<MongoStorageFactory>()
                    .DependsOn(Dependency.OnValue<MongoDatabase>(_config.ReadModelDb)),
                Component
                    .For<IRecycleBin>()
                    .ImplementedBy<RecycleBin>()
                    .DependsOn(Dependency.OnValue<MongoDatabase>(_config.ReadModelDb)),
                Component
                    .For<ConcurrentProjectionsEngine, ITriggerProjectionsUpdate>()
                    .ImplementedBy<ConcurrentProjectionsEngine>()
                    .LifestyleSingleton()
                    .DependsOn(Dependency.OnValue<ProjectionEngineConfig>(config))
                );
        }

        private void RegisterEventStore(IWindsorContainer container)
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
            container.Register(
                Component
                    .For<ICounterService>()
                    .ImplementedBy<CounterService>()
                    .DependsOn(Dependency.OnValue<MongoDatabase>(_config.SystemDb)),
                Component
                    .For<IIdentityManager, IIdentityGenerator, IIdentityConverter, IdentityManager>()
                    .ImplementedBy<IdentityManager>()
                );


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

        private static void EnableFlatIdMapping(IdentityManager converter)
        {
            EventStoreIdentityBsonSerializer.IdentityConverter = converter;
        }
    }
}