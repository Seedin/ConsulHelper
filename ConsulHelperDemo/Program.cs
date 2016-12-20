using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Diagnostics;
using BitAuto.Ucar.Utils.Common;

namespace ConsulHelperDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            KibanaHttpConcurrentTest();
            //ESThriftConcurrentTest();
            //WcfDemoConcurrentTest();
            //ESGrpcConcurrentTest();
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
                        }
                    }
                });
                tasks[i].Start();
            }
            int idleCount = 0, activeCount = 0;
            do
            {
                Thread.Sleep(5000);
                var clientCount = ConsulHelper.Instance.GetServiceClinetCount("kibana", "http");
                idleCount = clientCount.Item1;
                activeCount = clientCount.Item2;
                Console.WriteLine(string.Format("ClinetCount<{0},{1}>", idleCount, activeCount));
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
                        }
                    }
                });
                tasks[i].Start();
            }
            int idleCount = 0, activeCount = 0;
            do
            {
                Thread.Sleep(5000);
                var clientCount = ConsulHelper.Instance.GetServiceClinetCount("esthrift", "thrift");
                idleCount = clientCount.Item1;
                activeCount = clientCount.Item2;
                Console.WriteLine(string.Format("ClinetCount<{0},{1}>", idleCount, activeCount));
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
                                var ret = stub.GetData(10086);
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
                        }
                    }
                });
                tasks[i].Start();
            }
            int idleCount = 0, activeCount = 0;
            do
            {
                Thread.Sleep(5000);
                var clientCount = ConsulHelper.Instance.GetServiceClinetCount("wcfdemo", "wcf");
                idleCount = clientCount.Item1;
                activeCount = clientCount.Item2;
                Console.WriteLine(string.Format("ClinetCount<{0},{1}>", idleCount, activeCount));
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
                        }
                    }
                });
                tasks[i].Start();
            }
            int idleCount = 0, activeCount = 0;
            do
            {
                Thread.Sleep(5000);
                var clientCount = ConsulHelper.Instance.GetServiceClinetCount("esgrpc", "grpc");
                idleCount = clientCount.Item1;
                activeCount = clientCount.Item2;
                Console.WriteLine(string.Format("ClinetCount<{0},{1}>", idleCount, activeCount));
            }
            while (idleCount != activeCount);
            Console.ReadLine();
        }
    }
}
