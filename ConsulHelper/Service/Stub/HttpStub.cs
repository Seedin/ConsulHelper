using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BitAuto.Ucar.Utils.Common.Service.Stub
{
    /// <summary>
    /// HTTP代理类
    /// </summary>
    public class HttpStub
    {
        /// <summary>
        /// http客户端
        /// </summary>
        protected HttpClient client;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="client">http长连接客户端</param>
        public HttpStub(HttpClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Http头
        /// </summary>
        public HttpRequestHeaders Headers
        {
            get
            {
                return client.DefaultRequestHeaders;
            }
        }

        /// <summary>
        /// 对象公共字段或属性以键值对有序字典输出，不包含Null值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>键值对有序字典</returns>
        protected List<KeyValuePair<string, string>> ObjToStrParas(object obj)
        {
            var retParas = new List<KeyValuePair<string, string>>();
            if (obj == null)
            {
                return retParas;
            }

            //获取对象类型
            var objType = obj.GetType();

            //写入公共字段
            foreach (var field in objType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var filedValue = field.GetValue(obj);
                if (filedValue != null)
                {
                    retParas.Add(new KeyValuePair<string, string>(field.Name, filedValue.ToString()));
                }
            }

            //写入公共属性
            foreach (var property in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyValue = property.GetValue(obj);
                if (propertyValue != null)
                {
                    retParas.Add(new KeyValuePair<string, string>(property.Name, propertyValue.ToString()));
                }
            }

            //返回映射字典
            return retParas;
        }

        /// <summary>
        /// Post请求Json数据
        /// </summary>
        /// <typeparam name="U">请求POCO类型</typeparam>
        /// <typeparam name="V">响应POCO类型</typeparam>
        /// <param name="relatedUrl">相对url，可含参数</param>
        /// <param name="reqObj">请求POCO实例</param>
        /// <param name="paraInForm">请求参数是否为form形式，否则合并至Url</param>
        /// <returns>响应POCO实例</returns>
        public async Task<V> PostJson<U, V>(string relatedUrl, U reqObj, bool paraInForm = true)
        {
            this.Headers.Accept.TryParseAdd("application/json");
            var paras = ObjToStrParas(reqObj);
            FormUrlEncodedContent content;
            if (paraInForm)
            {
                content = new FormUrlEncodedContent(paras);
            }
            else
            {
                var paraStr = relatedUrl.Contains('?') ? "&" : "?" +
                    string.Join("&", paras.Select(para => para.Key + "=" + HttpUtility.UrlEncode(para.Value)));
                relatedUrl += paraStr;
                content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>());
            }


            var response = await client.PostAsync(relatedUrl, content).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<V>(response.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Post请求字符串数据
        /// </summary>
        /// <param name="relatedUrl">相对url，可含参数</param>
        /// <param name="formParas">Form参数键值对集合</param>
        /// <returns>响应字符串</returns>
        public async Task<string> Post(string relatedUrl, IEnumerable<KeyValuePair<string, string>> formParas)
        {
            var response = await client.PostAsync(relatedUrl, new FormUrlEncodedContent(formParas)).ConfigureAwait(false);
            return response.Content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// Get请求Json数据
        /// </summary>
        /// <typeparam name="U">请求POCO类型</typeparam>
        /// <typeparam name="V">响应POCO类型</typeparam>
        /// <param name="relatedUrl">相对url，可含参数</param>
        /// <param name="reqObj">请求POCO实例</param>
        /// <returns>响应POCO实例</returns>
        public async Task<V> GetJson<U, V>(string relatedUrl, U reqObj)
        {
            this.Headers.Accept.TryParseAdd("application/json");
            var paras = ObjToStrParas(reqObj);
            var paraStr = string.Empty;
            if (paras.Count > 0)
            {
                paraStr = relatedUrl.Contains('?') ? "&" : "?" +
                string.Join("&", paras.Select(para => para.Key + "=" + HttpUtility.UrlEncode(para.Value)));
            }
            var response = await client.GetAsync(relatedUrl + paraStr).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<V>(response.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Get请求字符串数据
        /// </summary>
        /// <param name="relatedUrl">相对url，可含参数</param>
        /// <returns>响应字符串</returns>
        public async Task<string> Get(string relatedUrl)
        {
            var response = await client.GetAsync(relatedUrl).ConfigureAwait(false);
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
