using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Downloader;

namespace DotnetSpider.Processor.Tests
{
    [TestClass()]
    public class BasePageProcessorTests
    {
        [TestMethod()]
        public void PropertyTest()
        {
            using Fakes.StubBasePageProcessor processor = new Fakes.StubBasePageProcessor
            {
                CallBase = true
            };
            log4net.Fakes.StubILog log = new log4net.Fakes.StubILog();
            processor.Logger = log;
            Assert.AreSame(log, processor.Logger);
            processor.Name = "name";
            Assert.AreEqual("name", processor.Name);
        }

        [TestMethod()]
        public async Task ProcessTest()
        {
            bool called = false;
            Fakes.StubBasePageProcessor processor = new Fakes.StubBasePageProcessor
            {
                CallBase = true,
                ProcessSyncResponse = page =>
                {
                    called = true;
                    return page.CreateResult();
                }
            };
            Request request = new Request("https://www.baidu.com");
            Response response = new Response(request);
            var result = await processor.Process(response);
            Assert.IsTrue(called);
            Assert.AreSame(request, result.Request);
        }
    }
}