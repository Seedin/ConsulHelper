using System;
using System.Reflection;
using System.Net.Http.Headers;
using BitAuto.Ucar.Utils.Common.Service;

namespace BitAuto.Ucar.Utils.Common.Service.Client
{
    public class HttpClient : ISerClient
    {
        /// <summary>
        /// 传输层
        /// </summary>
        System.Net.Http.HttpClient transport;

        /// <summary>
        /// 连接所有者
        /// </summary>
        SerPool owner;

        /// <summary>
        /// 连接配置
        /// </summary>
        SerConfig config;

        /// <summary>
        /// 连接版本
        /// </summary>
        int version;

        /// <summary>
        /// 释放标志
        /// </summary>
        protected bool disposed;

        /// <summary>
        /// 开启标志
        /// </summary>
        protected bool[] isOpen = new bool[] { false };

        /// <summary>
        /// url基址，包含IP及端口
        /// </summary>
        private string baseUrl;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">连接主机信息</param>
        /// <param name="owner">连接所有者</param>
        /// <param name="config">连接配置</param>
        public HttpClient(string hostInfo,
                            SerPool owner)
        {
            this.owner = owner;
            this.config = owner.Config;
            this.version = owner.Version;
            this.baseUrl = this.config.GetStringValue("Protocol", "http://")
                + hostInfo;
            transport = new System.Net.Http.HttpClient()
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        #region 属性
        /// <summary>
        /// 连接是否打开
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return isOpen[0];
            }
        }

        /// <summary>
        /// 连接拥有者
        /// </summary>
        public SerPool Owner
        {
            get
            {
                return owner;
            }
        }

        public int Version
        {
            get
            {
                return version;
            }
        }
        #endregion

        #region 私有方法
        protected void Dispose(bool disposing)
        {
            this.Owner.ReturnClient(this);
            disposed = true;
        }
        #endregion

        #region 公开方法
        /// <summary>
        /// 连接打开
        /// </summary>
        /// <returns>是否开启成功</returns>
        public void Open()
        {
            var task = transport.SendAsync(new System.Net.Http.HttpRequestMessage
            {
                Method = new System.Net.Http.HttpMethod("HEAD"),
                RequestUri = new Uri(this.baseUrl + "/")
            });
            if (task.Wait(Owner.ClientTimeout / 2))
            {
                task.Result.EnsureSuccessStatusCode();
                isOpen[0] = true;
            }
            else
            {
                isOpen[0] = false;
                throw new TimeoutException("连接时间超过" + Owner.ClientTimeout + "毫秒");
            }
        }

        /// <summary>
        /// 连接关闭
        /// </summary>
        /// <returns>是否关闭成功</returns>
        public void Close()
        {
            transport.Dispose();
        }

        /// <summary>
        /// 重置，连接归还连接池前操作
        /// </summary>
        public void Reset()
        {
            transport.DefaultRequestHeaders.Clear();
            transport.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.93 Safari/537.36");
            transport.DefaultRequestHeaders.Connection.TryParseAdd("Keep-Alive");
            var auth = this.config.GetStringValue("Auth", "").Split(' ');
            if (auth.Length == 2)
            {
                transport.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(auth[0], auth[1]);
            }
        }

        /// <summary>
        /// 释放连接池
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 获取桩子类型
        /// </summary>
        /// <typeparam name="T">桩子类型</typeparam>
        /// <returns>RPC桩子</returns>
        public T GetStub<T>()
        {
            return (T)Activator.CreateInstance(typeof(T), transport, isOpen);
        }
        #endregion
    }
}
