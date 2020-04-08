using DotnetSpider.Proxy.Helper;
using LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader
{
    public class HttpClientDownloader : BaseDowloader
    {
        /// <summary>
        /// <see cref="HttpClient"/>实例集合。
        /// 在调用<see cref="IDisposable.Dispose"/>时，会自动释放所有<see cref="HttpClient"/>。
        /// </summary>
        protected readonly Dictionary<int, Tuple<HttpClient, DynamicProxy, CookieContainer>> _httpClients = new Dictionary<int, Tuple<HttpClient, DynamicProxy, CookieContainer>>();
        /// <summary>
        /// <see cref="_httpClients"/>的读写锁。
        /// </summary>
        protected readonly object _httpClientsLocker = new object();

        /// <summary>
        /// What mediatype should not be treated as file to download.
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 定义哪些类型的内容不需要当成文件下载，即文本格式。
        /// </summary>
        public readonly List<string> ExcludeMediaTypes = new List<string>
        {
            "",
            "text/html",
            "text/plain",
            "text/richtext",
            "text/xml",
            "text/XML",
            "text/json",
            "text/javascript",
            "application/soap+xml",
            "application/xml",
            "application/json",
            "application/x-javascript",
            "application/javascript",
            "application/x-www-form-urlencoded"
        };

        /// <summary>
        /// 构造HttpClient函数，一般每个线程中只调用一次。
        /// </summary>
        public Func<HttpMessageHandler, HttpClient> HttpClientGetter { get; set; }

        /// <summary>
        /// 是否使用Cookies。如果为false，<see cref="Request.Cookies"/>将被忽略。
        /// 默认为false。
        /// </summary>
        public bool UseCookies { get; set; } = false;

        protected override async Task<Response> Downloading(Request request, IWebProxy proxy = null)
        {
            Response response = new Response(request);
            var httpClient = GetHttpClient(request, proxy);
            using (var message = GenerateHttpRequestMessage(request))
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(Timeout);
                try
                {
                    using (var httpResponse = await httpClient.SendAsync(message, cts.Token))
                    {

                    }
                }
                catch (TaskCanceledException e)
                {
                    if (e.CancellationToken == cts.Token)
                    {
                        response.IsDownloaderTimeout = true;
                    }
                    else
                    {
                        
                    }
                }
            }

            return response;
        }

        protected override void DisposeOthers()
        {
            base.DisposeOthers();
            lock (_httpClientsLocker)
            {
                foreach (var i in _httpClients.Values)
                {
                    i.Item1.Dispose();
                }

                _httpClients.Clear();
            }
        }

        /// <summary>
        /// 获取当前线程使用的<see cref="HttpClient"/>实例。
        /// 默认只为每个线程各生成一个<see cref="HttpClient"/>实例。
        /// 一般应在需要修改<see cref="HttpClient"/>实例生存周期时重载。
        /// 如果只需要更改<see cref="HttpMessageHandler"/>、<see cref="HttpClient"/>的类型，请为<see cref="HttpMessageHandlerGetter"/>、<see cref="HttpClientGetter"/>赋值。
        /// </summary>
        /// <param name="request">即将调用的请求。</param>
        /// <returns><see cref="HttpClient"/>实例</returns>
        protected virtual HttpClient GetHttpClient(Request request, IWebProxy proxy)
        {
            int threadNum = Thread.CurrentThread.ManagedThreadId;
            Tuple<HttpClient, DynamicProxy, CookieContainer> cur;
            lock (_httpClientsLocker)
            {
                if (_httpClients.ContainsKey(threadNum) == false)
                {
                    var handlerTuple = CreateHttpMessageHandler();
                    var httpClient = HttpClientGetter?.Invoke(handlerTuple.Item1) ?? CreateHttpClient(handlerTuple.Item1);
                    _httpClients.Add(threadNum,
                        new Tuple<HttpClient, DynamicProxy, CookieContainer>(httpClient, handlerTuple.Item2, handlerTuple.Item3));
                }

                cur = _httpClients[threadNum];
            }

            cur.Item2.InnerProxy = proxy;
            if (UseCookies)
            {
                var cookies = cur.Item3.GetCookies(new Uri(request.Url));
                foreach (Cookie i in cookies)
                {
                    i.Expired = true;
                }

                if (request.Cookies != null)
                {
                    foreach (var i in request.Cookies)
                    {
                        cookies.Add(i);
                    }
                }
            }

            return cur.Item1;
        }

        protected virtual HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            return new HttpClient(handler);
        }

        protected virtual Tuple<HttpMessageHandler, DynamicProxy, CookieContainer> CreateHttpMessageHandler()
        {
            DynamicProxy proxy = new DynamicProxy();
            HttpClientHandler handler = new HttpClientHandler
            {
                UseCookies = UseCookies,
                UseDefaultCredentials = false,
                UseProxy = true,
                Proxy = proxy
            };
            return new Tuple<HttpMessageHandler, DynamicProxy, CookieContainer>(handler, proxy, handler.CookieContainer);
        }

        private HttpRequestMessage GenerateHttpRequestMessage(Request request)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(request.Method, request.Url);

            // Headers 的优先级低于 Request.UserAgent 这种特定设置, 因此先加载所有 Headers, 再使用 Request.UserAgent 覆盖
            foreach (var header in request.Headers)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value?.ToString());
            }

            if (!string.IsNullOrWhiteSpace(request.UserAgent))
            {
                var header = "User-Agent";
                httpRequestMessage.Headers.Remove(header);
                httpRequestMessage.Headers.TryAddWithoutValidation(header, request.UserAgent);
            }

            if (!string.IsNullOrWhiteSpace(request.Referer))
            {
                var header = "Referer";
                httpRequestMessage.Headers.Remove(header);
                httpRequestMessage.Headers.TryAddWithoutValidation(header, request.Referer);
            }

            if (!string.IsNullOrWhiteSpace(request.Origin))
            {
                var header = "Origin";
                httpRequestMessage.Headers.Remove(header);
                httpRequestMessage.Headers.TryAddWithoutValidation(header, request.Origin);
            }

            if (!string.IsNullOrWhiteSpace(request.Accept))
            {
                var header = "Accept";
                httpRequestMessage.Headers.Remove(header);
                httpRequestMessage.Headers.TryAddWithoutValidation(header, request.Accept);
            }

            //添加CONTENT，可以为空字符串。
            //理论上所有类型的请求都可以有CONTENT，但大部分人只用在POST/PUT时使用。
            if (request.ContentData != null)
            {
                var bytes = CompressContent(request);
                httpRequestMessage.Content = new ByteArrayContent(bytes);

                if (!string.IsNullOrWhiteSpace(request.ContentType))
                {
                    var header = "Content-Type";
                    httpRequestMessage.Content.Headers.Remove(header);
                    httpRequestMessage.Content.Headers.TryAddWithoutValidation(header, request.ContentType);
                }

                var xRequestedWithHeader = "X-Requested-With";
                if (request.Headers.ContainsKey(xRequestedWithHeader) &&
                    request.Headers[xRequestedWithHeader].ToString() == "NULL")
                {
                    httpRequestMessage.Content.Headers.Remove(xRequestedWithHeader);
                }
                else
                {
                    if (!httpRequestMessage.Content.Headers.Contains(xRequestedWithHeader) &&
                        !httpRequestMessage.Headers.Contains(xRequestedWithHeader))
                    {
                        httpRequestMessage.Content.Headers.TryAddWithoutValidation(xRequestedWithHeader, "XMLHttpRequest");
                    }
                }
            }

            return httpRequestMessage;
        }

        private byte[] CompressContent(Request request)
        {
            var bytes = request.ContentData;
            switch (request.CompressMode)
            {
                case CompressMode.Lz4:
                    bytes = LZ4Codec.Wrap(bytes);
                    break;

                case CompressMode.None:
                    break;

                case CompressMode.Gzip:
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress))
                        {
                            compressedzipStream.Write(bytes, 0, bytes.Length);
                            compressedzipStream.Close();
                            bytes = ms.ToArray();
                        }
                    }

                    break;

                default:
                    throw new NotImplementedException(request.CompressMode.ToString());
            }

            return bytes;
        }
    }
}
