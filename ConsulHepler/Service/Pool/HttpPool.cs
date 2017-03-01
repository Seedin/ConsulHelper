using System;
using BitAuto.Ucar.Utils.Common.Service;
using BitAuto.Ucar.Utils.Common.Service.Client;

namespace BitAuto.Ucar.Utils.Common.Service.Pool
{
    public class HttpPool : SerPool
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hosts">服务器地址及端口</param>
        /// <param name="config">配置字典</param>
        public HttpPool(string[] hosts, SerConfig config)
            : base(hosts, config)
        {
        }

        /// <summary>
        /// 创建一个Thrift连接
        /// </summary>
        /// <returns></returns>
        protected override ISerClient CreateClient(string hostInfo)
        {
            return new HttpClient(hostInfo, this);
        }
    }
}
