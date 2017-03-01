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
                Console.WriteLine("http:KibanaHttp;");
                Console.WriteLine("thrift:ESThrift;");
                Console.WriteLine("grpc:ESGrpc;");
                Console.WriteLine("wcf:WcfDemo;");
            }
            mode = Console.ReadLine().ToLower();
            switch (mode)
            {
                case "http":
                    KibanaHttpConcurrentTest();
                    break;
                case "thrift":
                    ESThriftConcurrentTest();
                    break;
                case "grpc":
                    ESGrpcConcurrentTest();
                    break;
                case "wcf":
                    WcfDemoConcurrentTest();
                    break;
                default:
                    Console.WriteLine("无法支持的协议Demo");
                    break;
            }
        }

        static void KibanaHttpConcurrentTest()
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
                            using (var client = ConsulHelper.Instance.GetServiceClient("kibana"))
                            {
                                var stub = client.GetStub<BitAuto.Ucar.Utils.Common.Service.Stub.HttpStub>();
                                var ret = stub.PostJson<ConsulHelperDemo.Http.ESPara, ConsulHelperDemo.Http.ESSearch>(
                                    "/elasticsearch/.kibana/index-pattern/_search", new Http.ESPara() { })
                                    .GetAwaiter().GetResult();
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
                var clientCount = ConsulHelper.Instance.GetServiceClientCount("kibana", "http");
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
                            using (var client = ConsulHelper.Instance.GetServiceClient("esthrift"))
                            {
                                var stub = client.GetStub<Taoche.ES.TaocheESService.Client>();
                                var ret = stub.SearchTaocheCar(new Taoche.ES.DTOSearchCondition()
                                {
                                    RequestSource = 9,
                                    CommonFlag = 11
                                });
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
                var clientCount = ConsulHelper.Instance.GetServiceClientCount("esthrift", "thrift");
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

        static void ESGrpcConcurrentTest()
        {
            var tasks = new Thread[10];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Thread(() =>
                {
                    var param = new TaocheES.SearchCondition();
                    param.RequestSource = 9;
                    param.PageIndex = 1;
                    param.PageSize = 200;
                    param.ReturnFieldArray.Add("ucarid");
                    param.ReturnFieldArray.Add("userid");
                    param.ReturnFieldArray.Add("color");
                    param.ReturnFieldArray.Add("displayprice");
                    param.ReturnFieldArray.Add("cartitle");
                    param.CommonFlag = 0;
                    for (var j = 0; j < 1000; j++)
                    {
                        try
                        {
                            using (var client = ConsulHelper.Instance.GetServiceClient("esgrpc"))
                            {
                                var stub = client.GetStub<TaocheES.TaocheESService.TaocheESServiceClient>();
                                var ret = stub.SearchTaocheCar(param);
                                if (ret.Count <= 0)
                                {
                                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + ret.Count);
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
                var clientCount = ConsulHelper.Instance.GetServiceClientCount("esgrpc", "grpc");
                idleCount = clientCount.Item1;
                activeCount = clientCount.Item2;
                Console.WriteLine(string.Format("ClientCount<{0},{1}>", idleCount, activeCount));
            }
            while (idleCount != activeCount);
            Console.ReadLine();
        }
    }
}
