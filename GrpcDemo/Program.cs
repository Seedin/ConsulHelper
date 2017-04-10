using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using BitAuto.Ucar.Utils.Common;

namespace GrpcDemo
{
    public class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { DemoService.BindService(new DemosSrviceImpl()) },
                Ports = { new ServerPort("", Port, ServerCredentials.Insecure) }
            };
            server.Start();
            
            Console.WriteLine(ConsulHelper.Instance.GetServiceName() + " listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
