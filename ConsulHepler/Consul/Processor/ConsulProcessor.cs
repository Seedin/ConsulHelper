using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Configuration;
using BitAuto.Ucar.Utils.Common.Consul.Config;
using BitAuto.Ucar.Utils.Common.Consul.Container;
using BitAuto.Ucar.Utils.Common.Consul.Loader;

namespace BitAuto.Ucar.Utils.Common.Consul.Processor
{
    /// <summary>
    /// Consul处理器
    /// </summary>
    public class ConsulProcessor
    {
        /// <summary>
        /// 服务名
        /// </summary>
        private string serviceName;

        /// <summary>
        /// 刷新间隔
        /// </summary>
        private int refreshBreak;

        /// <summary>
        /// 心跳间隔
        /// </summary>
        private int heartBreak;

        /// <summary>
        /// 依赖服务标签定义键
        /// </summary>
        private const string ServiceTagsKeyFormat = "F:ServcieTags:{0}:{1}:{2}";

        /// <summary>
        /// 服务配置定义键
        /// </summary>
        private const string ServiceConfigKeyFormat = "F:Config:{0}:{1}";

        /// <summary>
        /// 同步线程
        /// </summary>
        private void SyncProcess()
        {
            while (true)
            {
                //同步间隔
                Thread.Sleep(TimeSpan.FromSeconds(refreshBreak));

                //同步键值
                SyncKeyValues();

                //同步服务信息
                SyncServices();
            }
        }

        /// <summary>
        /// 心跳线程
        /// </summary>
        private void HeartProcess()
        {
            while (true)
            {
                //同步间隔
                Thread.Sleep(TimeSpan.FromSeconds(heartBreak));

                //同步
                HeartBreakService();
            }
        }

        /// <summary>
        /// 同步可用服务信息
        /// </summary>
        /// <param name="serviceNames">服务名集合</param>
        /// <returns>同步更新可用服务负载点数量</returns>
        private int SyncServices()
        {
            var count = 0;
            try
            {
                foreach (var serviceName in ConsulCache.Instance.GetServiceNames())
                {
                    var services = ConsulLoader.AvaliableServices(serviceName,
                        ConsulCache.Instance.GetServiceTags(serviceName))
                        .GetAwaiter().GetResult();
                    count += ConsulCache.Instance.SetServiceInfo(serviceName, services);
                }
            }
            catch { }
            return count;
        }

        /// <summary>
        /// 同步分布式缓存键值对
        /// </summary>
        /// <returns></returns>
        private int SyncKeyValues()
        {
            var kvs = new List<Tuple<string, string>>();
            //刷新键值
            try
            {
                foreach (var key in ConsulCache.Instance.GetKeys())
                {
                    var value = ConsulLoader.GetKeyValue(key)
                        .GetAwaiter().GetResult();
                    if (value != null)
                    {
                        kvs.Add(new Tuple<string, string>(key, value));
                    }
                }
            }
            catch { }

            int count = ConsulCache.Instance.SetKeyValue(kvs.ToArray());

            //刷新Tags
            foreach (var sName in ConsulCache.Instance.GetServiceNames())
            {
                //var serviceTagKey = string.Format(ServiceTagsKeyFormat, serviceName, sName, Dns.GetHostName());
                var serviceTagKey = GetServiceTagsKey(sName);
                ConsulCache.Instance.SetServiceTag(sName, ConsulCache.Instance.GetKeyValue(serviceTagKey));
            }

            return count;
        }

        /// <summary>
        /// 服务心跳发送
        /// </summary>
        /// <returns>是否发送成功</returns>
        private bool HeartBreakService()
        {
            try
            {
                return ConsulLoader.HeartBreak("service:" + serviceName).GetAwaiter().GetResult();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            string basePath = null;
#if !CORECLR
            basePath = System.Web.HttpRuntime.AppDomainAppPath;
#else
            basePath = Directory.GetCurrentDirectory();
#endif

            //读取配置
            var consulConfig = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("Config/consulplugin.json", true, false)
                .Build().Get<ConsulConfigSection>();
            serviceName = consulConfig.ServiceName;
            refreshBreak = consulConfig.RefreshBreak;
            heartBreak = consulConfig.HeartBreak;

            //注册服务
            try
            {
                ConsulLoader.RegsterService(serviceName,
                    consulConfig.ServiceTags,
                    consulConfig.ServicePort,
                    consulConfig.HeartBreak,
                    consulConfig.HttpCheck,
                    consulConfig.TcpCheck).GetAwaiter().GetResult();
            }
            catch { throw; }

            //若不存在HttpCheck且Interval存在时发送初始心跳
            if (string.IsNullOrEmpty(consulConfig.HttpCheck) &&
                string.IsNullOrEmpty(consulConfig.TcpCheck) &&
                heartBreak > 0)
            {
                HeartBreakService();
            }

            //读取缓存键值对配置信息
            if (consulConfig.KeyValues != null)
            {
                foreach (KeyValueElement kvConfig in consulConfig.KeyValues)
                {
                    ConsulCache.Instance.SetKeyValue(new Tuple<string, string>(kvConfig.Name, kvConfig.Value));
                }
            }


            //读取依赖服务配置信息
            if (consulConfig.Services != null)
            {
                foreach (ServiceElement serviceConfig in consulConfig.Services)
                {
                    ConsulCache.Instance.SetServiceTag(serviceConfig.Name, serviceConfig.ServiceTags);
                    var serviceTagKey = GetServiceTagsKey(serviceConfig.Name);
                    try
                    {
                        if (string.IsNullOrEmpty(ConsulLoader.GetKeyValue(serviceTagKey).GetAwaiter().GetResult()))
                        {
                            ConsulLoader.SetKeyValue(serviceTagKey, serviceConfig.ServiceTags).GetAwaiter().GetResult();
                        }
                    }
                    catch { }
                    ConsulCache.Instance.SetKeyValue(new Tuple<string, string>(serviceTagKey, serviceConfig.ServiceTags));
                }
            }

            //初次同步依赖服务信息及缓存信息
            SyncKeyValues();
            SyncServices();

            //若不存在HttpCheck且Interval存在时开启心跳线程
            if (string.IsNullOrEmpty(consulConfig.HttpCheck) &&
                string.IsNullOrEmpty(consulConfig.TcpCheck) &&
                heartBreak > 0)
            {
                new Thread(HeartProcess) { IsBackground = true }.Start();
            }

            //同步线程开启
            new Thread(SyncProcess) { IsBackground = true }.Start();
        }

        /// <summary>
        /// 获取服务标签键值
        /// </summary>
        public string GetServiceTagsKey(string rellyServiceName)
        {
            return string.Format(ServiceTagsKeyFormat, serviceName, rellyServiceName, Dns.GetHostName());
        }

        /// <summary>
        /// 获取服务配置键值
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public string GetServiceConfigKey(string configName)
        {
            return string.Format(ServiceConfigKeyFormat, serviceName, configName);
        }

        /// <summary>
        /// 获取消费者服务名
        /// </summary>
        /// <returns>消费者服务名</returns>
        public string GetServiceName()
        {
            return serviceName;
        }
    }
}
