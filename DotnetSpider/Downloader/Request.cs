using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json;
using DotnetSpider.Common;

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 链接请求
    /// </summary>
    public class Request
    {
        #region Headers

        /// <summary>
        /// User-Agent
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 请求链接时Referer参数的值
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// 请求链接时Origin参数的值
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Accept
        /// </summary>
        public string Accept { get; set; }

        /// <summary>
        /// 仅在发送 POST 请求时需要设置
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Headers
        /// </summary>
        public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();

        #endregion

        /// <summary>
        /// 字符编码
        /// </summary>
        public string EncodingName { get; set; }

        /// <summary>
        /// 请求链接的方法
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.Get;

        /// <summary>
        /// 存储此链接对应的额外数据字典
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 请求此链接时需要POST的数据
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 如果是 POST 请求, 可以设置压缩模式上传数据
        /// </summary>
        public CompressMode CompressMode { get; set; }

        /// <summary>
        /// 请求链接, 不使用 Uri 的原因是可能引起多重编码的问题
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public Request()
        {

        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="url">链接</param>
        public Request(string url)
        {
            Url = url;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="properties">额外属性</param>
        public Request(string url, Dictionary<string, object> properties)
        {
            Url = url;
            if (properties != null)
            {
                Properties = properties;
            }
        }

        /// <summary>
        /// 比较obj是否与当前实例相等。
        /// 不比较Properties属性。
        /// </summary>
        /// <param name="obj">需要比较的对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this == (Request)obj;
        }

        /// <summary>
        /// 比较两个实例是否相等。
        /// 不比较Properties属性。
        /// </summary>
        /// <returns>是否相等</returns>
        public static bool operator == (Request a, Request b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a.Url != b.Url ||
                a.Method != b.Method ||
                a.Accept != b.Accept ||
                a.CompressMode != b.CompressMode ||
                a.Content != b.Content ||
                a.ContentType != b.ContentType ||
                a.EncodingName != b.EncodingName ||
                Comparaor.AreEquivalent(a.Headers, b.Headers) == false ||
                a.Origin != b.Origin ||
                a.Referer != b.Referer ||
                a.UserAgent != b.UserAgent)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 比较两个实例是否不相等。
        /// 不比较Properties属性。
        /// </summary>
        /// <returns>是否不相等</returns>
        public static bool operator !=(Request a, Request b)
        {
            return !(a == b);
        }

        /// <summary>
        /// 计算除Properties属性外的其他属性的Hash值。
        /// </summary>
        /// <returns>Hash值</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int c = HashCode.BeginCode;
                c = HashCode.GetHashCode(c, Accept);
                c = HashCode.GetHashCode(c, CompressMode);
                c = HashCode.GetHashCode(c, Content);
                c = HashCode.GetHashCode(c, ContentType);
                c = HashCode.GetHashCode(c, EncodingName);
                c = HashCode.GetHashCode(c, Headers);
                c = HashCode.GetHashCode(c, Method);
                c = HashCode.GetHashCode(c, Origin);
                c = HashCode.GetHashCode(c, Referer);
                c = HashCode.GetHashCode(c, Url);
                c = HashCode.GetHashCode(c, UserAgent);
                return c;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// 从JSON字符串构建Request。
        /// </summary>
        /// <param name="s">JSON字符串</param>
        /// <returns>Request实例</returns>
        public static Request FromString(string s)
        {
            return JsonConvert.DeserializeObject<Request>(s);
        }
    }
}