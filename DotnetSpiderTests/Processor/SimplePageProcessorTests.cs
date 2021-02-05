using DotnetSpider.Downloader;
using DotnetSpider.Processor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpiderTests.Processor.Tests
{
    [TestClass]
    public class SimplePageProcessorTests
    {
        [TestMethod]
        public void ProcessorTest()
        {
            SimplePageProcessor processor = new SimplePageProcessor();
            PrivateObject privateObject = new PrivateObject(processor, new PrivateType(typeof(BasePageProcessor)));
            Assert.IsNull(processor.Processor);
            Assert.IsNull(privateObject.Invoke("ProcessSync", (Response)null));

            bool called = false;
            processor.Processor = (_1, _2) =>
            {
                called = true;
                return null;
            };
            Assert.IsNull(privateObject.Invoke("ProcessSync", (Response)null));
            Assert.IsTrue(called);
        }
    }
}
