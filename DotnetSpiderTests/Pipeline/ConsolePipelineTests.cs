using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using Microsoft.QualityTools.Testing.Fakes;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace DotnetSpider.Pipeline.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ConsolePipelineTests
    {
        private IDisposable _shimsContext = null;
        private string _output = string.Empty;

        [TestInitialize]
        public void Init()
        {
            _shimsContext = ShimsContext.Create();
            System.Fakes.ShimConsole.WriteLineString = s => _output += s + "\n";
        }

        [TestCleanup]
        public void Clean()
        {
            _shimsContext.Dispose();
            _shimsContext = null;
            _output = string.Empty;
        }

        [TestMethod()]
        [Timeout(5000)]
        public void PropertyTest()
        {
            using ConsolePipeline pipeline = new ConsolePipeline();
            log4net.Fakes.StubILog log = new log4net.Fakes.StubILog();
            pipeline.Logger = log;
            Assert.AreSame(log, pipeline.Logger);
            pipeline.Name = "name";
            Assert.AreEqual("name", pipeline.Name);
            Assert.IsFalse(pipeline.IsCacheSupported);
            Assert.ThrowsException<NotImplementedException>(() =>
            {
                bool _ = pipeline.EnableCache;
            });
            Assert.ThrowsException<NotImplementedException>(() =>
            {
                pipeline.EnableCache = true;
            });
            Assert.ThrowsException<NotImplementedException>(() =>
            {
                var _ = pipeline.MaxCacheTime;
            });
            Assert.ThrowsException<NotImplementedException>(() =>
            {
                pipeline.MaxCacheTime = Timeout.InfiniteTimeSpan;
            });
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ProcessTest0()
        {
            using ConsolePipeline pipeline = new ConsolePipeline();
            pipeline.Process(new Dictionary<string, object>()
                {
                    { "0", 0 }
                });
            Assert.AreEqual("0:\t0\n", _output);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ProcessTest1()
        {
            bool printLog = false;
            ILog log = new log4net.Fakes.StubILog
            {
                InfoObject = _ => printLog = true
            };
            using ConsolePipeline pipeline = new ConsolePipeline
            {
                Logger = log
            };
            pipeline.Process(new Dictionary<string, object>()
                {
                    { "0", 0 },
                    { "1", 1 },
                });
            Assert.IsTrue(printLog);
            string[] lines = _output.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, lines.Length);
            Assert.AreEqual("0:\t0", lines[0]);
            Assert.AreEqual("1:\t1", lines[1]);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ProcessTest2()
        {
            using ConsolePipeline pipeline = new ConsolePipeline();
            var task = pipeline.Process((IReadOnlyDictionary<string, object>)null);
            Assert.AreSame(Task.CompletedTask, task);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ProcessTest3()
        {
            using ConsolePipeline pipeline = new ConsolePipeline();
            var task = pipeline.Process((IEnumerable<IReadOnlyDictionary<string, object>>)null);
            Assert.AreSame(Task.CompletedTask, task);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ProcessTest4()
        {
            bool printLog = false;
            ILog log = new log4net.Fakes.StubILog
            {
                InfoObject = _ => printLog = true
            };
            using ConsolePipeline pipeline = new ConsolePipeline
            {
                Logger = log
            };
            var results = new List<IReadOnlyDictionary<string, object>>();
            pipeline.Process(results);
            Assert.IsFalse(printLog);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ProcessTest5()
        {
            bool printLog = false;
            ILog log = new log4net.Fakes.StubILog
            {
                InfoObject = _ => printLog = true
            };
            using ConsolePipeline pipeline = new ConsolePipeline
            {
                Logger = log
            };
            var results = new List<IReadOnlyDictionary<string, object>>
            {
                new Dictionary<string, object>()
                {
                    { "0", 0 }
                }
            };
            pipeline.Process(results);
            Assert.AreEqual("0:\t0\n", _output);
            Assert.IsTrue(printLog);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ProcessTest6()
        {
            using ConsolePipeline pipeline = new ConsolePipeline();
            var results = new List<IReadOnlyDictionary<string, object>>
            {
                new Dictionary<string, object>()
                {
                    { "0", 0 }
                },
                null
            };
            pipeline.Process(results);
            Assert.AreEqual("0:\t0\n", _output);
        }
    }
}