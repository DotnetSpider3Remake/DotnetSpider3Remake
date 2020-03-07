using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DotnetSpider.Pipeline.Tests
{
    [TestClass()]
    public class ConsolePipelineTests
    {
        private readonly byte[] _data = new byte[4096];
        private TextWriter _out = null;

        [TestInitialize]
        public void Init()
        {
            _out = Console.Out;
        }

        [TestCleanup]
        public void Clean()
        {
            Console.SetOut(_out);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ProcessTest0()
        {
            using (MemoryStream memory = new MemoryStream(_data))
            {
                using StreamWriter writer = new StreamWriter(memory);
                Console.SetOut(writer);
                using ConsolePipeline pipeline = new ConsolePipeline();
                pipeline.Process(new Dictionary<string, object>()
                {
                    { "0", 0 }
                });
            }


            using (MemoryStream memory = new MemoryStream(_data))
            {
                using StreamReader reader = new StreamReader(memory);
                Assert.AreEqual("0:\t0", reader.ReadLine());
            }
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ProcessTest1()
        {
            using (MemoryStream memory = new MemoryStream(_data))
            {
                using StreamWriter writer = new StreamWriter(memory);
                Console.SetOut(writer);
                using ConsolePipeline pipeline = new ConsolePipeline();
                pipeline.Process(new Dictionary<string, object>()
                {
                    { "0", 0 },
                    { "1", 1 },
                });
            }


            using (MemoryStream memory = new MemoryStream(_data))
            {
                using StreamReader reader = new StreamReader(memory);
                Assert.AreEqual("0:\t0", reader.ReadLine());
                Assert.AreEqual("1:\t1", reader.ReadLine());
            }
        }
    }
}