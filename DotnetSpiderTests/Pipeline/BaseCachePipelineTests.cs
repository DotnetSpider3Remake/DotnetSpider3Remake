using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes;

namespace DotnetSpider.Pipeline.Tests
{
    [TestClass()]
    public class BaseCachePipelineTests
    {
        private IDisposable _shimsContext = null;
        Fakes.StubBaseCachePipeline _pipeline = null;

        [TestInitialize]
        public void Init()
        {
            _shimsContext = ShimsContext.Create();
            _pipeline = new Fakes.StubBaseCachePipeline
            {
                CallBase = true
            };
        }

        [TestCleanup]
        public void Clean()
        {
            _pipeline.Dispose();
            _pipeline = null;
            _shimsContext.Dispose();
            _shimsContext = null;
        }

        [TestMethod()]
        public void PropertyTest0()
        {
            log4net.Fakes.StubILog log = new log4net.Fakes.StubILog();
            _pipeline.Logger = log;
            Assert.AreSame(log, _pipeline.Logger);
            _pipeline.Name = "name";
            Assert.AreEqual("name", _pipeline.Name);
        }

        [TestMethod()]
        public void PropertyTest1()
        {
            Assert.IsTrue(_pipeline.IsCacheSupported);
            Assert.IsTrue(_pipeline.EnableCache);
            _pipeline.EnableCache = false;
            Assert.IsFalse(_pipeline.EnableCache);
            Assert.AreEqual(TimeSpan.FromSeconds(1), _pipeline.MaxCacheTime);
            _pipeline.MaxCacheTime = TimeSpan.FromSeconds(2);
            Assert.AreEqual(TimeSpan.FromSeconds(2), _pipeline.MaxCacheTime);
        }

        [TestMethod()]
        public void ProcessTest()
        {

        }

        [TestMethod()]
        public void ProcessTest1()
        {

        }
    }
}