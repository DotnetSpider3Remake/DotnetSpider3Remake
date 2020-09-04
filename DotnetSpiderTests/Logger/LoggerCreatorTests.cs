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

        [TestInitialize]
        public void InitTest()
        {
            _shimsContext = ShimsContext.Create();

            bool fileExist = false;
            log4net.Fakes.ShimLogManager.GetCurrentLoggersAssembly = _ => new ILog[0];
            log4net.Repository.Fakes.StubILoggerRepository repository = new log4net.Repository.Fakes.StubILoggerRepository
            {
                GetAppenders = () =>
                {
                    if (fileExist)
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
            log4net.Config.Fakes.ShimXmlConfigurator.ConfigureILoggerRepositoryFileInfo = (rep, fi) => 
            {
                fileExist = ReferenceEquals(rep, repository) && fi.Exists;
                return null;
            };
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
            PrivateType type = new PrivateType(typeof(LoggerCreator));
            FileInfo orginalPath = (FileInfo)type.GetStaticField("_fiDefaultConfig");
            try
            {
                type.SetStaticField("_fiDefaultConfig", new FileInfo("notexsit.notexsit"));
                var logger = LoggerCreator.GetLogger("test");
                Assert.IsNull(logger);
            }
            finally
            {
                type.SetStaticField("_fiDefaultConfig", orginalPath);
            }
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
            PrivateType type = new PrivateType(typeof(LoggerCreator));
            FileInfo orginalPath = (FileInfo)type.GetStaticField("_fiDefaultConfig");
            try
            {
                type.SetStaticField("_fiDefaultConfig", new FileInfo("notexsit.notexsit"));
                var logger = LoggerCreator.GetLogger(typeof(string));
                Assert.IsNull(logger);
            }
            finally
            {
                type.SetStaticField("_fiDefaultConfig", orginalPath);
            }
        }
    }
}