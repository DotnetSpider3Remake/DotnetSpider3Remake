using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Runner.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Runner.Helper.Tests
{
    [TestClass()]
    public class BaseZeroDisposableHelperTests
    {
        [TestMethod()]
        public void DisposeTest0()
        {
            using (new BaseZeroDisposableHelper())
            {

            }
        }

        [TestMethod()]
        public void DisposeTest1()
        {
            int i = 0;
            var helper = new BaseZeroDisposableHelper
            {
                Leave = () => ++i
            };
            helper.Dispose();
            Assert.AreEqual(1, i);
            helper.Dispose();
            Assert.AreEqual(1, i);
        }
    }
}