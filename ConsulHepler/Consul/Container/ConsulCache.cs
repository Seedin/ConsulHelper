using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Consul;

namespace BitAuto.Ucar.Utils.Common.Consul.Container
{
    public class ConsulCache
    {
        /// <summary>
        /// 单例构造函数
        /// </summary>
        private ConsulCache() { }

        /// <summary>
        /// 单例实例
        /// </summary>
        private static ConsulCache instance = null;

        /// <summary>
        /// 单例实例 
        /// </summary>
        public static ConsulCache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(ConsulCache))
                    {
                        if (instance == null)
                        {
                            instance = new ConsulCache();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 服务信息哈希值计分
        /// </summary>
        private Func<ServiceEntry, int> HashScore = (service) =>
        {
            return service.Node.Address.GetHashCode() % 49999
                    + service.Service.Port % 49999
                    + service.Service.Tags.Sum(tag => tag.GetHashCode() % 49999);
        };

        /// <summary>
        /// 服务标签字典
        /// </summary>
        private Dictionary<string, string> serviceTags = new Dictionary<string, string>();

        /// <summary>
        /// 回调监视标签快照
        /// </summary>
        private Dictionary<string, List<string>> hookedServiceTags = new Dictionary<string, List<string>>();

        /// <summary>
        /// 服务信息
        /// </summary>
        private Dictionary<string, ServiceEntry[]> serviceInfos = new Dictionary<string, ServiceEntry[]>();

        /// <summary>
        /// 服务钩子
        /// </summary>
        private Dictionary<string, List<Action<string>>> serviceHooks = new Dictionary<string, List<Action<string>>>();

        /// <summary>
        /// 缓存键值对
        /// </summary>
        private Dictionary<string, string> keyValues = new Dictionary<string, string>();

        /// <summary>
        /// 键值对钩子
        /// </summary>
        private Dictionary<string, List<Action<string, string>>> kvHooks = new Dictionary<string, List<Action<string, string>>>();

        /// <summary>
        /// 字典更新锁对象，因为并发写几率并不高，不采用并发字典直接使用并发锁对象
        /// </summary>
        private object updatelock = new object();

        /// <summary>
        /// 依赖服务标签设置
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="tags">服务标签，逗号分隔</param>
        public void SetServiceTag(string serviceName, string tags)
        {
            lock (updatelock)
            {
                if (serviceTags.ContainsKey(serviceName))
                {
                    //var tagSet = new SortedSet<string>(serviceTags[serviceName].Split(','));
                    //foreach (var tag in tags.Split(','))
                    //{
                    //    tagSet.Add(tag);
                    //}
                    //serviceTags[serviceName] = string.Join(",", tagSet);
                    serviceTags[serviceName] = tags ?? string.Empty;
                }
                else
                {
                    serviceTags.Add(serviceName, tags);
                }
            }
        }

        /// <summary>
        /// 获取依赖服务标签，未指定时返回空串标签标示不限制
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <returns>服务标签集</returns>
        public string GetServiceTags(string serviceName)
        {
            return serviceTags.ContainsKey(serviceName) ? serviceTags[serviceName] : string.Empty;
        }

        /// <summary>
        /// 设置服务信息，无则添加，有则更新
        /// </summary>
        /// <param name="name">服务名，无法自动识别服务名时再参考</param>
        /// <param name="services">服务信息</param>
        /// <returns>成功设置数量</returns>
        public int SetServiceInfo(string name, params ServiceEntry[] services)
        {
            var setCount = 0;
            var serviceNames = services
                .Select(service => service.Service.Service)
                .Distinct()
                .ToList();
            if (services.Length == 0
                && !string.IsNullOrEmpty(name))
            {
                serviceNames.Add(name);
            }
            var hookServiceNames = new List<string>();
            lock (updatelock)
            {
                foreach (var serviceName in serviceNames)
                {
                    var typeServices = services
                        .Where(service => service.Service.Service == serviceName)
                        .ToArray();
                    var serviceTags = GetServiceTags(serviceName).Split(',');
                    if (serviceInfos.ContainsKey(serviceName))
                    {
                        var lastServices = serviceInfos[serviceName];
                        //值更新时确认是否发生变动，变动时更新缓存服务
                        if (lastServices.Length != typeServices.Length ||
                            lastServices.Sum(HashScore) !=
                            typeServices.Sum(HashScore)
                            )
                        {
                            serviceInfos[serviceName] = typeServices;
                            //检查是否需要触发回调，选定标签的服务集合发生变动时触发回调

                            //变更前可用服务清单
                            var lastTagSers = GetTagsFilteredServices(lastServices, hookedServiceTags[serviceName]);
                            //变更后可用服务清单
                            var nextTagSers = GetTagsFilteredServices(typeServices, serviceTags);
                            //指定标签服务发生变动，添加回调标记
                            if (lastTagSers.Count != nextTagSers.Count ||
                                lastTagSers.Sum(HashScore) !=
                            nextTagSers.Sum(HashScore))
                            {
                                hookServiceNames.Add(serviceName);
                            }
                            //更新标签快照
                            hookedServiceTags[serviceName].Clear();
                            hookedServiceTags[serviceName].AddRange(serviceTags);
                        }
                    }
                    else
                    {
                        serviceInfos.Add(serviceName, typeServices);
                        hookServiceNames.Add(serviceName);
                        hookedServiceTags.Add(serviceName, new List<string>());
                        hookedServiceTags[serviceName].AddRange(serviceTags);
                    }
                    setCount += typeServices.Length;
                }
            }

            //锁外异步触发回调，避免不确定性
            if (hookServiceNames.Count > 0)
            {
                new Thread(() =>
                {
                    foreach (var serviceName in hookServiceNames)
                    {
                        if (!serviceHooks.ContainsKey(serviceName))
                        {
                            continue;
                        }
                        foreach (var callback in serviceHooks[serviceName])
                        {
                            callback(serviceName);
                        }
                    }
                }) { IsBackground = true }.Start();

            }

            return setCount;
        }

        /// <summary>
        /// 获取服务连接信息，格式为{ip}:{port}
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="serviceTags">服务内部标签，可用于服务分级</param>
        /// <returns>可用Host字符串数组</returns>
        public string[] GetServiceHosts(string serviceName, string serviceTags)
        {
            if (!serviceInfos.ContainsKey(serviceName))
            {
                return new string[0];
            }
            var services = GetTagsFilteredServices(serviceInfos[serviceName], serviceTags.Split(','));
            return services.Select(service => service.Node.Address + ":" + service.Service.Port).ToArray();
        }

        /// <summary>
        /// 获取全部服务名
        /// </summary>
        /// <returns></returns>
        public string[] GetServiceNames()
        {
            return serviceTags.Keys.ToArray();
        }

        /// <summary>
        /// 设置键值对
        /// </summary>
        /// <param name="kvs">键值对</param>
        /// <returns>成功设置数量</returns>
        public int SetKeyValue(params Tuple<string, string>[] kvs)
        {
            var setCount = 0;
            var hookKeys = new List<string>();
            lock (updatelock)
            {
                foreach (var kv in kvs)
                {
                    if (keyValues.ContainsKey(kv.Item1))
                    {
                        var lastValue = keyValues[kv.Item1];
                        //值更新时确认是否发生变动，变动时触发钩子
                        if (lastValue != kv.Item2)
                        {
                            keyValues[kv.Item1] = kv.Item2;
                            hookKeys.Add(kv.Item1);
                        }
                    }
                    else
                    {
                        keyValues.Add(kv.Item1, kv.Item2);
                    }
                    setCount++;
                }
            }

            //锁外异步触发回调，避免不确定性
            if (hookKeys.Count > 0)
            {
                new Thread(() =>
                {
                    foreach (var key in hookKeys)
                    {
                        if (!kvHooks.ContainsKey(key))
                        {
                            continue;
                        }
                        foreach (var callback in kvHooks[key])
                        {
                            callback(key, keyValues[key]);
                        }
                    }
                }) { IsBackground = true }.Start();
            }

            return setCount;
        }

        /// <summary>
        /// 根据键获取值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public string GetKeyValue(string key)
        {
            if (!keyValues.ContainsKey(key))
            {
                return null;
            }
            else
            {
                return keyValues[key];
            }
        }

        /// <summary>
        /// 获取全部缓存键
        /// </summary>
        /// <returns></returns>
        public string[] GetKeys()
        {
            return keyValues.Keys.ToArray();
        }

        /// <summary>
        /// 添加服务钩子
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="callBack">服务变更回调，回调参数为服务名</param>
        public void AddServiceHook(string serviceName, Action<string> callBack)
        {
            lock (updatelock)
            {
                List<Action<string>> serviceHook = null;
                if (serviceHooks.ContainsKey(serviceName))
                {
                    serviceHook = serviceHooks[serviceName];
                }
                else
                {
                    serviceHook = new List<Action<string>>();
                    serviceHooks.Add(serviceName, serviceHook);
                }
                if (serviceHook != null)
                {
                    serviceHook.Add(callBack);
                }
            }
        }

        /// <summary>
        /// 移除服务钩子
        /// </summary>
        /// <param name="serviceName"></param>
        public void RemoveServiceHook(string serviceName)
        {
            lock (updatelock)
            {
                if (serviceHooks.ContainsKey(serviceName))
                {
                    serviceHooks[serviceName].Clear();
                }
            }
        }

        /// <summary>
        /// 添加键值对钩子
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="callBack">键值对变更回调，回调参数按序为键、值</param>
        public void AddKvHook(string key, Action<string, string> callBack)
        {
            lock (updatelock)
            {
                List<Action<string, string>> kvHook = null;
                if (kvHooks.ContainsKey(key))
                {
                    kvHook = kvHooks[key];
                }
                else
                {
                    kvHook = new List<Action<string, string>>();
                    kvHooks.Add(key, kvHook);
                }
                kvHook.Add(callBack);
            }

        }

        /// <summary>
        /// 移除键值对钩子
        /// </summary>
        /// <param name="key">键</param>
        public void RemoveKvHook(string key)
        {
            lock (updatelock)
            {
                if (kvHooks.ContainsKey(key))
                {
                    kvHooks[key].Clear();
                }
            }
        }

        /// <summary>
        /// 获取被标签过滤后的服务
        /// </summary>
        /// <param name="services">原始服务清单</param>
        /// <param name="tags">标签</param>
        /// <returns>标签过滤服务</returns>
        public List<ServiceEntry> GetTagsFilteredServices(
            IEnumerable<ServiceEntry> services,
            IEnumerable<string> tags)
        {
            var tagServices = new List<ServiceEntry>(services);
            foreach (var tag in tags)
            {
                //仅匹配交集
                if (string.IsNullOrEmpty(tag))
                {
                    continue;
                }
                var alsorans = tagServices.Where(service => !service.Service.Tags.Contains(tag)).ToList();
                foreach (var alsoran in alsorans)
                {
                    tagServices.Remove(alsoran);
                }
            }
            return tagServices;
        }
    }
}
