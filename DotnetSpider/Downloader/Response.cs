using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 链接请求结果
    /// </summary>
    public class Response
    {
        public Response()
        {
        }

        public Response(Request request)
        {
            Request = request;
        }

        /// <summary>
        /// 链接请求
        /// </summary>
        public Request Request { get; set; }

        /// <summary>
        /// 最终请求的链接, 当发生302跳转时可能与请求的Url不一致
        /// </summary>
        public string TargetUrl { get; set; }

        /// <summary>
        /// 请求的结果, 一般情况下都是 String, 特殊情况下可以重载 Downloader 返回的是下载的二进制流
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// 请求结果的类型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// HTTP返回代码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 使用下载器时，是否发生超时现象。
        /// 当<see cref="IsDownloaderTimeout"/>为true时，其他属性（<see cref="Request"/>除外）均为默认值。
        /// </summary>
        public bool IsDownloaderTimeout { get; set; }

        /// <summary>
        /// 请求是否成功执行
        /// </summary>
        public bool IsSuccessStatusCode => StatusCode < HttpStatusCode.BadRequest && StatusCode >= HttpStatusCode.OK;

        public List<Dictionary<string, string>> SetCookies { get; set; }

        public Dictionary<string, HashSet<string>> Headers { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// 从JSON字符串构建Response。
        /// </summary>
        /// <param name="s">JSON字符串</param>
        /// <returns>Response实例</returns>
        public static Response FromString(string s)
        {
            return JsonConvert.DeserializeObject<Response>(s);
        }
    }
}