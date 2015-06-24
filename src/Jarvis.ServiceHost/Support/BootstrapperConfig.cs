using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Castle.Components.DictionaryAdapter;
using Jarvis.Reservations.Domain.Resource;
using MongoDB.Driver;

namespace Jarvis.ServiceHost.Support
{
    public class BootstrapperConfig
    {
        public List<string> ServerAddresses { get; private set; }
        public string EventStoreConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["events"].ConnectionString; }
        }

        public IEnumerable<Assembly> Assemblies { get; private set; }

        public BootstrapperConfig()
        {
            this.ServerAddresses = new EditableList<string>();
            this.Assemblies = new[] { typeof(ResourceAggregate).Assembly };

            var systemDbUrl = new MongoUrl(ConfigurationManager.ConnectionStrings["system"].ConnectionString);
            this.SystemDb = new MongoClient(systemDbUrl).GetServer().GetDatabase(systemDbUrl.DatabaseName);

            var readModelDbUrl = new MongoUrl(ConfigurationManager.ConnectionStrings["readmodel"].ConnectionString);
            this.ReadModelDb = new MongoClient(readModelDbUrl).GetServer().GetDatabase(readModelDbUrl.DatabaseName);
        }

        public MongoDatabase SystemDb { get; private set; }
        public MongoDatabase ReadModelDb { get; private set; }
        public string Boost { get { return "true"; } }
        public string[] EngineSlots { get { return "*".Split('|'); } }
        public int PollingMsInterval { get { return 1000; } }
        public int DelayedStartInMilliseconds { get { return 1000; } }
    }
}