
namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    /// <summary>
    /// 可发现服务元素
    /// </summary>
    public class ServiceElement : NameConfigurationElementBase
    {
        /// <summary>
        /// 服务标签参数，可用于服务分级，可多项，逗号(,)分隔
        /// </summary>
        public string ServiceTags { get; set; }
    }
}
