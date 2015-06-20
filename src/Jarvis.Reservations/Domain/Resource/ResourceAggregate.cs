using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Framework.Kernel.Engine;
using Jarvis.Framework.Shared.Events;

namespace Jarvis.Reservations.Domain.Resource
{
    public class ResourceAggregate : AggregateRoot<ResourceState>
    {
        public void Create(string serial, string description)
        {
            RaiseEvent(new ResourceCreated(serial, description));
        }
    }

    public class ResourceCreated : DomainEvent
    {
        public string Serial { get; private set; }
        public string Description { get; private set; }

        public ResourceCreated(string serial, string description)
        {
            Serial = serial;
            Description = description;
        }
    }

    public class ResourceState : AggregateState
    {

    }
}
