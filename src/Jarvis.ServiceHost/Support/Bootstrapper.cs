using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Framework.Shared.IdentitySupport;

namespace Jarvis.ServiceHost.Support
{
    public class Bootstrapper
    {
        public static void Init()
        {
            MongoFlatMapper.EnableFlatMapping(); //before any chanche that the driver scan any type.
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }
    }
}
