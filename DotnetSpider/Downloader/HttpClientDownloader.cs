using DotnetSpider.Proxy.Helper;
using LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        protected readonly Dictionary<int, Tuple<HttpClient, DynamicProxy>> _httpClients = new Dictionary<int, Tuple<HttpClient, DynamicProxy>>();
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
        public readonly HashSet<string> ExcludeMediaTypes = new HashSet<string>
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
        public Func<HttpMessageHandler, HttpClient> HttpClientGetter { get; set; } = handler => new HttpClient(handler);

        protected override async Task<Response> Downloading(Request request, IWebProxy proxy = null)
        {
            Response response = new Response(request)
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                IsDownloaderTimeout = false
            };
            try
            {
                var httpClient = GetHttpClient(request, proxy);
                using (var message = GenerateHttpRequestMessage(request))
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(Timeout);
                    try
                    {
                        using (var httpResponse = await httpClient.SendAsync(message, cts.Token))
                        {
                            response.StatusCode = httpResponse.StatusCode;
                            response.Headers = GetResponseHeaders(httpResponse.Headers);
                            response.SetCookies = GetSetCookies(response.Headers);
                            response.TargetUrl = httpResponse.RequestMessage.RequestUri.AbsoluteUri;
                            if (httpResponse.Content != null)
                            {
                                AppendContentHeaders(response.Headers, httpResponse.Content.Headers);
                                response.ContentType = httpResponse.Content.Headers?.ContentType?.ToString() ?? string.Empty;
                                if (ExcludeMediaTypes.Contains(response.ContentType))
                                {
                                    response.Content = await httpResponse.Content.ReadAsStringAsync();
                                }
                                else
                                {
                                    response.Content = await httpResponse.Content.ReadAsByteArrayAsync();
                                }
                            }
                        }
                    }
                    catch (TaskCanceledException e)
                    {
                        if (e.CancellationToken == cts.Token)
                        {
                            response.StatusCode = HttpStatusCode.RequestTimeout;
                            response.IsDownloaderTimeout = true;
                        }

                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.Error($"HTTP请求时发生异常，请求内容： { request }", e);
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
        /// 如果只需要更改<see cref="HttpClient"/>的类型，请为<see cref="HttpClientGetter"/>赋值。
        /// </summary>
        /// <param name="request">即将调用的请求。</param>
        /// <returns><see cref="HttpClient"/>实例</returns>
        protected virtual HttpClient GetHttpClient(Request request, IWebProxy proxy)
        {
            int threadNum = Thread.CurrentThread.ManagedThreadId;
            lock (_httpClientsLocker)
            {
                if (_httpClients.ContainsKey(threadNum) == false)
                {
                    var handlerTuple = CreateHttpMessageHandler();
                    var httpClient = HttpClientGetter(handlerTuple.Item1);
                    _httpClients.Add(threadNum,
                        new Tuple<HttpClient, DynamicProxy>(httpClient, handlerTuple.Item2));
                }

                var cur = _httpClients[threadNum];
                cur.Item2.InnerProxy = proxy;
                return cur.Item1;
            }
        }

        protected virtual Tuple<HttpMessageHandler, DynamicProxy> CreateHttpMessageHandler()
        {
            DynamicProxy proxy = new DynamicProxy();
            HttpClientHandler handler = new HttpClientHandler
            {
                UseCookies = false,
                UseDefaultCredentials = false,
                UseProxy = true,
                Proxy = proxy
            };
            return new Tuple<HttpMessageHandler, DynamicProxy>(handler, proxy);
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

            if (request.Cookies != null && request.Cookies.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var i in request.Cookies)
                {
                    builder.Append(i.Key);
                    builder.Append(":");
                    builder.Append(i.Value);
                    builder.Append("; ");
                }

                builder.Remove(builder.Length - 2, 2);
                var header = "Cookie";
                httpRequestMessage.Headers.Remove(header);
                httpRequestMessage.Headers.TryAddWithoutValidation(header, builder.ToString());
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

        private Dictionary<string, HashSet<string>> GetResponseHeaders(HttpResponseHeaders headers)
        {
            Dictionary<string, HashSet<string>> res = new Dictionary<string, HashSet<string>>();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (res.ContainsKey(header.Key))
                    {
                        res[header.Key].UnionWith(header.Value);
                    }
                    else 
                    {
                        res.Add(header.Key, new HashSet<string>(header.Value));
                    }
                }
            }

            return res;
        }

        private void AppendContentHeaders(Dictionary<string, HashSet<string>> headers, HttpContentHeaders contentHeaders)
        {
            if (contentHeaders == null)
            {
                return;
            }

            foreach (var header in contentHeaders)
            {
                if (headers.ContainsKey(header.Key))
                {
                    headers[header.Key].UnionWith(header.Value);
                }
                else
                {
                    headers.Add(header.Key, new HashSet<string>(header.Value));
                }
            }
        }

        private List<Dictionary<string, string>> GetSetCookies(Dictionary<string, HashSet<string>> header)
        {
            List<Dictionary<string, string>> cookies = new List<Dictionary<string, string>>();
            if (header.ContainsKey("Set-Cookie"))
            {
                foreach (var i in header["Set-Cookie"])
                {
                    Dictionary<string, string> cookie = new Dictionary<string, string>();
                    string[] lines = i.Split(';');
                    foreach (var j in lines)
                    {
                        string[] words = j.Split(':');
                        if (words.Length != 2)
                        {
                            continue;
                        }

                        cookie[words[0].Trim()] = words[1].Trim();
                    }

                    if (cookie.Count > 0)
                    {
                        cookies.Add(cookie);
                    }
                }
            }

            return cookies;
        }
    }
}
