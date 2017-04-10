using System;
using System.Text;
using System.Threading;
using BitAuto.Ucar.Utils.Common;

namespace ConsulHelperDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if CORECLR
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            String mode = null;
            if (args.Length >= 1)
                mode = args[0];
            if (string.IsNullOrEmpty(mode))
            {
                Console.WriteLine("请输入rpc名称，对应Demo如下");
                Console.WriteLine("http:HttpDemo;");
                Console.WriteLine("thrift:ThriftDemo;");
                Console.WriteLine("grpc:GrpcDemo;");
                Console.WriteLine("wcf:WcfDemo;");
            }
            mode = Console.ReadLine().ToLower();
            switch (mode)
            {
                case "http":
                    HttpDemoConcurrentTest();
                    break;
                case "thrift":
                    ESThriftConcurrentTest();
                    break;
                case "grpc":
                    GrpcDemoConcurrentTest();
                    break;
                case "wcf":
                    WcfDemoConcurrentTest();
                    break;
                default:
                    Console.WriteLine("无法支持的协议Demo");
                    break;
            }
        }

        static void HttpDemoConcurrentTest()
        {
            var tasks = new Thread[10];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Thread(() =>
                {
                    for (var j = 0; j < 10000; j++ )
                    {
                        try
                        {
                            using (var client = ConsulHelper.Instance.GetServiceClient("httpdemo"))
                            {
                                var stub = client.GetStub<BitAuto.Ucar.Utils.Common.Service.Stub.HttpStub>();
                                var ret = stub.Get("/api/values").GetAwaiter().GetResult();
                                if (j % 100 == 0)
                                {
                                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + j);
                                    Console.WriteLine(ret);
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + err.Message);
                            if (err.InnerException != null)
                            {
                                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + err.InnerException.Message);
                            }
                        }
                    }
                });
                tasks[i].Start();
            }
            int idleCount = 0, activeCount = 0;
            do
            {
                Thread.Sleep(5000);
                var clientCount = ConsulHelper.Instance.GetServiceClientCount("httpdemo", "http");
                idleCount = clientCount.Item1;
                activeCount = clientCount.Item2;
                Console.WriteLine(string.Format("ClientCount<{0},{1}>", idleCount, activeCount));
            }
            while (idleCount != activeCount);
            Console.ReadLine();
        }

        static void ESThriftConcurrentTest()
        {
            var tasks = new Thread[10];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Thread(() =>
                {
                    for (var j = 0; j < 10000; j++ )
                    {
                        try
                        {
                            using (var client = ConsulHelper.Instance.GetServiceClient("thriftdemo"))
                            {
                                var stub = client.GetStub<ThriftDemo.DemoService.Client>();
                                var ret = stub.GetKeyValue(new ThriftDemo.ServiceKey()
                                {
                                    Key = "test"
                                });
                                if (string.IsNullOrEmpty(ret.Value))
                                {
                                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + ret.GetHashCode());
                                }
                                if (j % 100 == 0)
                                {
                                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + j);
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + err.Message);
                            if (err.InnerException != null)
                            {
                                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + err.InnerException.Message);
                            }
                        }
                    }
                });
                tasks[i].Start();
            }
            int idleCount = 0, activeCount = 0;
            do
            {
                Thread.Sleep(5000);
                var clientCount = ConsulHelper.Instance.GetServiceClientCount("thriftdemo", "thrift");
                idleCount = clientCount.Item1;
                activeCount = clientCount.Item2;
                Console.WriteLine(string.Format("ClientCount<{0},{1}>", idleCount, activeCount));
            }
            while (idleCount != activeCount);
            Console.ReadLine();

        }

        static void WcfDemoConcurrentTest()
        {
            var tasks = new Thread[10];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Thread(() =>
                {
                    for (var j = 0; j < 1000; j++)
                    {
                        try
                        {
                            using (var client = ConsulHelper.Instance.GetServiceClient("wcfdemo"))
                            {
                                var stub = client.GetStub<IDemoService>();
                                var ret = stub.GetDataAsync(10086).GetAwaiter().GetResult();
                                if (string.IsNullOrEmpty(ret))
                                {
                                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + string.Empty);
                                }
                                if (j % 100 == 0)
                                {
                                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + j);
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + err.Message);
                            if (err.InnerException != null)
                            {
                                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + err.InnerException.Message);
                            }
                        }
                    }
                });
                tasks[i].Start();
            }
            int idleCount = 0, activeCount = 0;
            do
            {
                Thread.Sleep(5000);
                var clientCount = ConsulHelper.Instance.GetServiceClientCount("wcfdemo", "wcf");
                idleCount = clientCount.Item1;
                activeCount = clientCount.Item2;
                Console.WriteLine(string.Format("ClientCount<{0},{1}>", idleCount, activeCount));
            }
            while (idleCount != activeCount);
            Console.ReadLine();
        }

        static void GrpcDemoConcurrentTest()
        {
            var tasks = new Thread[10];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Thread(() =>
                {
                    var key = new GrpcDemo.ServiceKey() { Key = "test"};
                    for (var j = 0; j < 1000; j++)
                    {
                        try
                        {
                            using (var client = ConsulHelper.Instance.GetServiceClient("grpcdemo"))
                            {
                                var stub = client.GetStub<GrpcDemo.DemoService.DemoServiceClient>();
                                var callOptions = new Grpc.Core.CallOptions().WithDeadline(DateTime.UtcNow.AddMilliseconds(5000));
                                var ret = stub.GetKeyValue(key, callOptions);
                                if (string.IsNullOrEmpty(ret.Value))
                                {
                                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + ret.GetHashCode());
                                }
                                if (j % 100 == 0)
                                {
                                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + j);
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + err.Message);
                            if (err.InnerException != null)
                            {
                                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + err.InnerException.Message);
                            }
                        }
                    }
                });
                tasks[i].Start();
            }
            int idleCount = 0, activeCount = 0;
            do
            {
                Thread.Sleep(5000);
                var clientCount = ConsulHelper.Instance.GetServiceClientCount("grpcdemo", "grpc");
                idleCount = clientCount.Item1;
                activeCount = clientCount.Item2;
                Console.WriteLine(string.Format("ClientCount<{0},{1}>", idleCount, activeCount));
            }
            while (idleCount != activeCount);
            Console.ReadLine();
        }
    }
}
