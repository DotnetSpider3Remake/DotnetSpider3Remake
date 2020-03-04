using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Downloader.Tests
{
    [TestClass()]
    public class RequestTests
    {
        [TestMethod()]
        public void RequestTest0()
        {
            Assert.AreEqual(new Request(null), new Request());
        }


        [TestMethod()]
        public void RequestTest1()
        {
            Assert.AreEqual("test", new Request("test").Url);
        }


        [TestMethod()]
        public void RequestTest2()
        {
            Assert.IsNotNull(new Request("test", null));
        }


        [TestMethod()]
        public void RequestTest3()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            Assert.AreSame(properties, new Request("test", properties).Properties);
        }

        [TestMethod()]
        public void EqualsTest0()
        {
            var a = new Request();
            var b = new Request();
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestMethod()]
        public void EqualsTest1()
        {
            var a = new Request();
            var b = new Request("test");
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [TestMethod()]
        public void EqualsTest2()
        {
            var a = new Request();
            var b = new Request();
            b.Properties.Add("test", null);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestMethod()]
        public void EqualsTest3()
        {
            var a = new Request();
            var b = new Request();
            b.Headers.Add("test", null);
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [TestMethod()]
        public void GetHashCodeTest()
        {
            var a = new Request();
            var b = new Request();
            a.Url = "test";
            b.Origin = "test";
            Assert.IsTrue(a.GetHashCode() != b.GetHashCode());
        }

        [TestMethod()]
        public void JsonStringTest()
        {
            var r = new Request("https://www.baidu.com");
            r.Origin = "https://www.google.com";
            r.Properties.Add("test", 0);
            string json = r.ToString();
            Assert.IsTrue(json.Contains("https://www.baidu.com"));
            Assert.IsTrue(json.Contains("https://www.google.com"));
            Assert.IsTrue(json.Contains("test"));
            Assert.IsTrue(json.Contains("0"));
            Assert.AreEqual(r, Request.FromString(json));
        }
    }
}