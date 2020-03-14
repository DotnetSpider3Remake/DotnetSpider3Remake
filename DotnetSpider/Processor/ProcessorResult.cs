using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Processor
{
    /// <summary>
    /// 页面解析器的处理结果类。
    /// </summary>
    public class ProcessorResult
    {
        /// <summary>
        /// 是否需要重试当前页面。
        /// </summary>
        public bool Retry { get; set; } = false;

        /// <summary>
        /// 页面解析出来的目标链接不加入到调度队列中。
        /// </summary>
        public bool SkipTargetRequests { get; set; } = false;

        /// <summary>
        /// 忽略当前页面不作解析处理。
        /// </summary>
        public bool Bypass { get; set; } = false;

        /// <summary>
        /// 页面解析的数据结果。
        /// </summary>
        public Dictionary<string, object> ResultItems { get; } = new Dictionary<string, object>();

        /// <summary>
        /// 页面解析到的目标链接。
        /// </summary>
        public HashSet<Request> TargetRequests { get; } = new HashSet<Request>();

        /// <summary>
        /// 本次处理的请求。
        /// </summary>
        public Request Request { get; } = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="request">来源请求，是IResponseProcessor处理的一个请求，ProcessorResult作为它的结果返回</param>
        /// <exception cref="ArgumentException">request不是一个合法请求。</exception>
        /// <exception cref="ArgumentNullException">request为null。</exception>
        public ProcessorResult(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException($"Request can not be null !\n{ request }", "request");
            }
            else if (IsAvailable(request))
            {
                Request = request;
            }
            else
            {
                throw new ArgumentException($"It is not an legal Request!\n{ request }", "request");
            }
        }

        /// <summary>
        /// 添加解析到的目标链接, 添加到队列中
        /// </summary>
        /// <param name="urls">链接</param>
        /// <param name="copyHeaders">是否复制源请求的Headers，不会覆盖已有的Header值<</param>
        public void AddTargetRequests(IEnumerable<string> urls, bool copyHeaders = true)
        {
            if (urls == null)
            {
                return;
            }

            foreach (string url in urls)
            {
                AddTargetRequest(url, copyHeaders);
            }
        }

        /// <summary>
        /// 添加解析到的目标链接, 添加到队列中
        /// </summary>
        /// <param name="requests">链接</param>
        /// <param name="copyHeaders">是否复制源请求的Headers，不会覆盖已有的Header值<</param>
        public void AddTargetRequests(IEnumerable<Request> requests, bool copyHeaders = true)
        {
            if (requests == null)
            {
                return;
            }

            foreach (var request in requests)
            {
                AddTargetRequest(request, copyHeaders);
            }
        }

        /// <summary>
        /// 添加解析到的目标链接, 添加到队列中
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="copyHeaders">是否复制源请求的Headers，不会覆盖已有的Header值<</param>
        /// <param name="properties">附加到Request中的属性，如果为null，表示使用源请求的properties</param>
        public void AddTargetRequest(string url, bool copyHeaders = true, Dictionary<string, object> properties = null)
        {
            if (string.IsNullOrWhiteSpace(url) || url.Equals("#") || url.StartsWith("javascript:"))
            {
                return;
            }

            var newUrl = CanonicalizeUrl(url, Request.Url);
            var request = new Request(newUrl, properties ?? new Dictionary<string, object>(Request.Properties));
            AddTargetRequest(request, copyHeaders);
        }

        /// <summary>
        /// 添加解析到的目标链接, 添加到队列中
        /// </summary>
        /// <param name="request">链接</param>
        /// <param name="copyHeaders">是否复制源请求的Headers到传入的request实例中，不会覆盖已有的Header值</param>
        public void AddTargetRequest(Request request, bool copyHeaders = true)
        {
            if (request is null || !IsAvailable(request))
            {
                return;
            }

            if (copyHeaders && Request != request)
            {
                if (string.IsNullOrWhiteSpace(request.EncodingName))
                {
                    request.EncodingName = Request.EncodingName;
                }

                foreach (var header in Request.Headers)
                {
                    if (request.Headers.ContainsKey(header.Key) == false)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                if (string.IsNullOrWhiteSpace(request.Accept))
                {
                    request.Accept = Request.Accept;
                }

                if (string.IsNullOrWhiteSpace(request.Origin))
                {
                    request.Origin = Request.Origin;
                }

                if (string.IsNullOrWhiteSpace(request.Referer))
                {
                    request.Referer = Request.Referer;
                }

                if (string.IsNullOrWhiteSpace(request.UserAgent))
                {
                    request.UserAgent = Request.UserAgent;
                }
            }

            TargetRequests.Add(request);
        }

        private static bool IsAvailable(Request request)
        {
            if (request is null || request.Url is null)
            {
                return false;
            }

            var url = request.Url.ToString();
            if (url.Length < 6)
            {
                return false;
            }

            var schema = url.Substring(0, 5).ToLower();
            if (!schema.StartsWith("http") && !schema.StartsWith("https"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 计算最终的URL
        /// </summary>
        /// <param name="url">Base uri</param>
        /// <param name="refer">Relative uri</param>
        /// <returns>最终的URL</returns>
        private static string CanonicalizeUrl(string url, string refer)
        {
            try
            {
                Uri bas = new Uri(refer);
                Uri abs = new Uri(bas, url);
                return abs.AbsoluteUri;
            }
            catch
            {
                return url;
            }
        }
    }
}
