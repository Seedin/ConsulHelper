using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitAuto.Ucar.Utils.Common.Consul.Processor;
using BitAuto.Ucar.Utils.Common.Consul.Container;
using BitAuto.Ucar.Utils.Common.Consul.Loader;
using BitAuto.Ucar.Utils.Common.Service;
using BitAuto.Ucar.Utils.Common.Service.Pool;

namespace BitAuto.Ucar.Utils.Common
{
    public class ConsulHelper
    {
        /// <summary>
        /// Consul处理器
        /// </summary>
        private ConsulProcessor processor = null;

        /// <summary>
        /// 服务池
        /// </summary>
        private Dictionary<string, SerPool> serverPools = new Dictionary<string, SerPool>();

        /// <summary>
        /// 服务锁
        /// </summary>
        private object serviceLock = new object();

        /// <summary>
        /// 单例构造函数
        /// </summary>
        private ConsulHelper()
        {
            processor = new ConsulProcessor();
            processor.Initialize();
        }

        /// <summary>
        /// 单例实例
        /// </summary>
        private static ConsulHelper instance = null;

        /// <summary>
        /// 单例实例 
        /// </summary>
        public static ConsulHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(ConsulHelper))
                    {
                        if (instance == null)
                        {
                            instance = new ConsulHelper();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 获取服务连接信息，格式为{ip}:{port}
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="serviceTags">服务内部标签，可用于服务分级</param>
        /// <returns>可用Host字符串数组</returns>
        public string[] GetServiceHosts(string serviceName, string serviceTags)
        {
            var hosts = ConsulCache.Instance.GetServiceHosts(serviceName, serviceTags ?? string.Empty);
            try
            {
                if (hosts.Length == 0 && !ConsulCache.Instance.GetServiceNames().Contains(serviceName))
                {
                    ConsulCache.Instance.SetServiceInfo(serviceName,
                        ConsulLoader.AvaliableServices(
                            serviceName,
                            serviceTags ?? string.Empty)
                        .GetAwaiter()
                        .GetResult());
                    hosts = ConsulCache.Instance.GetServiceHosts(serviceName, serviceTags ?? string.Empty);
                }
            }
            catch { }
            return hosts;
        }

        /// <summary>
        /// 从Consul直接获取服务连接信息，不缓存，格式为{ip}:{port}
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="serviceTags">服务内部标签，可用于服务分级</param>
        /// <returns>可用Host字符串数组</returns>
        public string[] GetServiceHostsNoCache(string serviceName, string serviceTags)
        {
            return ConsulLoader.AvaliableServices(
                                serviceName,
                                serviceTags ?? ConsulCache.Instance.GetServiceTags(serviceName))
                            .GetAwaiter()
                            .GetResult()
                            .Select(service => service.Node.Address + ":" + service.Service.Port)
                            .ToArray();

        }

        /// <summary>
        /// 根据键获取值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public string GetKeyValue(string key)
        {
            var value = ConsulCache.Instance.GetKeyValue(key);

            try
            {
                if (value == null && !ConsulCache.Instance.GetKeys().Contains(key))
                {
                    value = ConsulLoader.GetKeyValue(key).GetAwaiter().GetResult();
                    ConsulCache.Instance.SetKeyValue(new Tuple<string, string>(key, value));
                }
            }
            catch { }

            return value;
        }

        /// <summary>
        /// 根据键从Consul直接获取获取值，不缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public string GetKeyValueNoCache(string key)
        {
            var value = ConsulLoader.GetKeyValue(key).GetAwaiter().GetResult();

            return value;
        }

        /// <summary>
        /// 保存分布式键值对
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public bool SetKeyValue(string key, string value)
        {
            try
            {
                ConsulLoader.SetKeyValue(key, value).GetAwaiter().GetResult();
            }
            catch { }

            return ConsulCache.Instance.SetKeyValue(new Tuple<string, string>(key, value)) > 0;
        }

        /// <summary>
        /// 向Consul直接保存分布式键值对，不缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public bool SetKeyValueNoCache(string key, string value)
        {
            return ConsulLoader.SetKeyValue(key, value).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 添加服务钩子
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="callBack">服务变更回调，回调参数为服务名</param>
        public void AddServiceHook(string serviceName, Action<string> callBack)
        {
            ConsulCache.Instance.AddServiceHook(serviceName, callBack);
        }

        /// <summary>
        /// 移除服务钩子
        /// </summary>
        /// <param name="serviceName">服务名</param>
        public void RemoveServiceHook(string serviceName)
        {
            ConsulCache.Instance.RemoveServiceHook(serviceName);
        }

        /// <summary>
        /// 添加键值对钩子
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="callBack">键值对变更回调，回调参数按序为键、值</param>
        public void AddKvHook(string key, Action<string, string> callBack)
        {
            ConsulCache.Instance.AddKvHook(key, callBack);
        }

        /// <summary>
        /// 移除键值对钩子
        /// </summary>
        /// <param name="key">键</param>
        public void RemoveKvHook(string key)
        {
            ConsulCache.Instance.RemoveKvHook(key);
        }

        /// <summary>
        /// 获取服务连接
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="protocolTags">协议标签，决定连接池类型，默认使用配置内指定标签</param>
        /// <returns>服务连接</returns>
        public ISerClient GetServiceClient(string serviceName, string protocolTags = null)
        {
            //参数检查
            ISerClient client = null;
            protocolTags = protocolTags ?? ConsulCache.Instance.GetServiceTags(serviceName);
            if (string.IsNullOrEmpty(serviceName) ||
                string.IsNullOrEmpty(protocolTags))
            {
                return client;
            }

            //筛选有效协议标签
            string protocolTag = null;
            foreach (var tag in protocolTags.Split(','))
            {
                switch (tag)
                {
                    case "http":
                    case "thrift":
                    case "wcf":
                    case "grpc":
                        protocolTag = tag;
                        break;
                    default:
                        continue;
                }
            }
            if (protocolTag == null)
            {
                return client;
            }

            //获取或创建连接池
            var poolKey = string.Join(":", serviceName, protocolTag);
            SerPool pool = null;
            if (serverPools.ContainsKey(poolKey))
            {
                pool = serverPools[poolKey];
            }
            else
            {
                lock (serviceLock)
                {
                    if (serverPools.ContainsKey(poolKey))
                    {
                        pool = serverPools[poolKey];
                    }
                    else
                    {
                        //读取连接池配置值
                        var config = new SerConfig();
                        foreach (var key in ConsulCache.Instance.GetKeys())
                        {
                            if (key.Contains(poolKey))
                            {
                                var configKey = key.Split(':').Last();
                                config.Add(configKey, GetKeyValue(key));

                                //设置键值回调
                                AddKvHook(key, (k, v) =>
                                {
                                    config[k.Split(':').Last()] = v;
                                });

                            }
                        }
                        //配置加入服务名
                        config.Add("ServiceName", serviceName);

                        //创建连接池
                        switch (protocolTag)
                        {
                            case "http":
                                pool = new HttpPool(GetServiceHosts(serviceName, protocolTags),
                                                    config);
                                break;
                            case "thrift":
                                pool = new ThriftPool(GetServiceHosts(serviceName, protocolTags),
                                                    config);
                                break;
                            case "wcf":
                                pool = new WcfPool(GetServiceHosts(serviceName, protocolTags),
                                                    config);
                                break;
                            case "grpc":
                                pool = new GrpcPool(GetServiceHosts(serviceName, protocolTags),
                                                    config);
                                break;
                            default:
                                return client;
                        }

                        //设置连接池重置回调，负载信息变更
                        AddServiceHook(serviceName, (s) =>
                        {
                            pool.ResetPool(GetServiceHosts(s, ConsulCache.Instance.GetServiceTags(s)));
                        });

                        //添加连接池
                        serverPools.Add(poolKey, pool);
                    }
                }
            }

            //返回连接
            return pool.BorrowClient();
        }

        /// <summary>
        /// 获取服务连接池计数
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="protocolTag">准确协议标签</param>
        /// <returns>连接池计数，依次为空闲连接数与有效连接数</returns>
        public Tuple<int, int> GetServiceClinetCount(string serviceName, string protocolTag)
        {
            var poolKey = string.Join(":", serviceName, protocolTag);
            if (!serverPools.ContainsKey(poolKey))
            {
                return new Tuple<int, int>(0, 0);
            }
            else
            {
                var pool = serverPools[poolKey];
                return new Tuple<int, int>(pool.IdleCount, pool.ActiveCount);
            }
        }

        /// <summary>
        /// 获取当前服务依赖服务标签配置键
        /// </summary>
        /// <param name="serviceName">依赖服务名</param>
        /// <returns>标签配置键</returns>
        public string GetServiceTagsKey(string serviceName)
        {
            return processor.GetServiceTagsKey(serviceName);
        }

        /// <summary>
        /// 获取服务配置键（标准前缀格式）
        /// </summary>
        /// <param name="configName">配置名，不含前缀</param>
        /// <returns>服务配置键</returns>
        public string GetServiceConfigKey(string configName)
        {
            return processor.GetServiceConfigKey(configName);
        }

        /// <summary>
        /// 获取当前应用配置服务名
        /// </summary>
        /// <returns>当前应用配置服务名</returns>
        public string GetServiceName()
        {
            return processor.GetServiceName();
        }
    }
}
