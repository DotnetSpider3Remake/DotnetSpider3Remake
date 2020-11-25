using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes;

namespace DotnetSpider.Tests
{
    [TestClass()]
    public class BaseSpiderTests
    {
        #region 全局设定
        private IDisposable _shimsContext = null;
        private Fakes.StubBaseSpider _instance = null;
        private Fakes.ShimBaseSpider _instanceShim = null;
        private PrivateObject _private = null;
        private PrivateType _type = null;

        [TestInitialize]
        public void Init()
        {
            _shimsContext = ShimsContext.Create();
            _instance = new Fakes.StubBaseSpider
            {
                CallBase = true
            };
            _instanceShim = new Fakes.ShimBaseSpider(_instance);
            _type = new PrivateType(typeof(BaseSpider));
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

        #region 实现接口的测试
        [TestMethod()]
        public void ContinueTest0()
        {
            _private.SetField("_hasExit", true);
            _private.SetField("_isRunning", false);
            Assert.IsFalse(_instance.Continue());
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public void ContinueTest1()
        {
            _private.SetField("_hasExit", false);
            _private.SetField("_isRunning", true);
            Assert.IsFalse(_instance.Continue());
            Assert.IsTrue((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public void ContinueTest2()
        {
            _private.SetField("_hasExit", false);
            _private.SetField("_isRunning", false);
            Assert.IsTrue(_instance.Continue());
            Assert.IsTrue((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public async Task ContinueAsyncTest0()
        {
            _private.SetField("_hasExit", true);
            _private.SetField("_isRunning", false);
            Assert.IsFalse(await _instance.ContinueAsync());
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public async Task ContinueAsyncTest1()
        {
            _private.SetField("_hasExit", false);
            _private.SetField("_isRunning", true);
            Assert.IsFalse(await _instance.ContinueAsync());
            Assert.IsTrue((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public async Task ContinueAsyncTest2()
        {
            _private.SetField("_hasExit", false);
            _private.SetField("_isRunning", false);
            Assert.IsTrue(await _instance.ContinueAsync());
            Assert.IsTrue((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ExitTest0()
        {
            _private.SetField("_hasExit", true);
            _instance.Scheduler = new Scheduler.Fakes.StubIScheduler()
            {
                Clear = Assert.Fail
            };
            Assert.IsFalse(_instance.Exit());
            Assert.IsTrue((bool)_private.GetField("_hasExit"));
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ExitTest1()
        {
            _private.SetField("_hasExit", false);
            int called = 0;
            _instance.Scheduler = new Scheduler.Fakes.StubIScheduler()
            {
                Clear = () => ++called
            };
            Assert.IsTrue(_instance.Exit());
            Assert.AreEqual(1, called);
            Assert.IsTrue((bool)_private.GetField("_hasExit"));
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ExitTest2()
        {
            _private.SetField("_hasExit", false);
            Assert.IsTrue(_instance.Exit());
            Assert.IsTrue((bool)_private.GetField("_hasExit"));
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ExitAsyncTest0()
        {
            _private.SetField("_hasExit", true);
            _instance.Scheduler = new Scheduler.Fakes.StubIScheduler()
            {
                Clear = Assert.Fail
            };
            Assert.IsFalse(await _instance.ExitAsync());
            Assert.IsTrue((bool)_private.GetField("_hasExit"));
        }

        [TestMethod()]
        public void PauseTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PauseAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RunTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RunAsyncTest()
        {
            Assert.Fail();
        }
        #endregion
    }
}