using System;
using System.Collections.Generic;
using System.ServiceModel;
using BitAuto.Ucar.Utils.Common.Service;
using BitAuto.Ucar.Utils.Common.Service.Client;

namespace BitAuto.Ucar.Utils.Common.Service.Pool
{
    /// <summary>
    /// WCF连接池
    /// </summary>
    public class WcfPool : SerPool
    {
        /// <summary>
        /// WCF服务名称
        /// </summary>
        internal string ServiceName { set; get; }

        /// <summary>
        /// 通道类型
        /// </summary>
        internal Type ChanelType { set; get; }

        /// <summary>
        /// 协议
        /// </summary>
        internal string Protocol { set; get; }

        /// <summary>
        /// 服务路径
        /// </summary>
        internal string ServicePath { set; get; }

        /// <summary>
        /// 通道工厂
        /// </summary>
        protected Dictionary<string, ChannelFactory> factories = new Dictionary<string, ChannelFactory>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hosts">服务器地址及端口</param>
        /// <param name="config">配置字典</param>
        public WcfPool(string[] hosts, SerConfig config)
            : base(hosts, config)
        {
        }

        protected override void CreatePool(string[] hosts)
        {
            lock (locker)
            {
                //结点列表初始化
                Hosts = hosts;

                //版本增加
                Version++;

                //读取配置
                MaxActive = Config.GetIntValue("MaxActive", 100);
                MinIdle = Config.GetIntValue("MinIdle", 2);
                MaxIdle = Config.GetIntValue("MaxIdle", 10);
                ClientTimeout = Config.GetIntValue("ClientTimeout", 5000);
                ServiceName = Config["ServiceName"];
                ChanelType = Type.GetType(Config["ChanelType"]);
                Protocol = Config.GetStringValue("Protocol", "http://");
                ServicePath = Config.GetStringValue("ServicePath", "/" + ServiceName + ".svc");

                if (clientsPool == null)
                {
                    clientsPool = new Queue<ISerClient>();
                }
                else
                {
                    while (idleCount > 0)
                    {
                        DestoryClient(DequeueClient());
                    }

                    foreach (var factory in factories)
                    {
                        if (factory.Value.State != CommunicationState.Closed &&
                            factory.Value.State != CommunicationState.Closing)
                        {
                            try
                            {
                                factory.Value.Close();
                            }
                            catch { }
                        }
                    }
                    factories.Clear();

                    clientsPool = new Queue<ISerClient>();
                }
            }
        }

        /// <summary>
        /// 创建一个Wcf连接
        /// </summary>
        /// <returns>服务器地址信息</returns>
        protected override ISerClient CreateClient(string hostInfo)
        {
            return new WcfClient(GetChannelFactory(hostInfo), this);
        }

        /// <summary>
        /// 获取与服务器地址信息一致的通道工厂
        /// </summary>
        /// <param name="hostInfo">服务器地址信息</param>
        /// <returns>通道工厂</returns>
        protected ChannelFactory GetChannelFactory(string hostInfo)
        {
            if (factories.ContainsKey(hostInfo))
            {
                return factories[hostInfo];
            }
            else
            {
                var factory = typeof(ChannelFactory<>)
                    .MakeGenericType(ChanelType)
                    .GetConstructor(new Type[] { typeof(string) })
                    .Invoke(new object[] { ServiceName }) as ChannelFactory;
                factory.Endpoint.Address = new EndpointAddress(Protocol + hostInfo + ServicePath);
                factories.Add(hostInfo, factory);
                return factory;
            }
        }

    }
}
