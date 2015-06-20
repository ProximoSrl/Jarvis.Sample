using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Jarvis.ServiceHost.Api
{
    [Route("api/echo")]
    public class EchoController : ApiController
    {
        [HttpGet]
        public string Get()
        {
            return "alive & kicking";
        }
    }
}
