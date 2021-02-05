using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Downloader;
using System.Threading;

namespace DotnetSpider.Scheduler.Tests
{
    [TestClass()]
    public class ListSchedulerTests
    {
        [TestMethod()]
        [Timeout(5000)]
        public void PropertyTest()
        {
            using ListScheduler scheduler = new ListScheduler();
            log4net.Fakes.StubILog log = new log4net.Fakes.StubILog();
            scheduler.Logger = log;
            Assert.AreSame(log, scheduler.Logger);
            scheduler.Name = "name";
            Assert.AreEqual("name", scheduler.Name);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ClearTest()
        {
            using var s = new ListScheduler();
            s.Clear();
            Assert.AreEqual(0, s.Count);
            s.Push(new Request());
            s.Clear();
            Assert.AreEqual(0, s.Count);
        }

        #region Poll

        [TestMethod()]
        [Timeout(5000)]
        public void PollTest0()
        {
            using var s = new ListScheduler();
            Request r = new Request("test");
            s.Push(r);
            Assert.AreSame(r, s.Poll());
        }

        [TestMethod()]
        [Timeout(5000)]
        public void PollTest1()
        {
            using var s = new ListScheduler();
            Task.Run(async () =>
            {
                await Task.Delay(100);
                s.Dispose();
            });
            DateTime begin = DateTime.Now;
            Assert.AreEqual(null, s.Poll());
            Assert.IsTrue(DateTime.Now - begin >= TimeSpan.FromMilliseconds(50));
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PollTest2()
        {
            using var s = new ListScheduler();
            var poll = Task.Run(s.Poll);
            await Task.Delay(50);
            Assert.IsFalse(poll.IsCompleted);
            s.Dispose();
            Assert.IsNull(await poll);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PollTest3()
        {
            using var s = new ListScheduler();
            var poll = Task.Run(() => s.Poll(TimeSpan.FromMilliseconds(100)));
            await Task.Delay(50);
            Assert.IsFalse(poll.IsCompleted);
            Assert.IsNull(await poll);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PollTest4()
        {
            using var s = new ListScheduler();
            var poll = Task.Run(() => s.Poll(Timeout.InfiniteTimeSpan));
            await Task.Delay(50);
            Assert.IsFalse(poll.IsCompleted);
            s.Dispose();
            Assert.IsNull(await poll);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PollAsyncTest0()
        {
            using var s = new ListScheduler();
            Request r = new Request("test");
            s.Push(r);
            Assert.AreSame(r, await s.PollAsync());
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PollAsyncTest1()
        {
            using var s = new ListScheduler();
            DateTime begin = DateTime.Now;
            Task delay = Task.Delay(100);
            var poll = s.PollAsync();
            await delay;
            s.Dispose();
            await poll;
            Assert.AreEqual(null, poll.Result);
            Assert.IsTrue(DateTime.Now - begin >= TimeSpan.FromMilliseconds(50));
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PollAsyncTest2()
        {
            using var s = new ListScheduler();
            var poll = s.PollAsync();
            await Task.Delay(50);
            Assert.IsFalse(poll.IsCompleted);
        }

        #endregion

        #region Push

        [TestMethod()]
        [Timeout(5000)]
        public void PushTest0()
        {
            using var s = new ListScheduler();
            s.Push(new Request());
            Assert.AreEqual(1, s.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void PushTest1()
        {
            using var s = new ListScheduler();
            s.Push(new Request("test"));
            s.Push(new Request("test"));
            Assert.AreEqual(2, s.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void PushTest2()
        {
            using var s = new ListScheduler();
            s.Push(new Request());
            s.Push(new Request("test"));
            Assert.AreEqual(2, s.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void PushTest3()
        {
            using var s = new ListScheduler();
            Request[] requests = { new Request("a"), new Request("b"), new Request("c") };
            s.Push(requests);
            Assert.AreEqual(3, s.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void PushTest4()
        {
            using var s = new ListScheduler();
            Request[] requests = { new Request("a"), new Request("b"), new Request("a") };
            s.Push(requests);
            Assert.AreEqual(3, s.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PushAsyncTest0()
        {
            using var s = new ListScheduler();
            await s.PushAsync(new Request());
            Assert.AreEqual(1, s.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void PushAsyncTest1()
        {
            using var s = new ListScheduler();
            Task.WaitAll(s.PushAsync(new Request("test")),
                s.PushAsync(new Request("test")));
            Assert.AreEqual(2, s.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void PushAsyncTest2()
        {
            using var s = new ListScheduler();
            Task.WaitAll(s.PushAsync(new Request()),
                s.PushAsync(new Request("test")));
            Assert.AreEqual(2, s.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PushAsyncTest3()
        {
            using var s = new ListScheduler();
            Request[] requests = { new Request("a"), new Request("b"), new Request("c") };
            await s.PushAsync(requests);
            Assert.AreEqual(3, s.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PushAsyncTest4()
        {
            using var s = new ListScheduler();
            Request[] requests = { new Request("a"), new Request("b"), new Request("a") };
            await s.PushAsync(requests);
            Assert.AreEqual(3, s.Count);
        }

        #endregion

        #region PollAsync and PushAsync

        [TestMethod()]
        [Timeout(5000)]
        public async Task PollAsyncAndPushAsync0()
        {
            using var s = new ListScheduler();
            var r = new Request();
            var poll = s.PollAsync();
            _ = s.PushAsync(r);
            Assert.AreSame(r, await poll);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PollAsyncAndPushAsync1()
        {
            using var s = new ListScheduler();
            var r = new Request();
            var poll = s.PollAsync();
            await Task.Delay(50);
            _ = s.PushAsync(r);
            Assert.AreSame(r, await poll);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void PollAsyncAndPushAsync2()
        {
            using var s = new ListScheduler();
            var a = new Request("a");
            var b = new Request("b");
            var c = new Request("c");
            Task.WaitAll(s.PollAsync(),
                s.PushAsync(a),
                s.PushAsync(b),
                s.PushAsync(c));
            Assert.AreEqual(2, s.Count);
        }

        #endregion

        #region TraverseStrategy

        [TestMethod()]
        [Timeout(5000)]
        public void TraverseStrategyTest0()
        {
            using var s = new ListScheduler()
            {
                TraverseStrategy = TraverseStrategy.FIFO
            };
            var a = new Request("a");
            var b = new Request("b");
            var c = new Request("c");
            s.Push(a);
            s.Push(b);
            s.Push(c);
            Assert.AreSame(a, s.Poll());
            Assert.AreSame(b, s.Poll());
            Assert.AreSame(c, s.Poll());
        }

        [TestMethod()]
        [Timeout(5000)]
        public void TraverseStrategyTest1()
        {
            using var s = new ListScheduler()
            {
                TraverseStrategy = TraverseStrategy.FILO
            };
            var a = new Request("a");
            var b = new Request("b");
            var c = new Request("c");
            s.Push(a);
            s.Push(b);
            s.Push(c);
            Assert.AreSame(c, s.Poll());
            Assert.AreSame(b, s.Poll());
            Assert.AreSame(a, s.Poll());
        }

        #endregion
    }
}