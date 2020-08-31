using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Logger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using log4net;
using System.Threading;

namespace DotnetSpider.Logger.Tests
{
    [TestClass()]
    public class LoggerCreatorTests
    {
        [TestMethod()]
        public void GetLoggerTest0()
        {
            var logger = LoggerCreator.GetLogger("test");
            Assert.IsNotNull(logger);
            Assert.IsNotNull(logger.Logger);
            Assert.AreEqual("test", logger.Logger.Name);
            Assert.AreEqual(1, logger.Logger.Repository.GetAppenders().Length);
        }

        [TestMethod()]
        public void GetLoggerTest1()
        {
            var logger = LoggerCreator.GetLogger(GetType());
            Assert.IsNotNull(logger);
            Assert.IsNotNull(logger.Logger);
            Assert.AreEqual(GetType().Name, logger.Logger.Name);
            Assert.AreEqual(1, logger.Logger.Repository.GetAppenders().Length);
        }
    }
}