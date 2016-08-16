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
            //LocalCacheConcurrentTest();
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
                                    "/elasticsearch/.kibana/index-pattern/_search", new Http.ESPara() { fields = "" }, false)
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

        static void LocalCacheConcurrentTest()
        {
            var tasks = new Thread[10];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Thread(() =>
                {
                    for (var j = 0; j < 10000; j++)
                    {
                        try
                        {
                            using (var client = ConsulHelper.Instance.GetServiceClient("localCache"))
                            {
                                var stub = client.GetStub<ILocalCacheService>();
                                var ret = stub.GetValueInTime(string.Format("YcZongshuCarSerialNew\\15{0}\\0.json"
                                , new Random().Next(40) + 60)
                                , TimeSpan.Zero);
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
                var clientCount = ConsulHelper.Instance.GetServiceClinetCount("localCache", "wcf");
                idleCount = clientCount.Item1;
                activeCount = clientCount.Item2;
                Console.WriteLine(string.Format("ClinetCount<{0},{1}>", idleCount, activeCount));
            }
            while (idleCount != activeCount);
            Console.ReadLine();

        }
    }
}
