using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Consul;

namespace BitAuto.Ucar.Utils.Common.Consul.Loader
{
    public class ConsulLoader
    {
        /// <summary>
        /// 服务注册
        /// </summary>
        /// <param name="name">服务名</param>
        /// <param name="tags">服务标签集，逗号分隔</param>
        /// <param name="port">服务端口</param>
        /// <param name="inerval">心跳间隔，被动心跳（被调用httpcheck）或主动心跳（TTL为间隔两倍）</param>
        /// <param name="httpcheck">被动心跳调用接口，可正常返回Http200即存活</param>
        /// <returns>是否注册成功</returns>
        public static async Task<bool> RegsterService(string name,
            string tags,
            int port,
            int inerval,
            string httpcheck,
            string tcpCheck)
        {
            using (var client = new ConsulClient())
            {
                var result = await client.Agent.ServiceRegister(
                    new AgentServiceRegistration()
                    {
                        ID = name,
                        Name = name,
                        Port = port,
                        Tags = tags.Split(','),
                        Check = new AgentServiceCheck()
                        {
                            HTTP = !string.IsNullOrEmpty(httpcheck) ? httpcheck : null,
                            TCP = !string.IsNullOrEmpty(tcpCheck) ? tcpCheck : null,
                            Interval = !string.IsNullOrEmpty(httpcheck) || !string.IsNullOrEmpty(tcpCheck) ? TimeSpan.FromSeconds(inerval) : (TimeSpan?)null,
                            TTL = string.IsNullOrEmpty(httpcheck) && string.IsNullOrEmpty(tcpCheck) ? TimeSpan.FromSeconds(2 * inerval) : (TimeSpan?)null,
                            Status = HealthStatus.Passing
                        }
                    }).ConfigureAwait(false);
                return result.StatusCode == HttpStatusCode.OK;
            }
        }

        /// <summary>
        /// 心跳通知（主动心跳模式使用，即不设定httpcheck）
        /// </summary>
        /// <param name="checkId">服务检查ID</param>
        /// <returns>通知成功</returns>
        public static async Task<bool> HeartBreak(string checkId)
        {
            using (var client = new ConsulClient())
            {
                var result = await client.Agent.UpdateTTL(checkId, null, TTLStatus.Pass).ConfigureAwait(false);
                return result.StatusCode == HttpStatusCode.OK;
            }
        }

        /// <summary>
        /// 可用服务发现
        /// </summary>
        /// <param name="name">服务名称</param>
        /// <param name="tags">服务标签</param>
        /// <returns>可用服务信息</returns>
        public static async Task<ServiceEntry[]> AvaliableServices(string name, string tags)
        {
            var services = new List<ServiceEntry>();
            using (var client = new ConsulClient())
            {
                foreach (var tag in tags.Split(','))
                {
                    var result = await client.Health.Service(name, !string.IsNullOrEmpty(tag) ? tag : null, true).ConfigureAwait(false);
                    foreach (var item in result.Response)
                    {
                        if (!services.Any(service => service.Node.Address == item.Node.Address
                            && service.Service.Port == item.Service.Port))
                        {
                            services.Add(item);
                        }
                    }
                    //services.AddRange(result.Response);
                }
                //交集处理，仅取出完全匹配服务
                foreach (var tag in tags.Split(','))
                {
                    if (string.IsNullOrEmpty(tag))
                    {
                        continue;
                    }
                    var alsorans = services.Where(service => !service.Service.Tags.Contains(tag)).ToList();
                    foreach (var alsoran in alsorans)
                    {
                        services.Remove(alsoran);
                    }
                }
            }
            return services.ToArray();
        }

        /// <summary>
        /// 设置分布式缓存键值对
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>是否设置成功</returns>
        public static async Task<bool> SetKeyValue(string key, string value)
        {
            using (var client = new ConsulClient())
            {
                var result = await client.KV.Put(
                    new KVPair(key) { Value = UTF8Encoding.UTF8.GetBytes(value) }).ConfigureAwait(false);
                return result.Response;
            }
        }

        /// <summary>
        /// 获取分布式缓存键值对
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public static async Task<string> GetKeyValue(string key)
        {
            using (var client = new ConsulClient())
            {
                var result = await client.KV.Get(key).ConfigureAwait(false);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return UTF8Encoding.UTF8.GetString(result.Response.Value ?? new byte[0]);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
