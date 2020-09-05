﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Downloader.Tests
{
    [TestClass()]
    public class RequestTests
    {
        #region 构造函数

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

        #endregion

        #region Equals

        [TestMethod()]
        public void EqualsTest0()
        {
            var a = new Request();
            var b = new Request();
            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod()]
        public void EqualsTest1()
        {
            var a = new Request();
            var b = new Request("test");
            Assert.IsFalse(a.Equals(b));
        }

        [TestMethod()]
        public void EqualsTest2()
        {
            var a = new Request();
            var b = new Request();
            b.Properties.Add("test", null);
            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod()]
        public void EqualsTest3()
        {
            var a = new Request();
            var b = new Request();
            b.Headers.Add("test", null);
            Assert.IsFalse(a.Equals(b));
        }

        [TestMethod()]
        public void EqualsTest4()
        {
            var a = new Request();
            Assert.IsFalse(a.Equals(null));
        }

        [TestMethod()]
        public void EqualsTest5()
        {
            var a = new Request();
            Assert.IsFalse(a.Equals(15));
        }

        #endregion

        #region GetHashCode

        [TestMethod()]
        public void GetHashCodeTest0()
        {
            var a = new Request();
            var b = new Request();
            a.Url = "test";
            b.Origin = "test";
            Assert.IsTrue(a.GetHashCode() != b.GetHashCode());
        }

        [TestMethod()]
        public void GetHashCodeTest1()
        {
            var a = new Request();
            var b = new Request();
            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
        }

        [TestMethod()]
        public void GetHashCodeTest2()
        {
            var a = new Request("test");
            a.Headers.Add("1", 1);
            var b = new Request("test");
            b.Headers.Add("1", 1);
            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
        }

        [TestMethod()]
        public void GetHashCodeTest3()
        {
            var a = new Request("test");
            var b = new Request("test");
            b.Properties.Add("1", 1);
            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
        }

        #endregion

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