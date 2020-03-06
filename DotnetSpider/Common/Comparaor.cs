using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Common
{
    /// <summary>
    /// 比较函数类
    /// </summary>
    public static class Comparaor
    {
        /// <summary>
        /// 比较两个IDictionary是否等效，即是否包含相同的内容。
        /// 与a、b的实际类型无关。
        /// </summary>
        /// <typeparam name="TKey">IDictionary中Key的类型</typeparam>
        /// <typeparam name="TValue">IDictionary中Value的类型</typeparam>
        /// <param name="a">第一个字典</param>
        /// <param name="b">第二个字典</param>
        /// <returns>是否等效</returns>
        public static bool AreEquivalent<TKey, TValue>(IDictionary<TKey, TValue> a, IDictionary<TKey, TValue> b)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            foreach (var i in a)
            {
                if (b.ContainsKey(i.Key) == false ||
                    Equals(b[i.Key], i.Value) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
