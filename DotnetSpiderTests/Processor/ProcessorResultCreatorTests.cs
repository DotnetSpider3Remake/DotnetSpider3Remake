using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Downloader;
using System.Diagnostics.CodeAnalysis;

namespace DotnetSpider.Processor.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ProcessorResultCreatorTests
    {
        [TestMethod()]
        public void CreateResultTest0()
        {
            Response response = new Response();
            Assert.ThrowsException<ArgumentNullException>(response.CreateResult);
        }

        [TestMethod()]
        public void CreateResultTest1()
        {
            Request request = new Request("http://www.baidu.com");
            Response response = new Response(request);
            var result = response.CreateResult();
            Assert.AreSame(request, result.Request);
        }

        [TestMethod()]
        public void CreateResultTest2()
        {
            Request request = new Request("http://www.baidu.com");
            Response response = new Response(request);
            var result = ProcessorResultCreator.CreateResult(null, response);
            Assert.AreSame(request, result.Request);
        }

        [TestMethod()]
        public void CreateResultTest3()
        {
            Response response = new Response();
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                ProcessorResultCreator.CreateResult(null, response);
            });
        }
    }
}