using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thrift.Server;
using Thrift.Transport;
using BitAuto.Ucar.Utils.Common;

namespace ThriftDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DemoServiceImpl handler = new DemoServiceImpl();
            DemoService.Processor processor = new DemoService.Processor(handler);
            TServerTransport serverTransport = new TServerSocket(9090);
            TServer server = new TSimpleServer(processor, serverTransport);

            // Use this for a multithreaded server
            // server = new TThreadPoolServer(processor, serverTransport);

            Console.WriteLine("Starting the server " + ConsulHelper.Instance.GetServiceName() + " ...");
            server.Serve();
        }
    }
}
