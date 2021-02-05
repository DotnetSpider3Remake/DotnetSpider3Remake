using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Common.Tests
{
    [TestClass()]
    public class DictionaryExtendTests
    {
        [TestMethod()]
        public void GetValueOrDefaultTest0()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            Assert.AreEqual(0, dic.GetValueOrDefault("1"));
        }

        [TestMethod()]
        public void GetValueOrDefaultTest1()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>()
            {
                { "1", 1 }
            };
            Assert.AreEqual(1, dic.GetValueOrDefault("1"));
        }
        [TestMethod()]
        public void GetValueOrDefaultTest2()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            Assert.AreEqual(5, dic.GetValueOrDefault("1", 5));
        }

        [TestMethod()]
        public void GetValueOrDefaultTest3()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>()
            {
                { "1", 1 }
            };
            Assert.AreEqual(1, dic.GetValueOrDefault("1", 5));
        }
    }
}