using System;
using System.Text;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace BitAuto.Ucar.Utils.Common.Service.Stub
{
    /// <summary>
    /// 通用代理类
    /// </summary>
    public class CommonStub
    {
        /// <summary>
        /// http客户端
        /// </summary>
        protected ISerClient client;

        /// <summary>
        /// 真实代理类
        /// </summary>
        protected object realStub;

        /// <summary>
        /// 通道类型
        /// </summary>
        protected Type chanelType;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="client">初始化</param>
        public CommonStub(ISerClient client)
        {
            this.client = client;
            chanelType = Type.GetType(client.Owner.Config["ChanelType"]);
            realStub = client.GetStub(chanelType);
        }

        /// <summary>
        /// 数据获取通用接口
        /// </summary>
        /// <typeparam name="U">响应类型</typeparam>
        /// <param name="relativeUrl">相对url</param>
        /// <param name="paras">参数</param>
        /// <returns>响应</returns>
        public U Get<U>(string relativeUrl, params object[] paras)
        {
            var result = Get(relativeUrl, paras);
            switch (client.GetType().Name)
            {
                case "GrpcClient":
                case "ThriftClient":
                case "WcfClient":
                    return result != null ? (U)result : default(U);
                case "HttpClient":
                    return JsonConvert.DeserializeObject<U>((string)result);
                default:
                    return default(U);
            }
        }

        /// <summary>
        /// 数据获取通用接口
        /// </summary>
        /// <param name="relativeUrl">相对URL</param>
        /// <param name="paras">参数</param>
        /// <returns>响应</returns>
        public object Get(string relativeUrl, params object[] paras)
        {
            var urlUnits = relativeUrl.Split('/').Where(unit => !string.IsNullOrEmpty(unit)).ToArray();
            if (urlUnits.Length == 0)
            {
                return null;
            }
            var methodName = urlUnits[0];
            MethodInfo method;
            Type[] paraTypes;
            switch (client.GetType().Name)
            {
                case "GrpcClient":
                    paraTypes = new Type[paras.Length + 1];
                    var parasPlus = new object[paras.Length + 1];
                    for (var i = 0; i < paras.Length; i++)
                    {
                        paraTypes[i] = paras[i].GetType();
                        parasPlus[i] = paras[i];
                    }
                    paraTypes[paras.Length] = typeof(Grpc.Core.CallOptions);
                    parasPlus[paras.Length] = new Grpc.Core.CallOptions()
                        .WithDeadline(DateTime.UtcNow.AddMilliseconds(client.Owner.ClientTimeout));
                    method = chanelType.GetMethod(methodName, paraTypes);
                    return method.Invoke(realStub, parasPlus);
                case "ThriftClient":
                case "WcfClient":
                    paraTypes = new Type[paras.Length];
                    for (var i = 0; i < paras.Length; i++)
                    {
                        paraTypes[i] = paras[i].GetType();
                    }
                    method = chanelType.GetMethod(methodName, paraTypes);
                    return method.Invoke(realStub, paras);
                case "HttpClient":
                    var stub = (HttpStub)realStub;
                    return stub.GetJson(relativeUrl, paras).GetAwaiter().GetResult();
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// 数据提交通用接口
        /// </summary>
        /// <typeparam name="U">响应类型</typeparam>
        /// <param name="relativeUrl">相对URL</param>
        /// <param name="paras">参数</param>
        /// <returns>响应</returns>
        public U Post<U>(string relativeUrl, params object[] paras)
        {
            var result = Post(relativeUrl, paras);
            switch (client.GetType().Name)
            {
                case "GrpcClient":
                case "ThriftClient":
                case "WcfClient":
                    return result != null ? (U)result : default(U);
                case "HttpClient":
                    return JsonConvert.DeserializeObject<U>((string)result);
                default:
                    return default(U);
            }
        }

        /// <summary>
        /// 数据提交通用接口
        /// </summary>
        /// <param name="relativeUrl">相对URL</param>
        /// <param name="paras">参数</param>
        /// <returns>响应</returns>
        public object Post(string relativeUrl, params object[] paras)
        {
            var urlUnits = relativeUrl.Split('/').Where(unit => !string.IsNullOrEmpty(unit)).ToArray();
            if (urlUnits.Length == 0)
            {
                return null;
            }
            var methodName = urlUnits[0];
            MethodInfo method;
            Type[] paraTypes;
            switch (client.GetType().Name)
            {
                case "GrpcClient":
                    paraTypes = new Type[paras.Length + 1];
                    var parasPlus = new object[paras.Length + 1];
                    for (var i = 0; i < paras.Length; i++)
                    {
                        paraTypes[i] = paras[i].GetType();
                        parasPlus[i] = paras[i];
                    }
                    paraTypes[paras.Length] = typeof(Grpc.Core.CallOptions);
                    parasPlus[paras.Length] = new Grpc.Core.CallOptions()
                        .WithDeadline(DateTime.UtcNow.AddMilliseconds(client.Owner.ClientTimeout));
                    method = chanelType.GetMethod(methodName, paraTypes);
                    return method.Invoke(realStub, parasPlus);
                case "ThriftClient":
                case "WcfClient":
                    paraTypes = new Type[paras.Length];
                    for (var i = 0; i < paras.Length; i++)
                    {
                        paraTypes[i] = paras[i].GetType();
                    }
                    method = chanelType.GetMethod(methodName, paraTypes);
                    return method.Invoke(realStub, paras);
                case "HttpClient":
                    var stub = (HttpStub)realStub;
                    HttpStub.ParaMode mode;
                    try
                    {
                        mode = (HttpStub.ParaMode)int.Parse(client.Owner.Config["ParaMode"]);
                    }
                    catch
                    {
                        mode = HttpStub.ParaMode.UrlPara;
                    }
                    return stub.PostJson(relativeUrl, mode, paras).GetAwaiter().GetResult();
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// 数据提交通用接口，无需响应结果
        /// </summary>
        /// <param name="relativeUrl">相对URL</param>
        /// <param name="paras">参数</param>
        public void PostOnly(string relativeUrl, params object[] paras)
        {
            var urlUnits = relativeUrl.Split('/').Where(unit => !string.IsNullOrEmpty(unit)).ToArray();
            if (urlUnits.Length == 0)
            {
                return;
            }
            var methodName = urlUnits[0];
            MethodInfo method;
            Type[] paraTypes;
            switch (client.GetType().Name)
            {
                case "GrpcClient":
                    paraTypes = new Type[paras.Length + 1];
                    var parasPlus = new object[paras.Length + 1];
                    for (var i = 0; i < paras.Length; i++)
                    {
                        paraTypes[i] = paras[i].GetType();
                        parasPlus[i] = paras[i];
                    }
                    paraTypes[paras.Length] = typeof(Grpc.Core.CallOptions);
                    parasPlus[paras.Length] = new Grpc.Core.CallOptions()
                        .WithDeadline(DateTime.UtcNow.AddMilliseconds(client.Owner.ClientTimeout));
                    method = chanelType.GetMethod(methodName, paraTypes);
                    method.Invoke(realStub, parasPlus);
                    break;
                case "ThriftClient":
                case "WcfClient":
                    paraTypes = new Type[paras.Length];
                    for (var i = 0; i < paras.Length; i++)
                    {
                        paraTypes[i] = paras[i].GetType();
                    }
                    method = chanelType.GetMethod(methodName, paraTypes);
                    method.Invoke(realStub, paras);
                    break;
                case "HttpClient":
                    var stub = (HttpStub)realStub;
                    HttpStub.ParaMode mode;
                    try
                    {
                        mode = (HttpStub.ParaMode)int.Parse(client.Owner.Config["ParaMode"]);
                    }
                    catch
                    {
                        mode = HttpStub.ParaMode.UrlPara;
                    }
                    stub.PostJson(relativeUrl, mode, paras).GetAwaiter().GetResult();
                    break;
                default:
                    break;
            }
        }
    }
}
