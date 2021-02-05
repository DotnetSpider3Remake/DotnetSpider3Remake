using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using DotnetSpider.Monitor;
using Microsoft.QualityTools.Testing.Fakes.Stubs;

namespace DotnetSpider.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class BaseSpiderTests
    {
        #region 全局设定
        private IDisposable _shimsContext = null;
        private StubObserver _stubObserver = null;
        private Fakes.StubBaseSpider _instance = null;
        private Fakes.ShimBaseSpider _instanceShim = null;
        private PrivateObject _private = null;
        private PrivateType _type = null;

        [TestInitialize]
        public void Init()
        {
            _shimsContext = ShimsContext.Create();
            _stubObserver = new StubObserver();
            _instance = new Fakes.StubBaseSpider
            {
                CallBase = true,
                InstanceObserver = _stubObserver
            };
            _instanceShim = new Fakes.ShimBaseSpider(_instance);
            _type = new PrivateType(typeof(BaseSpider));
            _private = new PrivateObject(_instance, _type);
        }

        [TestCleanup]
        public void Clean()
        {
            _shimsContext.Dispose();
            _shimsContext = null;
            _type = null;
            _private = null;
            _instanceShim = null;
            _instance.Dispose();
            _instance = null;
        }
        #endregion

        #region 实现接口的测试
        [TestMethod()]
        public void ContinueTest0()
        {
            _private.SetField("_hasExit", true);
            _private.SetField("_isRunning", false);
            Assert.IsFalse(_instance.Continue());
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public void ContinueTest1()
        {
            _private.SetField("_hasExit", false);
            _private.SetField("_isRunning", true);
            Assert.IsFalse(_instance.Continue());
            Assert.IsTrue((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public void ContinueTest2()
        {
            _private.SetField("_hasExit", false);
            _private.SetField("_isRunning", false);
            Assert.IsTrue(_instance.Continue());
            Assert.IsTrue((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public async Task ContinueAsyncTest0()
        {
            int callTimes = 0;
            _instanceShim.Continue = () => 
            { 
                ++callTimes; 
                return false; 
            };
            Assert.IsFalse(await _instance.ContinueAsync());
            Assert.AreEqual(1, callTimes);
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ExitTest0()
        {
            _private.SetField("_hasExit", true);
            _instance.Scheduler = new Scheduler.Fakes.StubIScheduler()
            {
                Clear = Assert.Fail
            };
            Assert.IsFalse(_instance.Exit());
            Assert.IsTrue((bool)_private.GetField("_hasExit"));
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ExitTest1()
        {
            _private.SetField("_hasExit", false);
            int called = 0;
            _instance.Scheduler = new Scheduler.Fakes.StubIScheduler()
            {
                Clear = () => ++called
            };
            Assert.IsTrue(_instance.Exit());
            Assert.AreEqual(1, called);
            Assert.IsTrue((bool)_private.GetField("_hasExit"));
        }

        [TestMethod()]
        [Timeout(5000)]
        public void ExitTest2()
        {
            _private.SetField("_hasExit", false);
            Assert.IsTrue(_instance.Exit());
            Assert.IsTrue((bool)_private.GetField("_hasExit"));
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task ExitAsyncTest0()
        {
            int callTimes = 0;
            _instanceShim.Exit = () =>
            {
                ++callTimes;
                return false;
            };
            Assert.IsFalse(await _instance.ExitAsync());
            Assert.AreEqual(1, callTimes);
        }

        [TestMethod()]
        public void PauseTest0()
        {
            _private.SetField("_hasExit", true);
            _private.SetField("_isRunning", false);
            Assert.IsFalse(_instance.Pause());
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public void PauseTest1()
        {
            _private.SetField("_hasExit", false);
            _private.SetField("_isRunning", true);
            Assert.IsTrue(_instance.Pause());
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public void PauseTest2()
        {
            _private.SetField("_hasExit", false);
            _private.SetField("_isRunning", false);
            Assert.IsFalse(_instance.Pause());
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task PauseAsyncTest0()
        {
            int callTimes = 0;
            _instanceShim.Pause = () =>
            {
                ++callTimes;
                return false;
            };
            Assert.IsFalse(await _instance.PauseAsync());
            Assert.AreEqual(1, callTimes);
        }

        [TestMethod()]
        public void RunTest0()
        {
            _instanceShim.InitLogger = Assert.Fail;
            _instanceShim.InitSpider = Assert.Fail;
            _instanceShim.Exit = () =>
            {
                Assert.Fail();
                return false;
            };
            _instanceShim.CheckConfiguration = () => 
            { 
                Assert.Fail();
                return false; 
            };
            _private.SetField("_hasStarted", true);
            _instance.Run();
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
        }

        [TestMethod()]
        public void RunTest1()
        {
            int calledIndex = 0;
            _instanceShim.InitLogger = () =>
            {
                Assert.AreEqual(2, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
            };
            _instanceShim.InitSpider = () =>
            {
                Assert.AreEqual(1, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
            };
            _instanceShim.Exit = () =>
            {
                Assert.AreEqual(4, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
                _private.SetField("_isRunning", false);
                return true;
            };
            _instanceShim.CheckConfiguration = () =>
            {
                Assert.AreEqual(3, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
                return false;
            };
            _instance.Run();
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
            Assert.AreEqual(4, calledIndex);
        }

        [TestMethod()]
        public void RunTest2()
        {
            int calledIndex = 0;
            _instanceShim.InitLogger = () =>
            {
                Assert.AreEqual(2, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
            };
            _instanceShim.InitSpider = () =>
            {
                Assert.AreEqual(1, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
            };
            _instanceShim.Exit = () =>
            {
                Assert.AreEqual(4, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
                _private.SetField("_isRunning", false);
                return true;
            };
            _instanceShim.CheckConfiguration = () =>
            {
                Assert.AreEqual(3, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
                return false;
            };
            _instance.Run();
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
            Assert.AreEqual(4, calledIndex);
        }

        [TestMethod()]
        public void RunTest3()
        {
            Assert.AreEqual(1, _instance.Parallels);
            int calledIndex = 0;
            _instanceShim.InitLogger = () =>
            {
                Assert.AreEqual(2, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
            };
            _instanceShim.InitSpider = () =>
            {
                Assert.AreEqual(1, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
            };
            _instanceShim.CheckConfiguration = () =>
            {
                Assert.AreEqual(3, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
                return true;
            };
            _instance.RunSpiderThread01 = () =>
            {
                Assert.AreEqual(4, ++calledIndex);
                Thread.Sleep(50);
            };
            _instanceShim.Exit = () =>
            {
                Assert.AreEqual(5, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
                _private.SetField("_isRunning", false);
                return true;
            };
            _instance.Run();
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
            Assert.AreEqual(5, calledIndex);
        }


        [TestMethod()]
        public void RunTest4()
        {
            Assert.AreEqual(1, _instance.Parallels);
            int calledIndex = 0;
            string[] logs = new string[] { "Spider start.", "Spider exit." };
            int logIndex = 0;
            _instanceShim.InitLogger = () =>
            {
                Assert.AreEqual(2, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
                _instance.Logger = new log4net.Fakes.StubILog()
                {
                    InfoObject = o =>
                    {
                        string msg = o as string;
                        Assert.IsTrue(logIndex < logs.Length);
                        Assert.AreEqual(logs[logIndex++], msg);
                    }
                };
            };
            _instanceShim.InitSpider = () =>
            {
                Assert.AreEqual(1, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
            };
            _instanceShim.CheckConfiguration = () =>
            {
                Assert.AreEqual(3, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
                return true;
            };
            _instance.ConditionOfStop = _ =>
            {
                Assert.Fail();
                return false;
            };
            _instanceShim.StartThreadThreadStart = _ => Assert.AreEqual(4, ++calledIndex);
            _instance.RunSpiderThread01 = () =>
            {
                Assert.AreEqual(5, ++calledIndex);
                Thread.Sleep(50);
            };
            _instanceShim.Exit = () =>
            {
                Assert.AreEqual(6, ++calledIndex);
                Assert.IsTrue((bool)_private.GetField("_isRunning"));
                _private.SetField("_isRunning", false);
                return true;
            };
            _instance.Run();
            Assert.IsFalse((bool)_private.GetField("_isRunning"));
            Assert.AreEqual(6, calledIndex);
            Assert.AreEqual(logs.Length, logIndex);
        }

        [TestMethod()]
        [Timeout(5000)]
        public async Task RunAsyncTest()
        {
            int callTimes = 0;
            _instanceShim.Run = () => ++callTimes;
            await _instance.RunAsync();
            Assert.AreEqual(1, callTimes);
        }
        #endregion

        #region 可以在派生类中重新实现的函数的测试
        [TestMethod]
        public void DisposeOthersTest0()
        {
            int callTimes = 0;
            _instanceShim.Exit = () =>
            {
                Assert.AreEqual(1, ++callTimes);
                return true;
            };
            _private.Invoke("DisposeOthers");
            Assert.AreEqual(1, callTimes);
        }

        [TestMethod]
        public void DisposeOthersTest1()
        {
            int callTimes = 0;
            _instanceShim.Exit = () =>
            {
                Assert.AreEqual(1, ++callTimes);
                return true;
            };
            _instance.Scheduler = new Scheduler.Fakes.StubIScheduler
            {
                Dispose = () => Assert.AreEqual(2, ++callTimes)
            };
            _instance.Downloader = new Downloader.Fakes.StubIDownloader
            {
                Dispose = () => Assert.AreEqual(3, ++callTimes)
            };
            _instance.Pipelines.Add(new Pipeline.Fakes.StubIPipeline
            {
                Dispose = () => Assert.AreEqual(4, ++callTimes)
            });
            _instance.PageProcessors.Add(new Processor.Fakes.StubIResponseProcessor
            {
                Dispose = () => Assert.AreEqual(5, ++callTimes)
            });
            _instance.HttpProxy = new Proxy.Fakes.StubIHttpProxy
            {
                Dispose = () => Assert.AreEqual(6, ++callTimes)
            };
            _private.Invoke("DisposeOthers");
            Assert.AreEqual(6, callTimes);
        }

        [TestMethod]
        public void InitSpiderTest0()
        {
            _private.Invoke("InitSpider");
        }

        [TestMethod]
        public void InitLoggerTest0()
        {
            _instance.Logger = new log4net.Fakes.StubILog();
            _instanceShim.SetLoggerIRecordable = _ => Assert.Fail();
            _private.Invoke("InitLogger");
        }

        [TestMethod]
        public void InitLoggerTest1()
        {
            int callTimes = 0;
            _instanceShim.SetLoggerIRecordable = _ => ++callTimes;
            _instance.Pipelines.Add(new Pipeline.Fakes.StubIPipeline());
            _instance.PageProcessors.Add(new Processor.Fakes.StubIResponseProcessor());
            _private.Invoke("InitLogger");
            Assert.AreEqual(6, callTimes);
        }

        [TestMethod]
        public void SetLoggerTest0()
        {
            _private.Invoke("SetLogger", (IRecordable)null);
        }

        [TestMethod]
        public void SetLoggerTest1()
        {
            var logger = new log4net.Fakes.StubILog();
            DotnetSpider.Monitor.Fakes.StubIRecordable rd = new Monitor.Fakes.StubIRecordable
            {
                LoggerGet = () => logger,
                LoggerSetILog = v => Assert.Fail()
            };
            _private.Invoke("SetLogger", rd);
        }

        [TestMethod]
        public void SetLoggerTest2()
        {
            log4net.ILog logger = null;
            DotnetSpider.Monitor.Fakes.StubIRecordable rd = new Monitor.Fakes.StubIRecordable
            {
                LoggerGet = () => logger,
                LoggerSetILog = v => logger = v,
                NameGet = () => null
            };
            var expected = new log4net.Fakes.StubILog();
            DotnetSpider.Logger.Fakes.ShimLoggerCreator.GetLoggerType = t => expected;
            DotnetSpider.Logger.Fakes.ShimLoggerCreator.GetLoggerString = n =>
            {
                Assert.Fail();
                return null;
            };
            _private.Invoke("SetLogger", rd);
            Assert.AreSame(expected, logger);
        }

        [TestMethod]
        public void SetLoggerTest3()
        {
            log4net.ILog logger = null;
            DotnetSpider.Monitor.Fakes.StubIRecordable rd = new Monitor.Fakes.StubIRecordable
            {
                LoggerGet = () => logger,
                LoggerSetILog = v => logger = v,
                NameGet = () => "name"
            };
            var expected = new log4net.Fakes.StubILog();
            DotnetSpider.Logger.Fakes.ShimLoggerCreator.GetLoggerType = t => 
            {
                Assert.Fail();
                return null;
            };
            DotnetSpider.Logger.Fakes.ShimLoggerCreator.GetLoggerString = n => expected;
            _private.Invoke("SetLogger", rd);
            Assert.AreSame(expected, logger);
        }

        [TestMethod]
        public void CheckConfigurationTest0()
        {
            _instance.Logger = new log4net.Fakes.StubILog()
            {
                FatalObject = o => Assert.Fail()
            };
            _instance.Scheduler = new DotnetSpider.Scheduler.Fakes.StubIScheduler();
            _instance.Downloader = new DotnetSpider.Downloader.Fakes.StubIDownloader();
            _instance.Pipelines.Add(new DotnetSpider.Pipeline.Fakes.StubIPipeline());
            _instance.PageProcessors.Add(new DotnetSpider.Processor.Fakes.StubIResponseProcessor());
            _instance.Parallels = 1;
            Assert.IsTrue((bool)_private.Invoke("CheckConfiguration"));
        }

        [TestMethod]
        public void CheckConfigurationTest1()
        {
            _instance.Parallels = 0;
            _instance.RequestInterval = TimeSpan.FromSeconds(10);
            _instance.FixedRequestDuration = TimeSpan.FromSeconds(10);
            Assert.IsFalse((bool)_private.Invoke("CheckConfiguration"));
        }

        [TestMethod]
        public void CheckConfigurationTest2()
        {
            int callTimes = 0;
            _instance.Logger = new log4net.Fakes.StubILog()
            {
                FatalObject = o =>
                {
                    ++callTimes;
                    Assert.AreEqual("Scheduler is null.", (string)o);
                }
            };
            //_instance.Scheduler = new DotnetSpider.Scheduler.Fakes.StubIScheduler();
            _instance.Downloader = new DotnetSpider.Downloader.Fakes.StubIDownloader();
            _instance.Pipelines.Add(new DotnetSpider.Pipeline.Fakes.StubIPipeline());
            _instance.PageProcessors.Add(new DotnetSpider.Processor.Fakes.StubIResponseProcessor());
            _instance.Parallels = 1;
            Assert.IsFalse((bool)_private.Invoke("CheckConfiguration"));
            Assert.AreEqual(1, callTimes);
        }

        [TestMethod]
        public void CheckConfigurationTest3()
        {
            int callTimes = 0;
            _instance.Logger = new log4net.Fakes.StubILog()
            {
                FatalObject = o =>
                {
                    ++callTimes;
                    Assert.AreEqual("Downloader is null.", (string)o);
                }
            };
            _instance.Scheduler = new DotnetSpider.Scheduler.Fakes.StubIScheduler();
            //_instance.Downloader = new DotnetSpider.Downloader.Fakes.StubIDownloader();
            _instance.Pipelines.Add(new DotnetSpider.Pipeline.Fakes.StubIPipeline());
            _instance.PageProcessors.Add(new DotnetSpider.Processor.Fakes.StubIResponseProcessor());
            _instance.Parallels = 1;
            Assert.IsFalse((bool)_private.Invoke("CheckConfiguration"));
            Assert.AreEqual(1, callTimes);
        }

        [TestMethod]
        public void CheckConfigurationTest4()
        {
            int callTimes = 0;
            _instance.Logger = new log4net.Fakes.StubILog()
            {
                FatalObject = o =>
                {
                    ++callTimes;
                    Assert.AreEqual("Pipelines are empty.", (string)o);
                }
            };
            _instance.Scheduler = new DotnetSpider.Scheduler.Fakes.StubIScheduler();
            _instance.Downloader = new DotnetSpider.Downloader.Fakes.StubIDownloader();
            //_instance.Pipelines.Add(new DotnetSpider.Pipeline.Fakes.StubIPipeline());
            _instance.PageProcessors.Add(new DotnetSpider.Processor.Fakes.StubIResponseProcessor());
            _instance.Parallels = 1;
            Assert.IsFalse((bool)_private.Invoke("CheckConfiguration"));
            Assert.AreEqual(1, callTimes);
        }

        [TestMethod]
        public void CheckConfigurationTest5()
        {
            int callTimes = 0;
            _instance.Logger = new log4net.Fakes.StubILog()
            {
                FatalObject = o =>
                {
                    ++callTimes;
                    Assert.AreEqual("PageProcessors are empty.", (string)o);
                }
            };
            _instance.Scheduler = new DotnetSpider.Scheduler.Fakes.StubIScheduler();
            _instance.Downloader = new DotnetSpider.Downloader.Fakes.StubIDownloader();
            _instance.Pipelines.Add(new DotnetSpider.Pipeline.Fakes.StubIPipeline());
            //_instance.PageProcessors.Add(new DotnetSpider.Processor.Fakes.StubIResponseProcessor());
            _instance.Parallels = 1;
            Assert.IsFalse((bool)_private.Invoke("CheckConfiguration"));
            Assert.AreEqual(1, callTimes);
        }

        [TestMethod]
        public void CheckConfigurationTest6()
        {
            int callTimes = 0;
            _instance.Logger = new log4net.Fakes.StubILog()
            {
                FatalObject = o =>
                {
                    ++callTimes;
                    Assert.AreEqual("Parallels is less than 1.", (string)o);
                }
            };
            _instance.Scheduler = new DotnetSpider.Scheduler.Fakes.StubIScheduler();
            _instance.Downloader = new DotnetSpider.Downloader.Fakes.StubIDownloader();
            _instance.Pipelines.Add(new DotnetSpider.Pipeline.Fakes.StubIPipeline());
            _instance.PageProcessors.Add(new DotnetSpider.Processor.Fakes.StubIResponseProcessor());
            _instance.Parallels = 0;
            Assert.IsFalse((bool)_private.Invoke("CheckConfiguration"));
            Assert.AreEqual(1, callTimes);
        }

        [TestMethod]
        public void CheckConfigurationTest7()
        {
            int callTimes = 0;
            _instance.Logger = new log4net.Fakes.StubILog()
            {
                FatalObject = o =>
                {
                    ++callTimes;
                    Assert.AreEqual("RequestInterval and FixedRequestDuration can not exist both.", (string)o);
                }
            };
            _instance.Scheduler = new DotnetSpider.Scheduler.Fakes.StubIScheduler();
            _instance.Downloader = new DotnetSpider.Downloader.Fakes.StubIDownloader();
            _instance.Pipelines.Add(new DotnetSpider.Pipeline.Fakes.StubIPipeline());
            _instance.PageProcessors.Add(new DotnetSpider.Processor.Fakes.StubIResponseProcessor());
            _instance.Parallels = 1;
            _instance.RequestInterval = TimeSpan.FromSeconds(10);
            _instance.FixedRequestDuration = TimeSpan.FromSeconds(10);
            Assert.IsFalse((bool)_private.Invoke("CheckConfiguration"));
            Assert.AreEqual(1, callTimes);
        }

        [TestMethod]
        public void CheckConfigurationTest8()
        {
            _instance.Logger = new log4net.Fakes.StubILog()
            {
                FatalObject = o => Assert.Fail()
            };
            _instance.Scheduler = new DotnetSpider.Scheduler.Fakes.StubIScheduler();
            _instance.Downloader = new DotnetSpider.Downloader.Fakes.StubIDownloader();
            _instance.Pipelines.Add(new DotnetSpider.Pipeline.Fakes.StubIPipeline());
            _instance.PageProcessors.Add(new DotnetSpider.Processor.Fakes.StubIResponseProcessor());
            _instance.Parallels = 1;
            _instance.RequestInterval = TimeSpan.FromSeconds(10);
            Assert.IsTrue((bool)_private.Invoke("CheckConfiguration"));
        }

        [TestMethod]
        public void CheckConfigurationTest9()
        {
            _instance.Logger = new log4net.Fakes.StubILog()
            {
                FatalObject = o => Assert.Fail()
            };
            _instance.Scheduler = new DotnetSpider.Scheduler.Fakes.StubIScheduler();
            _instance.Downloader = new DotnetSpider.Downloader.Fakes.StubIDownloader();
            _instance.Pipelines.Add(new DotnetSpider.Pipeline.Fakes.StubIPipeline());
            _instance.PageProcessors.Add(new DotnetSpider.Processor.Fakes.StubIResponseProcessor());
            _instance.Parallels = 1;
            _instance.FixedRequestDuration = TimeSpan.FromSeconds(10);
            Assert.IsTrue((bool)_private.Invoke("CheckConfiguration"));
        }
        #endregion

        #region 保护方法
        [TestMethod]
        public void AddSuccessTest()
        {
            _private.SetField("_countSuccess", 1);
            _private.Invoke("AddSuccess");
            Assert.AreEqual(2, (long)_private.GetField("_countSuccess"));
        }

        [TestMethod]
        public void AddStartingTest()
        {
            _private.SetField("_countStarted", 1);
            _private.Invoke("AddStarting");
            Assert.AreEqual(2, (long)_private.GetField("_countStarted"));
        }

        [TestMethod]
        public void AddFailedTest()
        {
            _private.SetField("_countFailed", 1);
            _private.Invoke("AddFailed");
            Assert.AreEqual(2, (long)_private.GetField("_countFailed"));
        }
        #endregion

        #region 私有方法
        [TestMethod]
        [Timeout(5000)]
        public void JudgeConditionThreadTest0()
        {
            _instance.ConditionOfStop = _ =>
            {
                Assert.Fail();
                return false;
            };
            _instanceShim.IsRunningGet = () => false;
            _instanceShim.Exit = () => 
            {
                Assert.Fail();
                return true;
            };
            _private.Invoke("JudgeConditionThread");
        }

        [TestMethod]
        [Timeout(5000)]
        public void JudgeConditionThreadTest1()
        {
            int callJudge = 0;
            int callExit = 0;
            _instance.ConditionOfStop = _ =>
            {
                ++callJudge;
                return true;
            };
            _instanceShim.IsRunningGet = () => true;
            _instanceShim.Exit = () =>
            {
                ++callExit;
                return true;
            };
            _private.Invoke("JudgeConditionThread");
            Assert.AreEqual(1, callJudge);
            Assert.AreEqual(1, callExit);
        }

        [TestMethod]
        [Timeout(5000)]
        public void JudgeConditionThreadTest2()
        {
            int callJudge = 0;
            int callExit = 0;
            _instance.ConditionOfStop = _ =>
            {
                if (++callJudge == 1)
                {
                    throw new Exception();
                }
                else
                {
                    return true;
                }
            };
            _instanceShim.IsRunningGet = () => true;
            _instanceShim.Exit = () =>
            {
                ++callExit;
                return true;
            };
            _private.Invoke("JudgeConditionThread");
            Assert.AreEqual(2, callJudge);
            Assert.AreEqual(1, callExit);
        }

        [TestMethod]
        [Timeout(5000)]
        public void JudgeConditionThreadTest3()
        {
            int callJudge = 0;
            int callExit = 0;
            int callLogger = 0;
            _instance.ConditionOfStop = _ =>
            {
                ++callJudge;
                switch (callJudge)
                {
                    case 1:
                        throw new Exception();
                    case 2:
                        return false;
                    default:
                        return true;
                }
            };
            _instanceShim.IsRunningGet = () => true;
            _instanceShim.Exit = () =>
            {
                ++callExit;
                return true;
            };
            _instance.Logger = new log4net.Fakes.StubILog()
            {
                ErrorObject = o =>
                {
                    ++callLogger;
                    StringAssert.StartsWith((string)o, "Exception ocurred when judge stop condition.Exception:");
                }
            };
            _private.Invoke("JudgeConditionThread");
            Assert.AreEqual(3, callJudge);
            Assert.AreEqual(1, callExit);
            Assert.AreEqual(1, callLogger);
        }
        #endregion

        #region 公共属性
        [TestMethod]
        public void MaxRetryTest()
        {
            Assert.AreEqual(2u, _instance.MaxRetry);
            _instance.MaxRetry = 3;
            Assert.AreEqual(3u, _instance.MaxRetry);
        }

        [TestMethod]
        public void RetryIntervalTest()
        {
            Assert.AreEqual(TimeSpan.FromSeconds(1), _instance.RetryInterval);
            _instance.RetryInterval = TimeSpan.FromSeconds(2);
            Assert.AreEqual(TimeSpan.FromSeconds(2), _instance.RetryInterval);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Spider", _instance.Name);
            _instance.Name = "HEHE";
            Assert.AreEqual("HEHE", _instance.Name);
        }

        [TestMethod]
        public void IsRunningTest0()
        {
            _private.SetField("_isRunning", false);
            _private.SetField("_hasExit", false);
            Assert.IsFalse(_instance.IsRunning);
        }

        [TestMethod]
        public void IsRunningTest1()
        {
            _private.SetField("_isRunning", true);
            _private.SetField("_hasExit", true);
            Assert.IsFalse(_instance.IsRunning);
        }

        [TestMethod]
        public void IsRunningTest2()
        {
            _private.SetField("_isRunning", true);
            _private.SetField("_hasExit", false);
            Assert.IsTrue(_instance.IsRunning);
        }

        [TestMethod]
        public void IsRunningTest3()
        {
            _private.SetField("_isRunning", false);
            _private.SetField("_hasExit", true);
            Assert.IsFalse(_instance.IsRunning);
        }

        [TestMethod]
        public void HasExitTest()
        {
            Assert.IsFalse(_instance.HasExit);
            _private.SetField("_hasExit", true);
            Assert.IsTrue(_instance.HasExit);
        }

        [TestMethod]
        public void HasStartedTest()
        {
            Assert.IsFalse(_instance.HasStarted);
            _private.SetField("_hasStarted", true);
            Assert.IsTrue(_instance.HasStarted);
        }

        [TestMethod]
        public void SuccessTest()
        {
            Assert.AreEqual<long>(0, _instance.Success);
            _private.SetField("_countSuccess", 1);
            Assert.AreEqual<long>(1, _instance.Success);
        }

        [TestMethod]
        public void FailedTest()
        {
            Assert.AreEqual<long>(0, _instance.Failed);
            _private.SetField("_countFailed", 1);
            Assert.AreEqual<long>(1, _instance.Failed);
        }

        [TestMethod]
        public void FinishedTest()
        {
            Assert.AreEqual<long>(0, _instance.Finished);
            _private.SetField("_countSuccess", 1);
            _private.SetField("_countFailed", 2);
            Assert.AreEqual<long>(3, _instance.Finished);
        }


        [TestMethod]
        public void StartedTest()
        {
            Assert.AreEqual<long>(0, _instance.Started);
            _private.SetField("_countStarted", 1);
            Assert.AreEqual<long>(1, _instance.Started);
        }


        [TestMethod]
        public void UnfinishedTest()
        {
            Assert.AreEqual<long>(0, _instance.Unfinished);
            _private.SetField("_countStarted", 5);
            _private.SetField("_countSuccess", 1);
            _private.SetField("_countFailed", 2);
            Assert.AreEqual<long>(2, _instance.Unfinished);
        }
        #endregion
    }
}