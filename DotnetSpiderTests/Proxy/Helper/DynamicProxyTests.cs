using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Proxy.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace DotnetSpider.Proxy.Helper.Tests
{
    [TestClass()]
    public class DynamicProxyTests
    {
        [TestMethod()]
        public void GetProxyTest0()
        {
            DynamicProxy dynamicProxy = new DynamicProxy();
            Uri src = new Uri("http://127.0.0.1");
            Uri actual = dynamicProxy.GetProxy(src);
            Assert.AreSame(src, actual);
        }

        [TestMethod()]
        public void GetProxyTest1()
        {
            DynamicProxy dynamicProxy = new DynamicProxy
            {
                InnerProxy = new WebProxy("http://127.0.0.1:1080", false)
            };
            Uri src = new Uri("http://test.test");
            Uri actual = dynamicProxy.GetProxy(src);
            Assert.AreNotEqual(src, actual);
        }

        [TestMethod()]
        public void CredentialsTest0()
        {
            DynamicProxy dynamicProxy = new DynamicProxy();
            Assert.IsNotNull(dynamicProxy.Credentials);
        }

        [TestMethod()]
        public void CredentialsTest1()
        {
            DynamicProxy dynamicProxy = new DynamicProxy();
            var notExpected = dynamicProxy.Credentials;
            dynamicProxy.InnerProxy = new WebProxy("http://127.0.0.1:1080");
            var actual = dynamicProxy.Credentials;
            Assert.AreNotSame(notExpected, actual);
        }

        [TestMethod()]
        public void CredentialsTest2()
        {
            var input = new NetworkCredential();
            DynamicProxy dynamicProxy = new DynamicProxy();
            dynamicProxy.Credentials = input;
            Assert.AreNotSame(input, dynamicProxy.Credentials);
        }

        [TestMethod()]
        public void CredentialsTest3()
        {
            var input = new NetworkCredential();
            DynamicProxy dynamicProxy = new DynamicProxy();
            dynamicProxy.InnerProxy = new WebProxy("http://127.0.0.1:1080");
            dynamicProxy.Credentials = input;
            Assert.AreSame(input, dynamicProxy.Credentials);
        }

        [TestMethod()]
        public void IsBypassedTest0()
        {
            DynamicProxy dynamicProxy = new DynamicProxy();
            Uri uri = new Uri("http://test.com");
            Assert.IsTrue(dynamicProxy.IsBypassed(uri));
        }

        [TestMethod()]
        public void IsBypassedTest1()
        {
            DynamicProxy dynamicProxy = new DynamicProxy
            {
                InnerProxy = new WebProxy("http://127.0.0.1:1080", true)
            };
            Assert.IsTrue(dynamicProxy.IsBypassed(new Uri("http://127.0.0.1")));
            Assert.IsFalse(dynamicProxy.IsBypassed(new Uri("http://test.com")));
        }
    }
}