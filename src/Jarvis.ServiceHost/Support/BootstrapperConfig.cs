using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;

namespace Jarvis.ServiceHost.Support
{
    public class BootstrapperConfig
    {
        public List<string> ServerAddresses { get; private set; }

        public BootstrapperConfig()
        {
            this.ServerAddresses = new EditableList<string>();
        }
    }
}