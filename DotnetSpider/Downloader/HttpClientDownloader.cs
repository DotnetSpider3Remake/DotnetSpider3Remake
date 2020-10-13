using DotnetSpider.Proxy.Helper;
using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 使用<see cref="HttpClient"/>实现的<see cref="IDownloader"/>，具有基本的下载功能。
    /// </summary>
    public class HttpClientDownloader : BaseDowloader
    {
        static HttpClientDownloader()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        #region 局部变量
        /// <summary>
        /// <see cref="HttpClient"/>实例集合。KEY为线程号。
        /// 在调用<see cref="IDisposable.Dispose"/>时，会自动释放所有<see cref="HttpClient"/>。
        /// </summary>
        protected readonly Dictionary<int, Tuple<HttpClient, DynamicProxy>> _httpClients = new Dictionary<int, Tuple<HttpClient, DynamicProxy>>();
        /// <summary>
        /// <see cref="_httpClients"/>的读写锁。
        /// </summary>
        protected readonly object _httpClientsLocker = new object();
        #endregion

        #region 公共属性
        /// <summary>
        /// 定义哪些类型的内容不需要当成文件下载，即文本格式。
        /// 如果<see cref="Response.ContentType"/>为空，也会当作文本格式处理。
        /// </summary>
        public HashSet<string> ExcludeMediaTypes { get; } = new HashSet<string>
        {
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
        #endregion

        #region 实现抽象函数
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
                using var message = GenerateHttpRequestMessage(request);
                var cts = new CancellationTokenSource();
                cts.CancelAfter(Timeout);
                try
                {
                    using var httpResponse = await httpClient.SendAsync(message, cts.Token);
                    await GenerateResponse(response, httpResponse);
                }
                catch (TaskCanceledException e)
                {
                    if (e.CancellationToken == cts.Token)
                    {
                        response.StatusCode = HttpStatusCode.RequestTimeout;
                        response.IsDownloaderTimeout = true;
                    }
                    else if (e.InnerException is IOException)
                    {
                        Logger?.Warn($"HTTP request failed：\"{ e.InnerException.Message }\"\n{ request }");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.Error($"HTTP request failed unexpectedly ： { request }", e);
            }

            return response;
        }

        /// <summary>
        /// 清理所有的<see cref="HttpClient"/>。
        /// </summary>
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
        #endregion

        #region 可以在派生类中重新实现的函数
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

        /// <summary>
        /// 创建HTTP请求需要的<see cref="HttpMessageHandler"/>和<see cref="DynamicProxy"/>，并关联两者。
        /// <see cref="HttpMessageHandler"/>中定义了HTTP请求的一些基本动作。
        /// </summary>
        /// <returns>新的已关联的<see cref="HttpMessageHandler"/>和<see cref="DynamicProxy"/>。</returns>
        protected virtual Tuple<HttpMessageHandler, DynamicProxy> CreateHttpMessageHandler()
        {
            DynamicProxy proxy = new DynamicProxy();
            HttpClientHandler handler = new HttpClientHandler
            {
                UseCookies = false,//Cookies直接由Request指定，不使用自带的Cookies系统。
                UseDefaultCredentials = false,//不使用默认身份认证，避免对代理的使用造成影响。
                UseProxy = true,//使用DynamicProxy作为代理，具体使用代理还是直连由DynamicProxy决定。
                Proxy = proxy,
                AllowAutoRedirect = AllowAutoRedirect//同步自动跳转设置
            };
            return new Tuple<HttpMessageHandler, DynamicProxy>(handler, proxy);
        }
        #endregion

        #region 构造请求
        /// <summary>
        /// 根据<see cref="Request"/>生成<see cref="HttpRequestMessage"/>。
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <returns>HTTP请求消息</returns>
        private static HttpRequestMessage GenerateHttpRequestMessage(Request request)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(request.Method, request.Url);
            // Headers 的优先级低于 Request.UserAgent 这种特定设置, 因此先加载所有 Headers, 再使用 Request.UserAgent 覆盖
            foreach (var header in request.Headers)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            SetCookies(httpRequestMessage.Headers, request);
            SetContent(httpRequestMessage, request);
            return httpRequestMessage;
        }

        /// <summary>
        /// 添加CONTENT，可以为空字符串。
        /// 理论上所有类型的请求都可以有CONTENT，但大部分人只用在POST/PUT时使用。
        /// </summary>
        /// <param name="httpRequestMessage">HTTP请求消息</param>
        /// <param name="request">HTTP请求</param>
        private static void SetContent(HttpRequestMessage httpRequestMessage, Request request)
        {
            if (request.ContentData == null)
            {
                return;
            }

            httpRequestMessage.Content = new ByteArrayContent(request.GetCompressedContentData());
            SetHeader(httpRequestMessage.Content.Headers, "Content-Type", request.ContentType);
            SetHeader(httpRequestMessage.Content.Headers, "X-Requested-With", request.XRequestedWith);
        }

        /// <summary>
        /// 设置HTTP请求的Cookies
        /// </summary>
        /// <param name="headers">HTTP请求头集合</param>
        /// <param name="request">HTTP请求</param>
        private static void SetCookies(HttpRequestHeaders headers, Request request)
        {
            if (request.Cookies.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                bool first = true;
                foreach (var i in request.Cookies)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append("; ");
                    }

                    builder.Append(i.Key);
                    builder.Append(":");
                    builder.Append(i.Value);
                }

                SetHeader(headers, "Cookie", builder.ToString());
            }
        }

        /// <summary>
        /// 设置HTTP头。
        /// </summary>
        /// <param name="headers">HTTP头集合</param>
        /// <param name="key">头的索引</param>
        /// <param name="value">头的值</param>
        private static void SetHeader(HttpHeaders headers, string key, string value)
        {
            if (value is null)
            {
                return;
            }

            headers.Remove(key);
            headers.TryAddWithoutValidation(key, value);
        }
        #endregion

        #region 解析响应
        /// <summary>
        /// 追加HTTP响应头
        /// </summary>
        /// <param name="headers">目的集合</param>
        /// <param name="contentHeaders">来源</param>
        private static void AppendResponseHeaders(Dictionary<string, HashSet<string>> headers, HttpHeaders contentHeaders)
        {
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

        /// <summary>
        /// 增加HTTP响应头中的"Set-Cookie"字段。
        /// </summary>
        /// <param name="cookies">Set-Cookie"字段集合</param>
        /// <param name="header">HTTP响应头</param>
        private static void AppendSetCookies(List<Dictionary<string, string>> cookies, Dictionary<string, HashSet<string>> header)
        {
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
        }

        /// <summary>
        /// 判断响应是否为文本类型。
        /// </summary>
        /// <param name="contentType"><see cref="Response.ContentType"/></param>
        /// <returns>是否为文本类型</returns>
        private bool IsTextResponse(string contentType)
        {
            bool isText = false;
            if (contentType == string.Empty)
            {
                isText = true;
            }
            else
            {
                string[] contentTypes = contentType.Split(';');
                foreach (var i in contentTypes)
                {
                    if (ExcludeMediaTypes.Contains(i.Trim()))
                    {
                        isText = true;
                        break;
                    }
                }
            }

            return isText;
        }

        /// <summary>
        /// 生成<see cref="Response"/>。
        /// </summary>
        /// <param name="response">目的响应</param>
        /// <param name="httpResponse">来源响应</param>
        /// <returns></returns>
        private async Task GenerateResponse(Response response, HttpResponseMessage httpResponse)
        {
            response.StatusCode = httpResponse.StatusCode;
            AppendResponseHeaders(response.Headers, httpResponse.Headers);
            AppendSetCookies(response.SetCookies, response.Headers);
            response.TargetUrl = httpResponse.RequestMessage.RequestUri.AbsoluteUri;
            if (httpResponse.Content != null)
            {
                AppendResponseHeaders(response.Headers, httpResponse.Content.Headers);
                response.ContentType = httpResponse.Content.Headers.ContentType?.ToString() ?? string.Empty;
                if (IsTextResponse(response.ContentType))
                {
                    response.Content = await httpResponse.Content.ReadAsStringAsync();
                }
                else
                {
                    response.Content = await httpResponse.Content.ReadAsByteArrayAsync();
                }
            }
            else
            {
                response.ContentType = string.Empty;
            }
        }
        #endregion
    }
}
