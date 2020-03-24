using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Proxy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy.Tests
{
    [TestClass()]
    public class FixedHttpProxyTests
    {
        [TestMethod()]
        public async Task GetProxyTest()
        {
            WebProxy proxy = new WebProxy("http://127.0.0.1:1080");
            FixedHttpProxy fixedHttpProxy = new FixedHttpProxy(proxy);
            Assert.AreSame(proxy, await fixedHttpProxy.GetProxy(null));
        }

        [TestMethod()]
        public async Task ReturnProxyTest()
        {
            WebProxy proxy = new WebProxy("http://127.0.0.1:1080");
            FixedHttpProxy fixedHttpProxy = new FixedHttpProxy(proxy);
            await fixedHttpProxy.GetProxy(null);
            await fixedHttpProxy.ReturnProxy(null, null);
            await fixedHttpProxy.ReturnProxy(null, null);
            Assert.AreSame(proxy, await fixedHttpProxy.GetProxy(null));
        }
    }
}