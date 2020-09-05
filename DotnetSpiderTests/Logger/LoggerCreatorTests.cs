using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Logger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using log4net;
using System.Threading;
using Microsoft.QualityTools.Testing.Fakes;
using System.IO;

namespace DotnetSpider.Logger.Tests
{
    [TestClass()]
    public class LoggerCreatorTests
    {
        private IDisposable _shimsContext = null;
        private bool _configExist = true;

        [TestInitialize]
        public void InitTest()
        {
            _shimsContext = ShimsContext.Create();

            log4net.Fakes.ShimLogManager.GetCurrentLoggersAssembly = _ => new ILog[0];
            log4net.Repository.Fakes.StubILoggerRepository repository = new log4net.Repository.Fakes.StubILoggerRepository
            {
                GetAppenders = () =>
                {
                    if (_configExist)
                    {
                        return new log4net.Appender.IAppender[] { new log4net.Appender.Fakes.StubIAppender() };
                    }
                    else
                    {
                        return new log4net.Appender.IAppender[0];
                    }
                }
            };
            log4net.Fakes.ShimLogManager.GetRepositoryAssembly = _ => repository;
            log4net.Config.Fakes.ShimXmlConfigurator.ConfigureILoggerRepositoryFileInfo = (rep, fi) => null;
            log4net.Core.Fakes.StubILogger logger = new log4net.Core.Fakes.StubILogger
            {
                RepositoryGet = () => repository
            };
            log4net.Fakes.StubILog log = new log4net.Fakes.StubILog
            {
                LoggerGet = () => logger
            };
            log4net.Fakes.ShimLogManager.GetLoggerAssemblyString = (_, name) =>
            {
                logger.NameGet = () => name;
                return log;
            };
        }

        [TestCleanup]
        public void CleanTest()
        {
            _shimsContext?.Dispose();
            _shimsContext = null;
            _configExist = true;
        }

        [TestMethod()]
        public void GetLoggerStringTest0()
        {
            var logger = LoggerCreator.GetLogger("test");
            Assert.IsNotNull(logger);
            Assert.IsNotNull(logger.Logger);
            Assert.AreEqual("test", logger.Logger.Name);
            Assert.AreEqual(1, logger.Logger.Repository.GetAppenders().Length);
        }

        [TestMethod()]
        public void GetLoggerStringTest1()
        {
            _configExist = false;
            var logger = LoggerCreator.GetLogger("test");
            Assert.IsNull(logger);
        }


        [TestMethod()]
        public void GetLoggerTypeTest0()
        {
            var logger = LoggerCreator.GetLogger(typeof(string));
            Assert.IsNotNull(logger);
            Assert.IsNotNull(logger.Logger);
            Assert.AreEqual(typeof(string).Name, logger.Logger.Name);
            Assert.AreEqual(1, logger.Logger.Repository.GetAppenders().Length);
        }

        [TestMethod()]
        public void GetLoggerTypeTest1()
        {
            _configExist = false;
            var logger = LoggerCreator.GetLogger(typeof(string));
            Assert.IsNull(logger);
        }
    }
}