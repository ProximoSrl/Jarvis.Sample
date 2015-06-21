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
        public string EventStoreConnectionString {
            get { return ConfigurationManager.ConnectionStrings["events"].ConnectionString; }
        }

        public IEnumerable<Assembly> Assemblies { get; private set; }

        public BootstrapperConfig()
        {
            this.ServerAddresses = new EditableList<string>();
            this.Assemblies = new[]{typeof(ResourceAggregate).Assembly};

            var systemDbUrl = new MongoUrl(ConfigurationManager.ConnectionStrings["system"].ConnectionString);
            this.SystemDb = new MongoClient(systemDbUrl).GetServer().GetDatabase(systemDbUrl.DatabaseName);
        }

        public MongoDatabase SystemDb { get; private set; }
    }
}