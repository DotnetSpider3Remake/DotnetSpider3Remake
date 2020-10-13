using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

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
            Assert.IsFalse(a.Equals(b));
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

        [TestMethod()]
        public void EqualsTest6()
        {
            var a = new Request();
            Assert.IsTrue(a.Equals(a));
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
            a.Headers.Add("1", "1");
            var b = new Request("test");
            b.Headers.Add("1", "1");
            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
        }

        [TestMethod()]
        public void GetHashCodeTest3()
        {
            var a = new Request("test");
            var b = new Request("test");
            b.Properties.Add("1", 1);
            Assert.IsTrue(a.GetHashCode() != b.GetHashCode());
        }

        #endregion

        [TestMethod()]
        public void JsonStringTest()
        {
            var r = new Request("https://www.baidu.com");
            r.Origin = "https://www.google.com";
            r.Properties.Add("test", (long)0);
            string json = r.ToString();
            Assert.IsTrue(json.Contains("https://www.baidu.com"));
            Assert.IsTrue(json.Contains("https://www.google.com"));
            Assert.IsTrue(json.Contains("test"));
            Assert.IsTrue(json.Contains("0"));
            Assert.AreEqual(r, Request.FromString(json));
        }

        [TestMethod()]
        public void GetUriTest()
        {
            Request request = new Request("http://www.baidu.com");
            Uri uri = request.GetUri();
            Assert.AreEqual("http://www.baidu.com", uri.OriginalString);
            Assert.AreNotSame(uri, request.GetUri());
        }

        #region 公共属性
        [TestMethod()]
        public void PropertyTest0()
        {
            Request request = new Request(null);
            Assert.AreEqual(HttpMethod.Get, request.Method);
            request.Method = HttpMethod.Post;
            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.IsNotNull(request.Headers);
        }

        [TestMethod()]
        public void PropertyTest1()
        {
            Request request = new Request(null);

            Assert.IsNull(request.Accept);
            request.Accept = "1";
            Assert.AreEqual("1", request.Accept);
        }

        [TestMethod()]
        public void PropertyTest2()
        {
            Request request = new Request(null);

            Assert.IsNull(request.UserAgent);
            request.UserAgent = "1";
            Assert.AreEqual("1", request.UserAgent);
        }

        [TestMethod()]
        public void PropertyTest3()
        {
            Request request = new Request(null);

            Assert.IsNull(request.Referer);
            request.Referer = "1";
            Assert.AreEqual("1", request.Referer);
        }

        [TestMethod()]
        public void PropertyTest4()
        {
            Request request = new Request(null);

            Assert.IsNull(request.Origin);
            request.Origin = "1";
            Assert.AreEqual("1", request.Origin);
        }

        [TestMethod()]
        public void PropertyTest5()
        {
            Request request = new Request(null);

            Assert.IsNull(request.ContentType);
            request.ContentType = "1";
            Assert.AreEqual("1", request.ContentType);
        }

        [TestMethod()]
        public void PropertyTest6()
        {
            Request request = new Request(null);

            Assert.IsNull(request.XRequestedWith);
            request.XRequestedWith = "1";
            Assert.AreEqual("1", request.XRequestedWith);
        }
        #endregion

        #region 内容数据
        [TestMethod()]
        public void ContentDataTest0()
        {
            Request request = new Request(null);
            Assert.IsNull(request.ContentData);
            byte[] expectedArray = new byte[2];
            request.ContentData = expectedArray;
            Assert.AreSame(expectedArray, request.ContentData);
        }

        [TestMethod()]
        public void ContentDataTest1()
        {
            Request request = new Request(null);
            Assert.IsNull(request.ContentData);
            byte[] expectedArray = new byte[2];
            request.ContentData = expectedArray;
            request.EncodingName = "UTF-32";
            Assert.AreSame(expectedArray, request.ContentData);
        }

        [TestMethod()]
        public void ContentDataTest2()
        {
            Request request = new Request(null);
            Assert.IsNull(request.ContentData);
            byte[] unexpectedArray = new byte[2];
            request.ContentData = unexpectedArray;
            request.Content = "test";
            Assert.AreNotSame(unexpectedArray, request.ContentData);
            var actual = request.ContentData;
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("test"), actual);
            Assert.AreSame(actual, request.ContentData);
        }

        [TestMethod()]
        public void ContentDataTest3()
        {
            Request request = new Request(null);
            Assert.IsNull(request.ContentData);
            request.Content = "test";
            var actual = request.ContentData;
            request.EncodingName = "UTF-32";
            CollectionAssert.AreNotEqual(actual, request.ContentData);
            CollectionAssert.AreEqual(Encoding.UTF32.GetBytes("test"), request.ContentData);
        }



        [TestMethod]
        public void GetCompressedContentDataTest0()
        {
            byte[] expected = new byte[] { 7, 8 };
            Request request = new Request
            {
                CompressMode = CompressMode.None,
                ContentData = expected
            };
            var actual = request.GetCompressedContentData();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetCompressedContentDataTest1()
        {
            byte[] expected = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0, 99, 231, 0, 0, 10, 12, 67, 0, 2, 0, 0, 0 };
            Request request = new Request
            {
                CompressMode = CompressMode.Gzip,
                ContentData = new byte[] { 7, 8 }
            };
            var actual = request.GetCompressedContentData();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetCompressedContentDataTest2()
        {
            byte[] expected = new byte[] { 32, 7, 8 };
            Request request = new Request
            {
                CompressMode = CompressMode.Lz4,
                ContentData = new byte[] { 7, 8 }
            };
            var actual = request.GetCompressedContentData();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetCompressedContentDataTest4()
        {
            byte[] expected = new byte[] { 99, 231, 0, 0 };
            Request request = new Request
            {
                CompressMode = CompressMode.Deflate,
                ContentData = new byte[] { 7, 8 }
            };
            var actual = request.GetCompressedContentData();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetCompressedContentDataTest5()
        {
            Request request = new Request();
            Assert.ThrowsException<NullReferenceException>(request.GetCompressedContentData);
        }
        #endregion
    }
}