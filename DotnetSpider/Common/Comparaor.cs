using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Common
{
    public static class Comparaor
    {
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
