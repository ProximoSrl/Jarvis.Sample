using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Framework.Shared.IdentitySupport;

namespace Jarvis.Reservations.Domain.Resource
{
    public class ResourceId : EventStoreIdentity
    {
        public ResourceId(long id) : base(id)
        {
        }

        public ResourceId(string id) : base(id)
        {
        }
    }
}
