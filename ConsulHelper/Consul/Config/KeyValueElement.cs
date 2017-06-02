
namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    /// <summary>
    /// 初始监控键值配置元素（组件本身支持动态监控，但第一次获取会有阻塞）
    /// </summary>
    public class KeyValueElement : NameConfigurationElementBase
    {
        /// <summary>
        /// 默认值
        /// </summary>
        public string Value { get; set; }
    }
}
