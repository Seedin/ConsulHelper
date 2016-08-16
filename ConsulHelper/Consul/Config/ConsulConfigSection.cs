using System.Configuration;
using System.Diagnostics;

namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    public class ConsulConfigSection : ConfigurationSection
    {
        private const string servicesItem = "services";
        private const string keyvaluesItem = "keyvalues";
        private const string enabledItem = "enabled";
        private const string servicenameItem = "servicename";
        private const string servicetagsItem = "servicetags";
        private const string serviceportItem = "serviceport";
        private const string httpcheckItem = "httpcheck";
        private const string tcpcheckItem = "tcpcheck";
        private const string heartbreakItem = "heartbreak";
        private const string refreshbreakItem = "refreshbreak";

        /// <summary>
        /// 是有有效参数
        /// </summary>
        [ConfigurationProperty(enabledItem, DefaultValue = true)]
        public bool Enabled
        {
            get { return (base[enabledItem] as bool?) ?? true; }
        }

        /// <summary>
        /// 心跳周期参数（秒）
        /// </summary>
        [ConfigurationProperty(heartbreakItem, DefaultValue = 15)]
        public int HeartBreak
        {
            get { return (base[heartbreakItem] as int?) ?? 15; }
        }

        /// <summary>
        /// 刷新周期参数（秒），刷新服务状态、键值事件
        /// </summary>
        [ConfigurationProperty(refreshbreakItem, DefaultValue = 60)]
        public int RefreshBreak
        {
            get { return (base[refreshbreakItem] as int?) ?? 60; }
        }

        /// <summary>
        /// 服务名称参数（同服务多负载相同，不同服务不可相同）
        /// </summary>
        [ConfigurationProperty(servicenameItem, DefaultValue = null)]
        public string ServiceName
        {
            get { return (base[servicenameItem] as string) ?? string.Empty; }
        }

        /// <summary>
        /// 服务标签参数，可用于服务分级，可多项，逗号(,)分隔
        /// </summary>
        [ConfigurationProperty(servicetagsItem, DefaultValue = null)]
        public string ServiceTags
        {
            get { return (base[servicetagsItem] as string) ?? string.Empty; }
        }

        /// <summary>
        /// 服务端口
        /// </summary>
        [ConfigurationProperty(serviceportItem, DefaultValue = 80)]
        public int ServicePort
        {
            get { return (base[serviceportItem] as int?) ?? 80; }
        }

        /// <summary>
        /// 心跳检查HTTP接口，返回200状态即可，被动检查接口，不提供被动检查接口将按心跳周期主动向Consul提交心跳请求
        /// </summary>
        [ConfigurationProperty(httpcheckItem, DefaultValue = null)]
        public string HttpCheck
        {
            get { return (base[httpcheckItem] as string) ?? string.Empty; }
        }

        /// <summary>
        /// 心跳检查Tcp接口，连接成功建立即可，被动检查接口，不提供被动检查接口将按心跳周期主动向Consul提交心跳请求
        /// </summary>
        [ConfigurationProperty(tcpcheckItem, DefaultValue = null)]
        public string TcpCheck
        {
            get { return (base[tcpcheckItem] as string) ?? string.Empty; }
        }

        /// <summary>
        /// 可发现服务
        /// </summary>
        [ConfigurationProperty(servicesItem)]
        public ServiceElementCollection Services
        {
            get
            {
                return base[servicesItem] as ServiceElementCollection;
            }
        }

        /// <summary>
        /// 需监视分布式缓存键值
        /// </summary>
        [ConfigurationProperty(keyvaluesItem)]
        public KeyValueElementCollection KeyValues
        {
            get
            {
                return base[keyvaluesItem] as KeyValueElementCollection;
            }
        }
    }
}
