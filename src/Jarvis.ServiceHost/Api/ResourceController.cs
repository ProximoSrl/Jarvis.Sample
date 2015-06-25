using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Jarvis.Framework.Shared.IdentitySupport;
using Jarvis.Framework.Shared.ReadModel;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence.EventStore;
using Jarvis.Reservations.Domain.Resource;
using Jarvis.Reservations.ReadModel.ResourceList;

namespace Jarvis.ServiceHost.Api
{
    [RoutePrefix("api/resource")]
    public class ResourceController : ApiController
    {
        public IRepositoryEx Repository { get; set; }
        public IIdentityGenerator IdentityGenerator { get; set; }
        public IConstructAggregatesEx AggregateFactory { get; set; }
        public IReader<ResourceListReadModel, ResourceId> Resources { get; set; }
        [Route("create")]
        [HttpGet]
        public string Create()
        {
            var id = IdentityGenerator.New<ResourceId>();
            var resource = (ResourceAggregate)AggregateFactory.Build(typeof (ResourceAggregate), id, null);
            resource.Create(DateTime.Now.Ticks.ToString(), "resource");
            Repository.Save(resource,Guid.NewGuid(), h => { });
            return id;
        }

        [HttpGet]
        [Route("list")]
        public IEnumerable<ResourceListReadModel> List()
        {
            var list = Resources.AllSortedById;
            return list;
        }

        [HttpGet]
        [Route("delete/{id}")]
        public void Delete(string id)
        {
            var typedId = new ResourceId(id);
            var aggregate = Repository.GetById<ResourceAggregate>(typedId);
            aggregate.Delete();
            Repository.Save(aggregate, Guid.NewGuid(), h => { });
        }
    }
}
