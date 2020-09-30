using DotnetSpider.Common;
using DotnetSpider.Downloader.Fakes;
using DotnetSpider.Proxy.Helper;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.UI.WebControls.Adapters;

namespace DotnetSpider.Downloader.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class HttpClientDownloaderTest
    {
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
            bool callSetExtraHeaders = false;
            ShimHttpClientDownloader.SetExtraHeadersHttpRequestHeadersRequest = (_1, _2) => callSetExtraHeaders = true;
            bool callSetContent = false;
            ShimHttpClientDownloader.SetContentHttpRequestMessageRequest = (_1, _2) => callSetContent = true;

            Request request = new Request("http://baidu.com");
            request.Headers["1"] = "1";
            request.Headers["2"] = "2";
            request.Headers["3"] = null;

            var message = (HttpRequestMessage)_type.InvokeStatic("GenerateHttpRequestMessage", request);
            Assert.IsTrue(callSetExtraHeaders);
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
    }
}
