using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Xml;

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
        /// 请求的结果, 一般情况下都是string, 特殊情况下返回的是ByteArray。
        /// 具体类型由Downloader决定。
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// 请求结果的类型
        /// </summary>
        public string ContentType { get; set; }

        private HtmlNode _htmlNode = null;
        /// <summary>
        /// 解析Content后得到的html节点
        /// </summary>
        [JsonIgnore]
        public HtmlNode HtmlNode
        {
            get
            {
                if (_htmlNode != null)
                {
                    return _htmlNode;
                }

                string content = Content as string;
                if (content == null)
                {
                    return null;
                }

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(content);
                _htmlNode = htmlDocument.DocumentNode;
                return _htmlNode;
            }
        }

        private XmlElement _xmlElement = null;
        /// <summary>
        /// 解析Content后得到的xml节点
        /// </summary>
        [JsonIgnore]
        public XmlElement XmlElement
        {
            get
            {
                if (_xmlElement != null)
                {
                    return _xmlElement;
                }

                string content = Content as string;
                if (content == null)
                {
                    return null;
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                _xmlElement = doc.DocumentElement;
                return _xmlElement;
            }
        }

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
        [JsonIgnore]
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