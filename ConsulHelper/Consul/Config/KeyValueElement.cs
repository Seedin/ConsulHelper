using System;
using System.Configuration;


namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    /// <summary>
    /// 初始监控键值配置元素（组件本身支持动态监控，但第一次获取会有阻塞）
    /// </summary>
    public class KeyValueElement : NameConfigurationElementBase
    {
        private const string enabledItem = "enabled";
        private const string valueItem = "value";

        /// <summary>
        /// 是否使用服务
        /// </summary>
        [ConfigurationProperty(enabledItem, DefaultValue = true)]
        public bool Enabled
        {
            get
            {
                return (base[enabledItem] as bool?) ?? true;
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        [ConfigurationProperty(valueItem, DefaultValue = null)]
        public string Value
        {
            get { return (base[valueItem] as string) ?? string.Empty; }
        }
    }
}
