using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Common
{
    /// <summary>
    /// Hash函数类
    /// </summary>
    public static class HashCodeExtend
    {
        /// <summary>
        /// 作为乘法因子的魔术数。
        /// </summary>
        private const int _magicCode = 16777619;

        /// <summary>
        /// Hash函数参考起始值。
        /// </summary>
        public static int BeginCode => -2128831035;

        /// <summary>
        /// 获取Hash值。
        /// </summary>
        /// <param name="lastValue">上一个Hash值</param>
        /// <param name="o">需要计算的实例</param>
        /// <returns>Hash值</returns>
        public static int GetHashCode(int lastValue, object o)
        {
            unchecked
            {
                return (lastValue * _magicCode) ^ (o?.GetHashCode() ?? 0);
            }
        }

        /// <summary>
        /// 获取IDictionary实例的Hash值。
        /// </summary>
        /// <typeparam name="TKey">IDictionary中Key的类型</typeparam>
        /// <typeparam name="TValue">IDictionary中Value的类型</typeparam>
        /// <param name="lastValue">上一个Hash值</param>
        /// <param name="dic">需要计算的IDictionary实例</param>
        /// <returns>Hash值</returns>
        public static int GetHashCode<TKey, TValue>(int lastValue, IDictionary<TKey, TValue> dic)
        {
            unchecked
            {
                if (dic == null)
                {
                    return (lastValue * _magicCode) ^ 0;
                }

                foreach (var i in dic)
                {
                    lastValue = GetHashCode(lastValue, i.Key);
                    lastValue = GetHashCode(lastValue, i.Value);
                }

                return (lastValue * _magicCode) ^ lastValue;
            }
        }
    }
}
