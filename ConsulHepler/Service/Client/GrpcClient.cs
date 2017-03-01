using System;
using System.Collections.Generic;
using Grpc.Core;

namespace BitAuto.Ucar.Utils.Common.Service.Client
{
    public class GrpcClient : ISerClient
    {
        /// <summary>
        /// 传输层
        /// </summary>
        Channel transport;

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
        public GrpcClient(string hostInfo,
                            SerPool owner)
        {
            this.owner = owner;
            this.config = owner.Config;
            this.version = owner.Version;
            var options = new List<ChannelOption> {
                new ChannelOption(ChannelOptions.MaxMessageLength,int.MaxValue)
            };
            transport = new Channel(hostInfo, ChannelCredentials.Insecure, options);
        }

        #region 属性
        /// <summary>
        /// 连接是否打开
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return transport.State == ChannelState.Ready;
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

        /// <summary>
        /// 版本
        /// </summary>
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
            if (transport.State != ChannelState.Ready)
            {
                transport.ConnectAsync().Wait(Owner.ClientTimeout / 2);
            }
            if (transport.State != ChannelState.Ready)
            {
                throw new RpcException(Status.DefaultCancelled, "连接失败");
            }
        }

        /// <summary>
        /// 连接关闭
        /// </summary>
        /// <returns>是否关闭成功</returns>
        public void Close()
        {
            transport.ShutdownAsync().Wait();
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
            return (T)Activator.CreateInstance(typeof(T), transport);
        }
        #endregion
    }
}
