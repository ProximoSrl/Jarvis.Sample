using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jarvis.ServiceHost.Support;
using Topshelf;

namespace Jarvis.ServiceHost
{
    public class Program
    {
        static int Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                Banner();
            }

            var exitCode = HostFactory.Run(host =>
            {
                host.UseOldLog4Net("log4net.config");

                host.Service<Bootstrapper>(service =>
                {
                    service.ConstructUsing(() => new Bootstrapper());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });

                host.RunAsNetworkService();

                host.SetDescription("Jarvis - Sample App");
                host.SetDisplayName("Jarvis - Sample App");
                host.SetServiceName("JarvisSampleApp");
            });

            return (int)exitCode;
        }

        private static void Banner()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("===================================================================");
            Console.WriteLine("Jarvis Sample App - Proximo srl");
            Console.WriteLine("===================================================================");
            Console.WriteLine("  install                        -> install service");
            Console.WriteLine("  uninstall                      -> remove service");
            Console.WriteLine("  net start JarvisSampleApp      -> start service");
            Console.WriteLine("  net stop JarvisSampleApp       -> stop service");
            Console.WriteLine("===================================================================");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
