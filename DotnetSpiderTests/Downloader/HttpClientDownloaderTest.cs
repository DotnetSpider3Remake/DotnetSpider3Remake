using DotnetSpider.Downloader;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DotnetSpider.Downloader.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class HttpClientDownloaderTest
    {
        private IDisposable _shimsContext = null;
        private DotnetSpider.Downloader.Fakes.StubHttpClientDownloader _instance = null;
        private DotnetSpider.Downloader.Fakes.ShimHttpClientDownloader _instanceShim = null;
        private PrivateObject _private = null;

        [TestInitialize]
        public void Init()
        {
            _shimsContext = ShimsContext.Create();
            _instance = new DotnetSpider.Downloader.Fakes.StubHttpClientDownloader
            {
                CallBase = true
            };
            _instanceShim = new DotnetSpider.Downloader.Fakes.ShimHttpClientDownloader(_instance);
            _private = new PrivateObject(_instance, new PrivateType(typeof(HttpClientDownloader)));
        }

        [TestCleanup]
        public void Clean()
        {
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
    }
}
