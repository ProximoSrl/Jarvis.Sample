using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Framework.Shared.ReadModel;
using Jarvis.Reservations.Domain.Resource;

namespace Jarvis.Reservations.ReadModel.ResourceList
{
    public class ResourceListReadModel : AbstractReadModel<ResourceId>
    {
        public string Serial { get; set; }
        public string Description { get; set; }
    }
}
