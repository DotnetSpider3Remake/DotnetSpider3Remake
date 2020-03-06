using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Common.Tests
{
    [TestClass()]
    public class ComparaorTests
    {
        #region AreEquivalent

        [TestMethod()]
        public void AreEquivalentTest0()
        {
            Assert.IsTrue(Comparaor.AreEquivalent<string, object>(null, null));
        }

        [TestMethod()]
        public void AreEquivalentTest1()
        {
            Assert.IsFalse(Comparaor.AreEquivalent(null, new Dictionary<int, int>()));
        }

        [TestMethod()]
        public void AreEquivalentTest2()
        {
            Assert.IsFalse(Comparaor.AreEquivalent(new Dictionary<int, int>(), null));
        }

        [TestMethod()]
        public void AreEquivalentTest3()
        {
            Assert.IsTrue(Comparaor.AreEquivalent(new Dictionary<int, int>() { { 0, 0 } }, new Dictionary<int, int>() { { 0, 0 } }));
        }

        [TestMethod()]
        public void AreEquivalentTest4()
        {
            Assert.IsTrue(Comparaor.AreEquivalent(new Dictionary<int, int>() { { 0, 0 } }, new SortedList<int, int>() { { 0, 0 } }));
        }

        [TestMethod()]
        public void AreEquivalentTest5()
        {
            Assert.IsFalse(Comparaor.AreEquivalent(new Dictionary<int, int>() { { 0, 0 }, { 1, 1 } }, new Dictionary<int, int>() { { 0, 0 } }));
        }

        #endregion
    }
}