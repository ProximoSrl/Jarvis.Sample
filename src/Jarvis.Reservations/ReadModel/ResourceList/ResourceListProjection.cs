using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Framework.Kernel.Events;
using Jarvis.Framework.Kernel.ProjectionEngine;
using Jarvis.Reservations.Domain.Resource;
using MongoDB.Driver.Builders;

namespace Jarvis.Reservations.ReadModel.ResourceList
{
    public class ResourceListProjection : AbstractProjection,
        IEventHandler<ResourceCreated>
    {
        private readonly ICollectionWrapper<ResourceListReadModel, ResourceId> _list;

        public ResourceListProjection(ICollectionWrapper<ResourceListReadModel, ResourceId> list)
        {
            _list = list;
        }

        public override void Drop()
        {
            _list.Drop();
        }

        public override void SetUp()
        {
            _list.CreateIndex(IndexKeys<ResourceListReadModel>.Ascending(x=>x.Serial));
        }

        public void On(ResourceCreated e)
        {
            _list.Insert(e, new ResourceListReadModel()
            {
                Serial = e.Serial,
                Description = e.Description
            });
        }
    }
}
