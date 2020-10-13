using DotnetSpider.Downloader.Fakes;
using DotnetSpider.Proxy.Helper;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class HttpClientDownloaderTests
    {
        #region 全局设定
        private IDisposable _shimsContext = null;
        private StubHttpClientDownloader _instance = null;
        private ShimHttpClientDownloader _instanceShim = null;
        private PrivateObject _private = null;
        private PrivateType _type = null;

        [TestInitialize]
        public void Init()
        {
            _shimsContext = ShimsContext.Create();
            _instance = new StubHttpClientDownloader
            {
                CallBase = true
            };
            _instanceShim = new ShimHttpClientDownloader(_instance);
            _type = new PrivateType(typeof(HttpClientDownloader));
            _private = new PrivateObject(_instance, _type);
        }

        [TestCleanup]
        public void Clean()
        {
            _type = null;
            _private = null;
            _instanceShim = null;
            _instance.Dispose();
            _instance = null;
            _shimsContext.Dispose();
            _shimsContext = null;
        }
        #endregion

        #region 可以在派生类中重新实现的函数的测试
        [TestMethod]
        public void HttpClientGetterTest()
        {
            Assert.IsNotNull(_instance.HttpClientGetter);
            _instance.HttpClientGetter = null;
            Assert.IsNull(_instance.HttpClientGetter);
        }

        [TestMethod]
        public void CreateHttpMessageHandlerTest0()
        {
            var result = (Tuple<HttpMessageHandler, DynamicProxy>)_private.Invoke("CreateHttpMessageHandler");
            Assert.IsInstanceOfType(result.Item1, typeof(HttpClientHandler));
            HttpClientHandler handler = (HttpClientHandler)result.Item1;
            Assert.IsTrue(handler.UseProxy);
            Assert.IsFalse(handler.UseDefaultCredentials);
            Assert.AreSame(result.Item2, handler.Proxy);
        }

        [TestMethod]
        public void CreateHttpMessageHandlerTest1()
        {
            _instance.AllowAutoRedirect = false;
            HttpClientHandler handler = (HttpClientHandler)((Tuple<HttpMessageHandler, DynamicProxy>)_private.Invoke("CreateHttpMessageHandler")).Item1;
            Assert.IsFalse(handler.AllowAutoRedirect);

            _instance.AllowAutoRedirect = true;
            handler = (HttpClientHandler)((Tuple<HttpMessageHandler, DynamicProxy>)_private.Invoke("CreateHttpMessageHandler")).Item1;
            Assert.IsTrue(handler.AllowAutoRedirect);
        }
        #endregion

        #region 构造请求的测试
        [TestMethod]
        public void GetHttpClientTest()
        {
            using HttpClient client = new HttpClient();
            _instance.HttpClientGetter = _ => client;
            bool callCreateHttpMessageHandler = false;
            _instanceShim.CreateHttpMessageHandler = () =>
            {
                callCreateHttpMessageHandler = true;
                return new Tuple<HttpMessageHandler, DynamicProxy>(null, new DynamicProxy());
            };

            HttpClient actual = (HttpClient)_private.Invoke("GetHttpClient", (Request)null, (IWebProxy)null);
            Assert.AreSame(client, actual);
            Assert.IsTrue(callCreateHttpMessageHandler);

            callCreateHttpMessageHandler = false;
            _private.Invoke("GetHttpClient", (Request)null, (IWebProxy)null);
            Assert.IsFalse(callCreateHttpMessageHandler);
        }

        [TestMethod]
        public void GenerateHttpRequestMessageTest()
        {
            bool callSetCookies = false;
            ShimHttpClientDownloader.SetCookiesHttpRequestHeadersRequest = (_1, _2) => callSetCookies = true;
            bool callSetContent = false;
            ShimHttpClientDownloader.SetContentHttpRequestMessageRequest = (_1, _2) => callSetContent = true;

            Request request = new Request("http://baidu.com");
            request.Headers["1"] = "1";
            request.Headers["2"] = "2";
            request.Headers["3"] = null;

            var message = (HttpRequestMessage)_type.InvokeStatic("GenerateHttpRequestMessage", request);
            Assert.IsTrue(callSetCookies);
            Assert.IsTrue(callSetContent);
            Assert.IsNotNull(message);
            Assert.AreEqual(request.Url, message.RequestUri.OriginalString);
            Assert.AreEqual(request.Method, message.Method);

            var actual = new Dictionary<string, object>();
            foreach (var i in message.Headers)
            {
                actual[i.Key] = i.Value.Single();
            }

            Assert.AreEqual(request.Headers.Count, actual.Count);
            CollectionAssert.AreEquivalent(request.Headers.Keys, actual.Keys);
            Assert.AreEqual("1", actual["1"]);
            Assert.AreEqual("2", actual["2"]);
            Assert.AreEqual("", actual["3"]);
        }

        [TestMethod]
        public void SetContentTest0()
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
            Request request = new Request();
            _type.InvokeStatic("SetContent", httpRequestMessage, request);

            Assert.IsNull(httpRequestMessage.Content);
        }

        [TestMethod]
        public async Task SetContentTest1()
        {
            byte[] data = new byte[2] { 0, 1 };
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
            Request request = new Request 
            {
                ContentData = data
            };
            ShimHttpClientDownloader.CompressContentRequest = _ => data;
            ShimHttpClientDownloader.SetHeaderHttpHeadersStringString = (_1, _2, _3) => { };
            _type.InvokeStatic("SetContent", httpRequestMessage, request);

            Assert.IsInstanceOfType(httpRequestMessage.Content, typeof(ByteArrayContent));
            CollectionAssert.AreEqual(data, await httpRequestMessage.Content.ReadAsByteArrayAsync());
            Assert.IsFalse(httpRequestMessage.Content.Headers.Contains("X-Requested-With"));
        }

        [TestMethod]
        public async Task SetContentTest2()
        {
            byte[] data = new byte[2] { 0, 1 };
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");
            Request request = new Request
            {
                ContentData = data,
                ContentType = "application/json",
                Headers = { { "X-Requested-With", "XMLHttpRequest" } }
            };
            ShimHttpClientDownloader.CompressContentRequest = _ => data;
            _type.InvokeStatic("SetContent", httpRequestMessage, request);

            Assert.IsInstanceOfType(httpRequestMessage.Content, typeof(ByteArrayContent));
            CollectionAssert.AreEqual(data, await httpRequestMessage.Content.ReadAsByteArrayAsync());

            Assert.IsTrue(httpRequestMessage.Content.Headers.Contains("X-Requested-With"));
            Assert.AreEqual("XMLHttpRequest", httpRequestMessage.Content.Headers.GetValues("X-Requested-With").Single());
        }

        [TestMethod]
        public void SetCookiesTest0()
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
            Request request = new Request();
            _type.InvokeStatic("SetCookies", httpRequestMessage.Headers, request);

            Assert.IsFalse(httpRequestMessage.Headers.Contains("Cookie"));
        }

        [TestMethod]
        public void SetCookiesTest1()
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
            Request request = new Request();
            request.Cookies["1"] = "a";
            request.Cookies["2"] = "b";
            _type.InvokeStatic("SetCookies", httpRequestMessage.Headers, request);

            Assert.IsTrue(httpRequestMessage.Headers.TryGetValues("Cookie", out IEnumerable<string> values));
            Assert.AreEqual("1:a; 2:b", values.Single());
        }

        [TestMethod]
        public void SetHeaderTest0()
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
            _type.InvokeStatic("SetHeader", httpRequestMessage.Headers, "1", null);
            Assert.IsFalse(httpRequestMessage.Headers.Contains("1"));
        }

        [TestMethod]
        public void SetHeaderTest1()
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
            _type.InvokeStatic("SetHeader", httpRequestMessage.Headers, "1", "a");
            Assert.IsTrue(httpRequestMessage.Headers.TryGetValues("1", out IEnumerable<string> values));
            Assert.AreEqual("a", values.Single());
        }

        [TestMethod]
        public void CompressContentTest0()
        {
            byte[] expected = new byte[] { 7, 8 };
            Request request = new Request
            {
                CompressMode = CompressMode.None,
                ContentData = expected
            };
            var actual = (byte[])_type.InvokeStatic("CompressContent", request);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CompressContentTest1()
        {
            byte[] expected = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 99, 231, 0, 0, 10, 12, 67, 0, 2, 0, 0, 0 };
            Request request = new Request
            {
                CompressMode = CompressMode.Gzip,
                ContentData = new byte[] { 7, 8 }
            };
            var actual = (byte[])_type.InvokeStatic("CompressContent", request);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CompressContentTest2()
        {
            byte[] expected = new byte[] { 32, 7, 8 };
            Request request = new Request
            {
                CompressMode = CompressMode.Lz4,
                ContentData = new byte[] { 7, 8 }
            };
            var actual = (byte[])_type.InvokeStatic("CompressContent", request);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CompressContentTest4()
        {
            byte[] expected = new byte[] { 99, 231, 0, 0 };
            Request request = new Request
            {
                CompressMode = CompressMode.Deflate,
                ContentData = new byte[] { 7, 8 }
            };
            var actual = (byte[])_type.InvokeStatic("CompressContent", request);
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region 解析响应中的测试
        [TestMethod]
        public void AppendResponseHeadersTest()
        {
            using HttpResponseMessage message = new HttpResponseMessage();
            message.Headers.TryAddWithoutValidation("1", "a");
            message.Headers.TryAddWithoutValidation("2", "a");

            Dictionary<string, HashSet<string>> headers = new Dictionary<string, HashSet<string>>
            {
                { "1", new HashSet<string> { "b", "c" } }
            };

            _type.InvokeStatic("AppendResponseHeaders", headers, message.Headers);
            Assert.IsTrue(headers.ContainsKey("1"));
            CollectionAssert.AreEquivalent(new string[] { "a", "b", "c" }, headers["1"].ToArray());
            Assert.IsTrue(headers.ContainsKey("2"));
            CollectionAssert.AreEquivalent(new string[] { "a" }, headers["2"].ToArray());
        }

        [TestMethod]
        public void AppendSetCookiesTest0()
        {
            List<Dictionary<string, string>> cookies = new List<Dictionary<string, string>>();
            Dictionary<string, HashSet<string>> headers = new Dictionary<string, HashSet<string>>();

            _type.InvokeStatic("AppendSetCookies", cookies, headers);
            Assert.AreEqual(0, cookies.Count);
        }

        [TestMethod]
        public void AppendSetCookiesTest1()
        {
            List<Dictionary<string, string>> cookies = new List<Dictionary<string, string>>();
            Dictionary<string, HashSet<string>> headers = new Dictionary<string, HashSet<string>>
            {
                { 
                    "Set-Cookie",
                    new HashSet<string> 
                    {
                        "",
                        "1",
                        "1:a"
                    } 
                }
            };

            _type.InvokeStatic("AppendSetCookies", cookies, headers);
            Assert.AreEqual(1, cookies.Count);
            CollectionAssert.AreEquivalent(new Dictionary<string, string> { { "1", "a" } }, cookies[0]);
        }

        #endregion
    }
}
