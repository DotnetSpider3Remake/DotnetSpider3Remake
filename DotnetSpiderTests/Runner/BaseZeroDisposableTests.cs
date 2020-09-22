using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes.Stubs;
using System.Threading;
using Microsoft.QualityTools.Testing.Fakes;

namespace DotnetSpider.Runner.Tests
{
    [TestClass()]
    public class BaseZeroDisposableTests
    {
        private IDisposable _shimsContext = null;
        Fakes.StubBaseZeroDisposable _instance = null;
        PrivateObject _private = null;

        [TestInitialize]
        public void Init()
        {
            _shimsContext = ShimsContext.Create();
            _instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true
            };
            _private = new PrivateObject(_instance, new PrivateType(typeof(BaseZeroDisposable)));
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
        [Timeout(5000)]
        public void DisposeTest()
        {
            _private.Invoke("Enter");
            var work = Task.Run(_instance.Dispose);
            var delay = Task.Delay(100);
            Assert.AreEqual(1, Task.WaitAny(work, delay));
            Assert.IsTrue(delay.IsCompleted);
            Assert.IsFalse(work.IsCompleted);
            _private.Invoke("Leave");
            work.Wait();
        }

        [TestMethod()]
        [Timeout(5000)]
        public void DisposeOthersTest0()
        {
            _instance.Dispose();
        }

        [TestMethod()]
        [Timeout(5000)]
        public void DisposeOthersTest1()
        {
            int called = 0;
            _instance.DisposeOthers01 = () => ++called;
            _instance.Dispose();
            Assert.AreEqual(1, called, "BaseZeroDisposable.DisposeOthers has never been called.");
            _instance.Dispose();
            _instance.Dispose();
            Assert.AreEqual(1, called, "BaseZeroDisposable.DisposeOthers would been called only once.");
        }

        [TestMethod()]
        [Timeout(5000)]
        public void WaitFinishedTest0()
        {
            _private.Invoke("WaitFinished");
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task WaitForDisposeTest0()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Task<bool> wait = (Task<bool>)_private.Invoke("WaitForDispose", tokenSource.Token);
            Assert.IsFalse(wait.IsCompleted);
            await Task.Delay(10);
            Assert.IsFalse(wait.IsCompleted);
            tokenSource.Cancel();
            Assert.IsFalse(await wait);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task WaitForDisposeTest1()
        {
            Task<bool> wait = (Task<bool>)_private.Invoke("WaitForDispose", (CancellationToken?)null);
            Assert.IsFalse(wait.IsCompleted);
            await Task.Delay(10);
            Assert.IsFalse(wait.IsCompleted);
            _instance.Dispose();
            Assert.IsTrue(await wait);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task WaitForDisposeTest2()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource(10);
            Task<bool> wait = (Task<bool>)_private.Invoke("WaitForDispose", tokenSource.Token);
            Assert.IsFalse(wait.IsCompleted);
            Assert.IsFalse(await wait);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void WaitFinishedTest1()
        {
            _private.Invoke("Enter");
            var work = Task.Run(() => _private.Invoke("WaitFinished"));
            var delay = Task.Delay(100);
            Assert.AreEqual(1, Task.WaitAny(work, delay));
            Assert.IsTrue(delay.IsCompleted);
            Assert.IsFalse(work.IsCompleted);
            _private.Invoke("Leave");
            work.Wait();
        }

        [TestMethod()]
        [Timeout(5000)]
        public void IsDisposedTest()
        {
            Assert.IsFalse((bool)_private.GetProperty("IsDisposed"));
            _instance.Dispose();
            Assert.IsTrue((bool)_private.GetProperty("IsDisposed"));
        }

        [TestMethod()]
        [Timeout(5000)]
        public void MaxDisposeWaitTest()
        {
            _private.SetProperty("MaxDisposeWait", TimeSpan.FromMilliseconds(100));
            _private.Invoke("Enter");
            var work = Task.Run(_instance.Dispose);
            var delay = Task.Delay(50);
            Assert.AreEqual(1, Task.WaitAny(work, delay));
            Assert.IsTrue(delay.IsCompleted);
            Assert.IsFalse(work.IsCompleted);
            work.Wait();
        }
    }
}