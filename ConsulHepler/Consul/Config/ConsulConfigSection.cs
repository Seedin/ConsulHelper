
namespace BitAuto.Ucar.Utils.Common.Consul.Config
{
    public class ConsulConfigSection
    {
        /// <summary>
        /// 心跳周期参数（秒）
        /// </summary>
        public int HeartBreak { get; set; }

        /// <summary>
        /// 刷新周期参数（秒），刷新服务状态、键值事件
        /// </summary>
        public int RefreshBreak { get; set; }

        /// <summary>
        /// 服务名称参数（同服务多负载相同，不同服务不可相同）
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务标签参数，可用于服务分级，可多项，逗号(,)分隔
        /// </summary>
        public string ServiceTags { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int ServicePort { get; set; }

        /// <summary>
        /// 心跳检查HTTP接口，返回200状态即可，被动检查接口，不提供被动检查接口将按心跳周期主动向Consul提交心跳请求
        /// </summary>
        public string HttpCheck { set; get; }

        /// <summary>
        /// 心跳检查Tcp接口，连接成功建立即可，被动检查接口，不提供被动检查接口将按心跳周期主动向Consul提交心跳请求
        /// </summary>
        public string TcpCheck { set; get; }

        /// <summary>
        /// 可发现服务
        /// </summary>
        public ServiceElement[] Services { set; get; }

        /// <summary>
        /// 需监视分布式缓存键值
        /// </summary>
        public KeyValueElement[] KeyValues { set; get; }
    }
}
