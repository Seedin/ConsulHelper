using System;
using System.Reflection;
using System.ServiceModel;

namespace BitAuto.Ucar.Utils.Common.Service.Client
{
    /// <summary>
    /// WCF客户端
    /// </summary>
    public class WcfClient : ISerClient
    {
        /// <summary>
        /// 传输层
        /// </summary>
        ICommunicationObject transport;

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
        /// 构造函数
        /// </summary>
        /// <param name="host">连接主机信息</param>
        /// <param name="owner">连接所有者</param>
        /// <param name="config">连接配置</param>
        public WcfClient(ChannelFactory factory,
                            SerPool owner)
        {
            this.owner = owner;
            this.config = owner.Config;
            this.version = owner.Version;
            if (factory.State != CommunicationState.Opened)
            {
                factory.Open();
            }
            transport = factory.GetType().GetMethod("CreateChannel", new Type[] { })
                .Invoke(factory, null) as ICommunicationObject;
        }

        #region 属性
        /// <summary>
        /// 连接是否打开
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return transport.State == CommunicationState.Opened;
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
            if (transport.State != CommunicationState.Opened &&
                transport.State != CommunicationState.Opening)
            {
                transport.Open();
            }
        }

        /// <summary>
        /// 连接关闭
        /// </summary>
        /// <returns>是否关闭成功</returns>
        public void Close()
        {
            if (transport.State != CommunicationState.Closed &&
                transport.State != CommunicationState.Closing)
            {
                try
                {
                    transport.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// 重置，连接归还连接池前操作
        /// </summary>
        public void Reset() { }


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
            return (T)transport;
        }
        #endregion
    }
}
