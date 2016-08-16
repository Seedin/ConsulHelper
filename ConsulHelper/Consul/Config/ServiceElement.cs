using System;
using System.Configuration;

namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    /// <summary>
    /// 可发现服务元素
    /// </summary>
    public class ServiceElement : NameConfigurationElementBase
    {
        private const string enabledItem = "enabled";
        private const string servicetagsItem = "servicetags";

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
        /// 服务标签参数，可用于服务分级，可多项，逗号(,)分隔
        /// </summary>
        [ConfigurationProperty(servicetagsItem, DefaultValue = null)]
        public string ServiceTags
        {
            get { return (base[servicetagsItem] as string) ?? string.Empty; }
        }
    }
}
