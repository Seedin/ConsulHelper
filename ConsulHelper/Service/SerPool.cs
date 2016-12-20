using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BitAuto.Ucar.Utils.Common.Service
{
    public abstract class SerPool : IDisposable
    {
        #region 内部成员
        /// <summary>
        /// 连接缓存池
        /// </summary>
        protected Queue<ISerClient> clientsPool;

        /// <summary>
        /// 同步连接
        /// </summary>
        protected AutoResetEvent resetEvent;

        /// <summary>
        /// 空闲连接数
        /// </summary>
        protected volatile int idleCount = 0;

        /// <summary>
        /// 活动连接数
        /// </summary>
        protected volatile int activeCount = 0;

        /// <summary>
        /// 同步连接锁
        /// </summary>		
        protected object locker = new object();

        /// <summary>
        /// 释放标志
        /// </summary>
        protected bool disposed;

        /// <summary>
        /// 随机数生成器
        /// </summary>
        protected Random rand = new Random();
        #endregion

        #region 属性
        /// <summary>
        /// 服务主机地址
        /// </summary>
        internal string[] Hosts { set; get; }

        /// <summary>
        /// 配置键值
        /// </summary>
        internal SerConfig Config { set; get; }

        /// <summary>
        /// 启动版本，连接池重启时版本递增
        /// </summary>
        internal int Version { set; get; }

        /// <summary>
        /// 连接池最大活动连接数
        /// </summary>
        public int MaxActive { protected set; get; }
        /// <summary>
        /// 连接池最小空闲连接数
        /// </summary>
        public int MinIdle { protected set; get; }
        /// <summary>
        /// 连接池最大空闲连接数
        /// </summary>
        public int MaxIdle { protected set; get; }
        /// <summary>
        /// 通信超时时间，单位毫秒
        /// </summary>
        public int ClientTimeout { protected set; get; }

        /// <summary>
        /// 空闲连接数
        /// </summary>
        public int IdleCount
        {
            get
            {
                return idleCount;
            }
        }

        /// <summary>
        /// 活动连接数
        /// </summary>
        public int ActiveCount
        {
            get
            {
                return activeCount;
            }
        }
        #endregion

        #region 构造方法

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hosts">服务器地址及端口</param>
        /// <param name="config">配置字典</param>
        protected SerPool(string[] hosts, SerConfig config)
        {
            //参数校验及赋值
            if (hosts == null)
            {
                throw new ArgumentNullException("hosts");
            }
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            this.Hosts = hosts;
            this.Config = config;
            this.Version = 0;

            //初始化
            CreateResetEvent();
            CreatePool(hosts);
        }


        #endregion

        #region 公有操作方法

        /// <summary>
        /// 从连接池取出一个连接
        /// </summary>
        /// <returns>连接</returns>
        public virtual ISerClient BorrowClient()
        {
            if (Monitor.TryEnter(locker, TimeSpan.FromMilliseconds(ClientTimeout)))
            {
                try
                {
                    ISerClient client = null;
                    Exception innerErr = null;
                    var validClient = false;
                    //连接池无空闲连接	

                    if (idleCount > 0 && !validClient)
                    {
                        client = DequeueClient();
                        validClient = ValidateClient(client, out innerErr);
                        if (!validClient)
                        {
                            DestoryClient(client);
                        }
                    }

                    //连接池无空闲连接	
                    if (!validClient)
                    {
                        //连接池已已创建连接数达上限				
                        if (activeCount > MaxActive)
                        {
                            if (!resetEvent.WaitOne(ClientTimeout))
                            {
                                throw new TimeoutException("连接池繁忙，暂无可用连接。");
                            }
                        }
                        else
                        {
                            client = InitializeClient(out innerErr);
                            if (client == null)
                            {
                                throw new InvalidOperationException("连接获取失败，请确认调用服务状态。", innerErr);
                            }
                        }
                    }

                    //空闲连接数小于最小空闲数，添加一个连接到连接池（已创建数不能超标）			
                    if (idleCount < MinIdle && activeCount < MaxActive)
                    {
                        var candiate = InitializeClient(out innerErr);
                        if (candiate != null)
                        {
                            EnqueueClient(candiate);
                        }
                    }

                    return client;
                }
                finally
                {
                    Monitor.Exit(locker);
                }
            }
            else
            {
                throw new TimeoutException("获取连接等待超过" + ClientTimeout + "毫秒。");
            }
        }

        /// <summary>
        /// 归还一个连接至连接池
        /// </summary>
        /// <param name="client">连接</param>
        public virtual void ReturnClient(ISerClient client)
        {
            lock (locker)
            {
                //空闲连接数达到上限或者连接版本过期，不再返回线程池,直接销毁			
                if (idleCount >= MaxIdle ||
                    this.Version != client.Version)
                {
                    DestoryClient(client);
                }
                else
                {
                    //if (ValidateClient(client))
                    //{
                    //    //有效连接回归连接池
                    //    client.Reset();
                    //    EnqueueClient(client);
                    //}
                    //else
                    //{
                    //    //无效连接回收
                    //    DestoryClient(client);
                    //}
                    //连接回归连接池
                    EnqueueClient(client);
                    //发通知信号，连接池有连接变动
                    resetEvent.Set();
                }
            }
        }

        /// <summary>
        /// 重置连接池
        /// </summary>
        public virtual void ResetPool(string[] hosts)
        {
            CreatePool(hosts);
        }

        /// <summary>
        /// 释放连接池
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region 私有方法

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    lock (locker)
                    {
                        Version++;
                        while (idleCount > 0)
                        {
                            DequeueClient().Close();
                        }
                    }
                }
                disposed = true;
            }
        }


        /// <summary>
        /// 创建线程同步对象
        /// </summary>
        protected virtual void CreateResetEvent()
        {
            lock (locker)
            {
                if (resetEvent == null)
                {
                    resetEvent = new AutoResetEvent(false);
                }
            }
        }

        /// <summary>
        /// 创建连接池
        /// </summary>

        protected virtual void CreatePool(string[] hosts)
        {
            lock (locker)
            {
                //结点列表初始化
                Hosts = hosts;

                //版本增加
                Version++;

                //读取配置
                MaxActive = Config.GetIntValue("MaxActive", 100);
                MinIdle = Config.GetIntValue("MinIdle", 2);
                MaxIdle = Config.GetIntValue("MaxIdle", 10);
                ClientTimeout = Config.GetIntValue("ClientTimeout", 5000);

                if (clientsPool == null)
                {
                    clientsPool = new Queue<ISerClient>();
                }
                else
                {
                    while (idleCount > 0)
                    {
                        DestoryClient(DequeueClient());
                    }

                    clientsPool = new Queue<ISerClient>();
                }
            }
        }


        /// <summary>
        /// 连接进入连接池
        /// </summary>
        /// <param name="client">连接</param>
        protected void EnqueueClient(ISerClient client)
        {
            clientsPool.Enqueue(client);
            idleCount++;
        }

        /// <summary>
        /// 连接取出连接池
        /// </summary>
        /// <returns>连接</returns>
        protected ISerClient DequeueClient()
        {
            var client = clientsPool.Dequeue();
            idleCount--;
            return client;
        }

        /// <summary>
        /// 创建一个连接，虚函数，应由特定连接池继承
        /// </summary>
        /// <returns>连接</returns>
        protected virtual ISerClient CreateClient(string hostInfo)
        {
            return null;
        }

        /// <summary>
        /// 初始化连接，隐藏创建细节
        /// </summary>
        /// <returns>连接</returns>
        protected ISerClient InitializeClient(out Exception err)
        {
            err = null;
            if (Hosts.Length == 0)
            {
                err = new NullReferenceException("没有可用服务节点");
                return null;
            }
            var hostIndexs = new int[Hosts.Length];
            int j = 0;
            for (var i = 0; i < Hosts.Length; i++)
            {
                hostIndexs[i] = i;
            }
            for (var i = 0; i < Hosts.Length; i++)
            {
                try
                {
                    j = rand.Next(Hosts.Length);
                    ISerClient client = CreateClient(Hosts[hostIndexs[j]]);
                    if (ValidateClient(client, out err))
                    {
                        activeCount++;
                        client.Reset();
                        return client;
                    }
                }
                catch (Exception e)
                {
                    hostIndexs[j] = hostIndexs[(j + 1) % Hosts.Length];
                    err = e;
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// 校验连接，确保连接开启
        /// </summary>
        /// <param name="client">连接</param>

        protected bool ValidateClient(ISerClient client, out Exception err)
        {
            try
            {
                client.Open();
                err = null;
                return true;
            }
            catch (Exception e)
            {
                err = e;
                return false;
            }
        }

        /// <summary>
        /// 销毁连接
        /// </summary>
        /// <param name="client">连接</param>
        protected void DestoryClient(ISerClient client)
        {
            if (client != null)
            {
                client.Close();
            }
            activeCount--;
        }

        #endregion
    }
}
