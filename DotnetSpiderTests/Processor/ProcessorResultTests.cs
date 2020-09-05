using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Processor;
using System;
using System.Collections.Generic;
using System.Text;
using DotnetSpider.Downloader;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace DotnetSpider.Processor.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ProcessorResultTests
    {
        #region 构造函数
        [TestMethod()]
        public void ProcessorResultTest0()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new ProcessorResult(new Request());
            });
        }

        [TestMethod()]
        public void ProcessorResultTest1()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new ProcessorResult(new Request("1.1.1.1"));
            });
        }

        [TestMethod()]
        public void ProcessorResultTest2()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new ProcessorResult(new Request("ftp://1.1.1.1"));
            });
        }

        [TestMethod()]
        public void ProcessorResultTest3()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                new ProcessorResult(null);
            });
        }

        [TestMethod()]
        public void ProcessorResultTest4()
        {
            var r = new Request("http://www.baidu.com");
            var p = new ProcessorResult(r);
            Assert.AreEqual(r, p.Request);
        }

        #endregion

        #region AddTargetRequests

        [TestMethod()]
        public void AddTargetRequestsTest0()
        {
            var p = new ProcessorResult(new Request("http://www.baidu.com"));
            p.AddTargetRequests(new List<string>());
            Assert.AreEqual(0, p.TargetRequests.Count);
            p.AddTargetRequests((List<string>)null);
            Assert.AreEqual(0, p.TargetRequests.Count);
        }

        [TestMethod()]
        public void AddTargetRequestsTest1()
        {
            var p = new ProcessorResult(new Request("http://www.baidu.com"));
            p.AddTargetRequests(new List<Request>());
            Assert.AreEqual(0, p.TargetRequests.Count);
            p.AddTargetRequests((List<Request>)null);
            Assert.AreEqual(0, p.TargetRequests.Count);
        }

        [TestMethod()]
        public void AddTargetRequestsTest2()
        {
            var srcR = new Request("http://www.baidu.com")
            {
                Headers = { { "1", "2" } }
            };
            var p = new ProcessorResult(srcR);

            p.AddTargetRequests(new string[] { "/test" });
            Assert.AreEqual(1, p.TargetRequests.Count);
            var r = p.TargetRequests.First();
            Assert.AreEqual("http://www.baidu.com/test", r.Url);
            CollectionAssert.AreEquivalent(srcR.Headers, r.Headers);
        }

        [TestMethod()]
        public void AddTargetRequestsTest3()
        {
            var srcR = new Request("http://www.baidu.com")
            {
                Headers = { { "1", "2" } }
            };
            var p = new ProcessorResult(srcR);

            p.AddTargetRequests(new Request[] { new Request("http://www.baidu.com/test") });
            Assert.AreEqual(1, p.TargetRequests.Count);
            var r = p.TargetRequests.First();
            CollectionAssert.AreEquivalent(srcR.Headers, r.Headers);
        }

        #endregion

        #region AddTargetRequest

        [TestMethod()]
        public void AddTargetRequestTest0()
        {
            var p = new ProcessorResult(new Request("http://www.baidu.com"));
            p.AddTargetRequest((Request)null);
            Assert.AreEqual(0, p.TargetRequests.Count);
            p.AddTargetRequest("");
            Assert.AreEqual(0, p.TargetRequests.Count);
            p.AddTargetRequest(new Request());
            Assert.AreEqual(0, p.TargetRequests.Count);
        }

        [TestMethod()]
        public void AddTargetRequestTest1()
        {
            var srcR = new Request("http://www.baidu.com")
            {
                Headers = { { "1", "2" } }
            };
            var p = new ProcessorResult(srcR);

            p.AddTargetRequest("/test");
            Assert.AreEqual(1, p.TargetRequests.Count);
            var r = p.TargetRequests.Last();
            Assert.AreEqual("http://www.baidu.com/test", r.Url);
            CollectionAssert.AreEquivalent(srcR.Headers, r.Headers);
            p.AddTargetRequest("http:///news.bing.com/");
            Assert.AreEqual(1, p.TargetRequests.Count);
        }

        [TestMethod()]
        public void AddTargetRequestTest2()
        {
            var p = new ProcessorResult(new Request("http://www.baidu.com")
            {
                Headers = { { "1", "2" } }
            });
            p.AddTargetRequest(new Request("http://news.baidu.com"), false);
            Assert.AreEqual(1, p.TargetRequests.Count);
            CollectionAssert.AreNotEquivalent(p.Request.Headers, p.TargetRequests.First().Headers);
        }

        #endregion

        [TestMethod()]
        public void ByPassTest()
        {
            var p = new ProcessorResult(new Request("http://www.baidu.com"));
            Assert.IsFalse(p.Bypass);
            p.Bypass = true;
            Assert.IsTrue(p.Bypass);
        }

        [TestMethod()]
        public void RetryTest()
        {
            var p = new ProcessorResult(new Request("http://www.baidu.com"));
            Assert.IsFalse(p.Retry);
            p.Retry = true;
            Assert.IsTrue(p.Retry);
        }

        [TestMethod()]
        public void SkipTargetRequestsTest()
        {
            var p = new ProcessorResult(new Request("http://www.baidu.com"));
            Assert.IsFalse(p.SkipTargetRequests);
            p.SkipTargetRequests = true;
            Assert.IsTrue(p.SkipTargetRequests);
        }

        [TestMethod()]
        public void ResultItemsTest()
        {
            var p = new ProcessorResult(new Request("http://www.baidu.com"));
            Assert.IsNotNull(p.ResultItems);
            Assert.AreEqual(0, p.ResultItems.Count);
        }
    }
}