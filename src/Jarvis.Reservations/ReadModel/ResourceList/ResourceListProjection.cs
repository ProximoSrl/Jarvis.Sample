using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Framework.Kernel.Events;
using Jarvis.Framework.Kernel.ProjectionEngine;
using Jarvis.Framework.Kernel.ProjectionEngine.RecycleBin;
using Jarvis.Reservations.Domain.Resource;
using MongoDB.Driver.Builders;

namespace Jarvis.Reservations.ReadModel.ResourceList
{
    public class ResourceListProjection : AbstractProjection,
        IEventHandler<ResourceCreated>,
        IEventHandler<ResourceDeleted>
    {
        private readonly ICollectionWrapper<ResourceListReadModel, ResourceId> _list;
        private IRecycleBin _recycleBin;
        public ResourceListProjection(
            ICollectionWrapper<ResourceListReadModel, ResourceId> list,
            IRecycleBin recycleBin
        )
        {
            _list = list;
            _recycleBin = recycleBin;
            list.Attach(this,bEnableNotifications:false );
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
                Id = (ResourceId) e.AggregateId,
                Serial = e.Serial,
                Description = e.Description
            });
        }

        public void On(ResourceDeleted e)
        {
            _list.Delete(e,(ResourceId)e.AggregateId); 
            _recycleBin.Delete(e.AggregateId,"default",e.CommitStamp);
        }
    }
}
