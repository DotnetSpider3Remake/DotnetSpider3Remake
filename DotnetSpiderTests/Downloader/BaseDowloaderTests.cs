using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes;
using System.Diagnostics.CodeAnalysis;

namespace DotnetSpider.Downloader.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class BaseDowloaderTests
    {
        private IDisposable _shimsContext = null;
        private Fakes.StubBaseDowloader _instance = null;
        private Fakes.ShimBaseDowloader _instanceShim = null;
        private PrivateObject _private = null;

        [TestInitialize]
        public void Init()
        {
            _shimsContext = ShimsContext.Create();
            _instance = new Fakes.StubBaseDowloader
            {
                CallBase = true
            };
            _instanceShim = new Fakes.ShimBaseDowloader(_instance);
            _private = new PrivateObject(_instance, new PrivateType(typeof(BaseDowloader)));
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

        [TestMethod()]
        public void PropertyTest0()
        {
            Assert.IsNull(_instance.Logger);
            log4net.Fakes.StubILog log = new log4net.Fakes.StubILog();
            _instance.Logger = log;
            Assert.AreSame(log, _instance.Logger);
        }

        [TestMethod()]
        public void PropertyTest1()
        {
            Assert.IsNull(_instance.Name);
            _instance.Name = "name";
            Assert.AreEqual("name", _instance.Name);
        }

        [TestMethod()]
        public void PropertyTest2()
        {
            Assert.IsTrue(_instance.AllowAutoRedirect);
            _instance.AllowAutoRedirect = false;
            Assert.IsFalse(_instance.AllowAutoRedirect);
        }

        [TestMethod()]
        public void PropertyTest3()
        {
            Assert.AreEqual(TimeSpan.FromSeconds(8), _instance.Timeout);
            _instance.Timeout = TimeSpan.Zero;
            Assert.AreEqual(TimeSpan.Zero, _instance.Timeout);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task DownloadAsyncTest()
        {
            bool called = false;
            Response expected = new Response();
            _instance.DownloadingRequestIWebProxy = (_1, _2) =>
            {
                called = true;
                return Task.FromResult(expected);
            };

            Response response = await _instance.DownloadAsync(null);
            Assert.IsTrue(called);
            Assert.AreSame(expected, response);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void DownloadTest()
        {
            bool called = false;
            Response expected = new Response();
            _instance.DownloadingRequestIWebProxy = (_1, _2) =>
            {
                called = true;
                return Task.FromResult(expected);
            };

            Response response = _instance.Download(null);
            Assert.IsTrue(called);
            Assert.AreSame(expected, response);
        }
    }
}