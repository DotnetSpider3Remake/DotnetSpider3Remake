using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DotnetSpider.Pipeline.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class BaseCachePipelineTests
    {
        private IDisposable _shimsContext = null;
        Fakes.StubBaseCachePipeline _instance = null;
        Fakes.ShimBaseCachePipeline _instanceShim = null;
        PrivateObject _private = null;

        [TestInitialize]
        public void Init()
        {
            _shimsContext = ShimsContext.Create();
            _instance = new Fakes.StubBaseCachePipeline
            {
                CallBase = true
            };
            _instanceShim = new Fakes.ShimBaseCachePipeline(_instance);
            _private = new PrivateObject(_instance, new PrivateType(typeof(BaseCachePipeline)));
        }

        [TestCleanup]
        public void Clean()
        {
            _private = null;
            _instanceShim = null;
            _instance.Dispose();
            _instance = null;
            _shimsContext.Dispose();
            _shimsContext = null;
        }

        [TestMethod()]
        public void PropertyTest0()
        {
            log4net.Fakes.StubILog log = new log4net.Fakes.StubILog();
            _instance.Logger = log;
            Assert.AreSame(log, _instance.Logger);
            _instance.Name = "name";
            Assert.AreEqual("name", _instance.Name);
        }

        [TestMethod()]
        public void PropertyTest1()
        {
            Assert.IsTrue(_instance.IsCacheSupported);
            Assert.IsTrue(_instance.EnableCache);
            _instance.EnableCache = false;
            Assert.IsFalse(_instance.EnableCache);
            Assert.AreEqual(TimeSpan.FromSeconds(1), _instance.MaxCacheTime);
            _instance.MaxCacheTime = TimeSpan.FromSeconds(2);
            Assert.AreEqual(TimeSpan.FromSeconds(2), _instance.MaxCacheTime);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task WaitForNextProcessTest0()
        {
            _instance.MaxCacheTime = TimeSpan.FromMilliseconds(10);
            Task wait = (Task)_private.Invoke("WaitForNextProcess", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsFalse(wait.IsCompleted);
            _instance.Dispose();
            await wait;
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task WaitForNextProcessTest1()
        {
            _instance.MaxCacheTime = TimeSpan.FromMilliseconds(10);
            Task wait = (Task)_private.Invoke("WaitForNextProcess");
            Assert.IsFalse(wait.IsCompleted);
            await Task.Delay(200);
            Assert.IsTrue(wait.IsCompleted);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessTest0()
        {
            bool called = false;
            _instance.ProcessIEnumerableOfTupleOfIEnumerableOfIReadOnlyDictionaryOfStringObjectISpider = _ =>
            {
                called = true;
                return Task.CompletedTask;
            };
            await (Task)_private.Invoke("Process");
            Assert.IsFalse(called);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessTest1()
        {
            IEnumerable<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>> processedItems = null;
            _instance.ProcessIEnumerableOfTupleOfIEnumerableOfIReadOnlyDictionaryOfStringObjectISpider = c =>
            {
                processedItems = c;
                return Task.CompletedTask;
            };
            var expected = new List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>
            {
                null
            };
            _private.SetField("_caches", expected);
            await (Task)_private.Invoke("Process");
            Assert.AreSame(expected, processedItems);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessTest2()
        {
            var expectedException = new Exception();
            _instance.ProcessIEnumerableOfTupleOfIEnumerableOfIReadOnlyDictionaryOfStringObjectISpider = _ =>
            {
                throw expectedException;
            };
            var caches = new List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>
            {
                null
            };
            _private.SetField("_caches", caches);
            await (Task)_private.Invoke("Process");

            string msg = null;
            Exception actualException = null;
            _instance.Logger = new log4net.Fakes.StubILog
            {
                ErrorObjectException = (o, e) =>
                {
                    msg = (string)o;
                    actualException = e;
                }
            };
            _instance.Name = "test";
            _private.SetField("_caches", caches);
            await (Task)_private.Invoke("Process");
            Assert.AreSame(expectedException, actualException);
            Assert.AreEqual("test has an exception when exec Process.", msg);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void RunCachePipelineTest0()
        {
            int processCalled = 0;
            int waitForNextProcessCalled = 0;
            _instanceShim.Process = async () =>
            {
                ++processCalled;
                await Task.Delay(1);
            };
            _instanceShim.WaitForNextProcess = async () =>
            {
                ++waitForNextProcessCalled;
                await Task.Delay(5);
            };
            _ = Task.Run(async () =>
            {
                await Task.Delay(100);
                _instance.Dispose();
            });
            _private.Invoke("RunCachePipeline");
            Assert.AreEqual(processCalled, waitForNextProcessCalled + 1);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void RunCachePipelineTest1()
        {
            int processCalled = 0;
            int waitForNextProcessCalled = 0;
            _instanceShim.Process = async () =>
            {
                ++processCalled;
                await Task.Delay(1);
            };
            _instanceShim.WaitForNextProcess = async () =>
            {
                ++waitForNextProcessCalled;
                await Task.Delay(5);
            };
            _instance.Dispose();
            _private.Invoke("RunCachePipeline");
            Assert.AreEqual(0, waitForNextProcessCalled);
            Assert.AreEqual(1, processCalled);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task InitCachePipeline0()
        {
            bool callRunCachePipeline = false;
            _instanceShim.RunCachePipeline = () => callRunCachePipeline = true;
            _private.Invoke("InitCachePipeline");
            await Task.Delay(100);
            Assert.IsTrue(callRunCachePipeline);

            callRunCachePipeline = false;
            _private.Invoke("InitCachePipeline");
            await Task.Delay(100);
            Assert.IsFalse(callRunCachePipeline);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task InitCachePipeline1()
        {
            bool callRunCachePipeline = false;
            _instanceShim.RunCachePipeline = () => callRunCachePipeline = true;
            _instance.Dispose();
            _private.Invoke("InitCachePipeline");
            await Task.Delay(100);
            Assert.IsFalse(callRunCachePipeline);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessIReadOnlyDictionaryOfStringObjectISpiderTest0()
        {
            bool callInitCachePipeline = false;
            _instanceShim.InitCachePipeline = () => callInitCachePipeline = true;
            var caches = (List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>)_private.GetField("_caches");
            await _instance.Process((IReadOnlyDictionary<string, object>)null, null);
            Assert.IsFalse(callInitCachePipeline);
            Assert.AreEqual(0, caches.Count);

            await _instance.Process(new Dictionary<string, object>(), null);
            Assert.IsTrue(callInitCachePipeline);
            Assert.AreEqual(1, caches.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessIReadOnlyDictionaryOfStringObjectISpiderTest1()
        {
            bool callInitCachePipeline = false;
            _instanceShim.InitCachePipeline = () => callInitCachePipeline = true;
            var caches = (List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>)_private.GetField("_caches");
            caches.Add(Tuple.Create<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>(new List<IReadOnlyDictionary<string, object>> { null }, null));
            await _instance.Process(new Dictionary<string, object>(), null);
            Assert.IsTrue(callInitCachePipeline);
            Assert.AreEqual(1, caches.Count);
            Assert.AreEqual(2, caches[0].Item1.Count());
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessIReadOnlyDictionaryOfStringObjectISpiderTest2()
        {
            bool callInitCachePipeline = false;
            _instanceShim.InitCachePipeline = () => callInitCachePipeline = true;
            var caches = (List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>)_private.GetField("_caches");
            caches.Add(Tuple.Create<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>(null, null));
            await _instance.Process(new Dictionary<string, object>(), new DotnetSpider.Fakes.StubISpider());
            Assert.IsTrue(callInitCachePipeline);
            Assert.AreEqual(2, caches.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessIReadOnlyDictionaryOfStringObjectISpiderTest3()
        {
            bool callInitCachePipeline = false;
            _instanceShim.InitCachePipeline = () => callInitCachePipeline = true;
            bool callProcess = false;
            _instance.ProcessIEnumerableOfTupleOfIEnumerableOfIReadOnlyDictionaryOfStringObjectISpider = caches =>
            {
                Assert.AreEqual(1, caches.Count());
                Assert.IsNotNull(caches.First().Item1);
                Assert.AreEqual(1, caches.First().Item1.Count());
                Assert.IsNull(caches.First().Item2);
                callProcess = true;
                return Task.CompletedTask;
            };
            _instance.EnableCache = false;
            var caches = (List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>)_private.GetField("_caches");
            await _instance.Process(new Dictionary<string, object>(), null);
            Assert.IsFalse(callInitCachePipeline);
            Assert.AreEqual(0, caches.Count);
            Assert.IsTrue(callProcess);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessIEnumerableOfIReadOnlyDictionaryOfStringObjectISpiderTest0()
        {
            bool callInitCachePipeline = false;
            _instanceShim.InitCachePipeline = () => callInitCachePipeline = true;
            var caches = (List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>)_private.GetField("_caches");
            
            await _instance.Process((IEnumerable<IReadOnlyDictionary<string, object>>)null, null);
            Assert.IsFalse(callInitCachePipeline);
            Assert.AreEqual(0, caches.Count);

            await _instance.Process(new List<IReadOnlyDictionary<string, object>>(), null);
            Assert.IsTrue(callInitCachePipeline);
            Assert.AreEqual(1, caches.Count);
            Assert.AreEqual(0, caches[0].Item1.Count());
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessIEnumerableOfIReadOnlyDictionaryOfStringObjectISpiderTest1()
        {
            bool callInitCachePipeline = false;
            _instanceShim.InitCachePipeline = () => callInitCachePipeline = true;
            var caches = (List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>)_private.GetField("_caches");
            caches.Add(Tuple.Create<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>(new List<IReadOnlyDictionary<string, object>> { null }, null));
            await _instance.Process(new List<IReadOnlyDictionary<string, object>> { null }, null);
            Assert.IsTrue(callInitCachePipeline);
            Assert.AreEqual(1, caches.Count);
            Assert.AreEqual(2, caches[0].Item1.Count());
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessIEnumerableOfIReadOnlyDictionaryOfStringObjectISpiderTest2()
        {
            bool callInitCachePipeline = false;
            _instanceShim.InitCachePipeline = () => callInitCachePipeline = true;
            var caches = (List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>)_private.GetField("_caches");
            caches.Add(Tuple.Create<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>(null, null));
            await _instance.Process(new List<IReadOnlyDictionary<string, object>>(), new DotnetSpider.Fakes.StubISpider());
            Assert.IsTrue(callInitCachePipeline);
            Assert.AreEqual(2, caches.Count);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ProcessIEnumerableOfIReadOnlyDictionaryOfStringObjectISpiderTest3()
        {
            bool callInitCachePipeline = false;
            _instanceShim.InitCachePipeline = () => callInitCachePipeline = true;
            bool callProcess = false;
            _instance.ProcessIEnumerableOfTupleOfIEnumerableOfIReadOnlyDictionaryOfStringObjectISpider = caches =>
            {
                Assert.AreEqual(1, caches.Count());
                Assert.IsNotNull(caches.First().Item1);
                Assert.AreEqual(1, caches.First().Item1.Count());
                Assert.IsNull(caches.First().Item2);
                callProcess = true;
                return Task.CompletedTask;
            };
            _instance.EnableCache = false;
            var caches = (List<Tuple<IEnumerable<IReadOnlyDictionary<string, object>>, ISpider>>)_private.GetField("_caches");
            await _instance.Process(new List<IReadOnlyDictionary<string, object>> { null }, null);
            Assert.IsFalse(callInitCachePipeline);
            Assert.AreEqual(0, caches.Count);
            Assert.IsTrue(callProcess);
        }
    }
}