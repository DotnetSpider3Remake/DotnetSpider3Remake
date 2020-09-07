using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes.Stubs;

namespace DotnetSpider.Runner.Tests
{
    [TestClass()]
    public class BaseZeroDisposableTests
    {
        [TestMethod()]
        [Timeout(5000)]
        public void DisposeTest()
        {
            using Fakes.StubBaseZeroDisposable instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true
            };
            PrivateObject privateInstance = new PrivateObject(instance);
            privateInstance.Invoke("Enter");
            var work = Task.Run(instance.Dispose);
            var delay = Task.Delay(100);
            Assert.AreEqual(1, Task.WaitAny(work, delay));
            Assert.IsTrue(delay.IsCompleted);
            Assert.IsFalse(work.IsCompleted);
            privateInstance.Invoke("Leave");
            work.Wait();
        }

        [TestMethod()]
        [Timeout(5000)]
        public void DisposeOthersTest0()
        {
            Fakes.StubBaseZeroDisposable instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true
            };
            instance.Dispose();
        }

        [TestMethod()]
        [Timeout(5000)]
        public void DisposeOthersTest1()
        {
            int called = 0;
            Fakes.StubBaseZeroDisposable instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true,
                DisposeOthers01 = () => ++called
            };
            instance.Dispose();
            Assert.AreEqual(1, called, "BaseZeroDisposable.DisposeOthers has never been called.");
            instance.Dispose();
            instance.Dispose();
            Assert.AreEqual(1, called, "BaseZeroDisposable.DisposeOthers would been called only once.");
        }

        [TestMethod()]
        [Timeout(5000)]
        public void WaitFinishedTest0()
        {
            using Fakes.StubBaseZeroDisposable instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true
            };
            PrivateObject privateInstance = new PrivateObject(instance);
            privateInstance.Invoke("WaitFinished");
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task WaitForDisposeTest0()
        {
            using Fakes.StubBaseZeroDisposable instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true
            };
            PrivateObject privateInstance = new PrivateObject(instance);
            bool cancelled = false;
            object isCanclledLocker = new object();
            Func<bool> isCancelled = () =>
            {
                lock (isCanclledLocker)
                {
                    return cancelled;
                }
            };
            Task wait = (Task)privateInstance.Invoke("WaitForDispose", isCancelled);
            Assert.IsFalse(wait.IsCompleted);
            await Task.Delay(10);
            Assert.IsFalse(wait.IsCompleted);
            instance.Dispose();
            await wait;
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task WaitForDisposeTest1()
        {
            using Fakes.StubBaseZeroDisposable instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true
            };
            PrivateObject privateInstance = new PrivateObject(instance);
            Task wait = (Task)privateInstance.Invoke("WaitForDispose", (Func<bool>)null);
            Assert.IsFalse(wait.IsCompleted);
            await Task.Delay(10);
            Assert.IsFalse(wait.IsCompleted);
            instance.Dispose();
            await wait;
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task WaitForDisposeTest2()
        {
            using Fakes.StubBaseZeroDisposable instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true
            };
            PrivateObject privateInstance = new PrivateObject(instance);
            bool cancelled = false;
            object isCanclledLocker = new object();
            Func<bool> isCancelled = () =>
            {
                lock (isCanclledLocker)
                {
                    return cancelled;
                }
            };
            Task wait = (Task)privateInstance.Invoke("WaitForDispose", isCancelled);
            Assert.IsFalse(wait.IsCompleted);
            await Task.Delay(10);
            Assert.IsFalse(wait.IsCompleted);
            lock (isCanclledLocker)
            {
                cancelled = true;
            }

            await wait;
        }

        [TestMethod()]
        [Timeout(5000)]
        public void WaitFinishedTest1()
        {
            using Fakes.StubBaseZeroDisposable instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true
            };
            PrivateObject privateInstance = new PrivateObject(instance);
            privateInstance.Invoke("Enter");
            var work = Task.Run(() => privateInstance.Invoke("WaitFinished"));
            var delay = Task.Delay(100);
            Assert.AreEqual(1, Task.WaitAny(work, delay));
            Assert.IsTrue(delay.IsCompleted);
            Assert.IsFalse(work.IsCompleted);
            privateInstance.Invoke("Leave");
            work.Wait();
        }

        [TestMethod()]
        [Timeout(5000)]
        public void IsDisposedTest()
        {
            Fakes.StubBaseZeroDisposable instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true
            };
            PrivateObject privateInstance = new PrivateObject(instance);
            Assert.IsFalse((bool)privateInstance.GetProperty("IsDisposed"));
            instance.Dispose();
            Assert.IsTrue((bool)privateInstance.GetProperty("IsDisposed"));
        }

        [TestMethod()]
        [Timeout(5000)]
        public void MaxDisposeWaitTest()
        {
            using Fakes.StubBaseZeroDisposable instance = new Fakes.StubBaseZeroDisposable
            {
                CallBase = true
            };
            PrivateObject privateInstance = new PrivateObject(instance);
            privateInstance.SetProperty("MaxDisposeWait", TimeSpan.FromMilliseconds(100));
            privateInstance.Invoke("Enter");
            var work = Task.Run(instance.Dispose);
            var delay = Task.Delay(50);
            Assert.AreEqual(1, Task.WaitAny(work, delay));
            Assert.IsTrue(delay.IsCompleted);
            Assert.IsFalse(work.IsCompleted);
            work.Wait();
        }
    }
}