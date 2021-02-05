using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Common.Tests
{
    [TestClass()]
    public class HashCodeTests
    {
        [TestMethod()]
        public void GetHashCodeTest0()
        {
            int expected = 0;
            int actual = HashCodeExtend.GetHashCode(0, 0);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetHashCodeTest1()
        {
            int expected = 84696351;
            int actual = HashCodeExtend.GetHashCode(HashCodeExtend.BeginCode, null);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetHashCodeTest2()
        {
            int expected = 1539727482;
            int actual = HashCodeExtend.GetHashCode(HashCodeExtend.BeginCode, new Dictionary<int, int>() { { 0, 0} });
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetHashCodeTest3()
        {
            int expected = -2079276838;
            int actual = HashCodeExtend.GetHashCode(HashCodeExtend.BeginCode, new Dictionary<int, int>());
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetHashCodeTest4()
        {
            int expected = 84696351;
            int actual = HashCodeExtend.GetHashCode(HashCodeExtend.BeginCode, (IDictionary<int, int>)null);
            Assert.AreEqual(expected, actual);
        }
    }
}