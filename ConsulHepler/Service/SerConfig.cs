using System.Collections.Generic;

namespace BitAuto.Ucar.Utils.Common.Service
{
    /// <summary>
    /// 服务配置字典
    /// </summary>
    public class SerConfig : Dictionary<string, string>
    {
        /// <summary>
        /// 获取整型值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="alternative">代值</param>
        /// <returns>值</returns>
        public int GetIntValue(string key, int alternative = 0)
        {
            try
            {
                return int.Parse(this[key]);
            }
            catch
            {
                return alternative;
            }
        }

        /// <summary>
        /// 获取长整型值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="alternative">代值</param>
        /// <returns>值</returns>
        public long GetLongValue(string key, long alternative = 0)
        {
            try
            {
                return long.Parse(this[key]);
            }
            catch
            {
                return alternative;
            }
        }

        /// <summary>
        /// 获取浮点型值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="alternative">代值</param>
        /// <returns>值</returns>
        public double GetDoubleValue(string key, double alternative = 0)
        {
            try
            {
                return double.Parse(this[key]);
            }
            catch
            {
                return alternative;
            }
        }

        /// <summary>
        /// 获取布尔型值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="alternative">代值</param>
        /// <returns>值</returns>
        public bool GetBoolenValue(string key, bool alternative = false)
        {
            try
            {
                return bool.Parse(this[key]);
            }
            catch
            {
                return alternative;
            }
        }

        /// <summary>
        /// 获取字符串型值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="alternative">代值</param>
        /// <returns>值</returns>
        public string GetStringValue(string key, string alternative = null)
        {
            try
            {
                return this[key];
            }
            catch
            {
                return alternative;
            }
        }
    }
}
