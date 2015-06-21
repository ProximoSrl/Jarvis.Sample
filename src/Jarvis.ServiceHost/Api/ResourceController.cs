using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Jarvis.Framework.Shared.IdentitySupport;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence.EventStore;
using Jarvis.Reservations.Domain.Resource;

namespace Jarvis.ServiceHost.Api
{
    [RoutePrefix("api/resource")]
    public class ResourceController : ApiController
    {
        public IRepositoryEx Repository { get; set; }
        public IIdentityGenerator IdentityGenerator { get; set; }
        public IConstructAggregatesEx AggregateFactory { get; set; }
        [Route("create")]
        [HttpGet]
        public string Create()
        {
            var id = IdentityGenerator.New<ResourceId>();
            var resource = (ResourceAggregate)AggregateFactory.Build(typeof (ResourceAggregate), id, null);
            resource.Create("01", "first resource");
            Repository.Save(resource,Guid.NewGuid(), h => { });
            return id;
        }
    }
}
