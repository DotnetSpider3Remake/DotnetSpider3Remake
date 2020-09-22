using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes;
using System.Reflection;

namespace DotnetSpider.Pipeline.Tests
{
    [TestClass()]
    public class BaseCachePipelineTests
    {
        private IDisposable _shimsContext = null;
        Fakes.StubBaseCachePipeline _instance = null;
        PrivateObject _private = null;

        [TestInitialize]
        public void Init()
        {
            _shimsContext = ShimsContext.Create();
            _instance = new Fakes.StubBaseCachePipeline
            {
                CallBase = true
            };
            _private = new PrivateObject(_instance, new PrivateType(typeof(BaseCachePipeline)));
        }

        [TestCleanup]
        public void Clean()
        {
            _private = null;
            _instance.Dispose();
            _instance = null;
            _shimsContext.Dispose();
            _shimsContext = null;
        }

        [TestMethod()]
        public void PropertyTest0()
        {
            log4net.Fakes.StubILog log = new log4net.Fakes.StubILog();
            _instance.Logger = log;
            Assert.AreSame(log, _instance.Logger);
            _instance.Name = "name";
            Assert.AreEqual("name", _instance.Name);
        }

        [TestMethod()]
        public void PropertyTest1()
        {
            Assert.IsTrue(_instance.IsCacheSupported);
            Assert.IsTrue(_instance.EnableCache);
            _instance.EnableCache = false;
            Assert.IsFalse(_instance.EnableCache);
            Assert.AreEqual(TimeSpan.FromSeconds(1), _instance.MaxCacheTime);
            _instance.MaxCacheTime = TimeSpan.FromSeconds(2);
            Assert.AreEqual(TimeSpan.FromSeconds(2), _instance.MaxCacheTime);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task WaitForNextProcess0()
        {
            _instance.MaxCacheTime = TimeSpan.FromMilliseconds(10);
            Task wait = (Task)_private.Invoke("WaitForNextProcess", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsFalse(wait.IsCompleted);
            _instance.Dispose();
            await wait;
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task WaitForNextProcess1()
        {
            _instance.MaxCacheTime = TimeSpan.FromMilliseconds(10);
            Task wait = (Task)_private.Invoke("WaitForNextProcess");
            Assert.IsFalse(wait.IsCompleted);
            await Task.Delay(200);
            Assert.IsTrue(wait.IsCompleted);
        }
    }
}