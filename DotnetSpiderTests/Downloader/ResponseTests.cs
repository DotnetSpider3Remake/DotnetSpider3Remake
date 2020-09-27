using DotnetSpider.Downloader;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader.Tests
{
    [TestClass()]
    public class ResponseTests
    {
        [TestMethod()]
        public void ResponseTest0()
        {
            Response response = new Response();
            Assert.IsNull(response.Request);
        }

        [TestMethod()]
        public void ResponseTest1()
        {
            Request request = new Request();
            Response response = new Response(request);
            Assert.AreSame(request, response.Request);
        }

        [TestMethod()]
        public void ToStringTest()
        {
            Request request = new Request("http://www.baidu.com");
            Response response = new Response(request);
            string json = response.ToString();
            Assert.IsTrue(json.Contains("http://www.baidu.com"));
        }

        [TestMethod()]
        public void FromStringTest()
        {
            Response response = Response.FromString("{}");
            Assert.IsNull(response.Request);
            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [TestMethod()]
        public void PropertyTest0()
        {
            Request request = new Request("http://www.baidu.com");
            Response response = new Response(request);
            response.Request = null;
            Assert.AreEqual(null, response.Request);

            response.TargetUrl = "test";
            Assert.AreEqual("test", response.TargetUrl);

            response.Content = "hello";
            Assert.AreEqual("hello", response.Content);

            response.ContentType = "plain/text";
            Assert.AreEqual("plain/text", response.ContentType);

            response.StatusCode = HttpStatusCode.OK;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            response.IsDownloaderTimeout = false;
            Assert.IsFalse(response.IsDownloaderTimeout);

            List<Dictionary<string, string>> cookies = new List<Dictionary<string, string>>();
            response.SetCookies = cookies;
            Assert.AreSame(cookies, response.SetCookies);

            Dictionary<string, HashSet<string>> headers = new Dictionary<string, HashSet<string>>();
            Assert.IsNull(response.Headers);
            response.Headers = headers;
            Assert.AreSame(headers, response.Headers);
        }

        [TestMethod]
        public void HtmlNodeTest0()
        {
            Response response = new Response();
            Assert.IsNull(response.HtmlNode);
        }

        [TestMethod]
        public void HtmlNodeTest1()
        {
            Response response = new Response();
            response.Content = "<html></html>";
            var node = response.HtmlNode;
            Assert.IsNotNull(node);
            Assert.AreSame(node, response.HtmlNode);
        }

        [TestMethod]
        public void XmlElementTest0()
        {
            Response response = new Response();
            Assert.IsNull(response.XmlElement);
        }

        [TestMethod]
        public void XmlElementTest1()
        {
            Response response = new Response();
            response.Content = "<html></html>";
            var node = response.XmlElement;
            Assert.IsNotNull(node);
            Assert.AreSame(node, response.XmlElement);
        }

        [TestMethod]
        public void IsSuccessStatusCodeTest0()
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.OK;
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public void IsSuccessStatusCodeTest1()
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.Continue;
            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public void IsSuccessStatusCodeTest2()
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.Unauthorized;
            Assert.IsFalse(response.IsSuccessStatusCode);
        }
    }
}