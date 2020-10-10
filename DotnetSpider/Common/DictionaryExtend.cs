using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Common
{
    public static class DictionaryExtend
    {
        /// <summary>
        /// 获取值，如果值不存在，返回<see cref="default"/>。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dic">字典</param>
        /// <param name="key">键</param>
        /// <returns>值或默认值</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            if (dic.TryGetValue(key, out TValue value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// 获取值，如果值不存在，返回<see cref="defaultValue"/>。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dic">字典</param>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>值或默认值</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue defaultValue)
        {
            if (dic.TryGetValue(key, out TValue value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
