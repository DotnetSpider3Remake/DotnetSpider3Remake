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
        #region AreEquivalentIDictionary

        [TestMethod()]
        public void AreEquivalentIDictionaryTest0()
        {
            Assert.IsTrue(Comparaor.AreEquivalent<string, object>(null, null));
        }

        [TestMethod()]
        public void AreEquivalentIDictionaryTest1()
        {
            Assert.IsFalse(Comparaor.AreEquivalent(null, new Dictionary<int, int>()));
        }

        [TestMethod()]
        public void AreEquivalentIDictionaryTest2()
        {
            Assert.IsFalse(Comparaor.AreEquivalent(new Dictionary<int, int>(), null));
        }

        [TestMethod()]
        public void AreEquivalentIDictionaryTest3()
        {
            Assert.IsTrue(Comparaor.AreEquivalent(new Dictionary<int, int>() { { 0, 0 } }, new Dictionary<int, int>() { { 0, 0 } }));
        }

        [TestMethod()]
        public void AreEquivalentIDictionaryTest4()
        {
            Assert.IsTrue(Comparaor.AreEquivalent(new Dictionary<int, int>() { { 0, 0 } }, new SortedList<int, int>() { { 0, 0 } }));
        }

        [TestMethod()]
        public void AreEquivalentIDictionaryTest5()
        {
            Assert.IsFalse(Comparaor.AreEquivalent(new Dictionary<int, int>() { { 0, 0 }, { 1, 1 } }, new Dictionary<int, int>() { { 0, 0 } }));
        }

        [TestMethod()]
        public void AreEquivalentIDictionaryTest6()
        {
            var dic = new Dictionary<int, int>() { { 0, 0 } };
            Assert.IsTrue(Comparaor.AreEquivalent(dic, dic));
        }

        [TestMethod()]
        public void AreEquivalentIDictionaryTest7()
        {
            var a = new Dictionary<int, int>() { { 0, 0 } };
            var b = new Dictionary<int, int>() { { 0, 1 } };
            Assert.IsFalse(Comparaor.AreEquivalent(a, b));
        }

        [TestMethod()]
        public void AreEquivalentIDictionaryTest8()
        {
            var a = new Dictionary<int, int>() { { 0, 0 } };
            var b = new Dictionary<int, int>() { { 1, 0 } };
            Assert.IsFalse(Comparaor.AreEquivalent(a, b));
        }

        #endregion

        #region AreEquivalentIList

        [TestMethod()]
        public void AreEquivalentIListTest0()
        {
            List<int> a = new List<int> { };
            List<int> b = new List<int> { };
            Assert.IsTrue(Comparaor.AreEquivalent(a, b));
        }

        [TestMethod()]
        public void AreEquivalentIListTest1()
        {
            List<int> a = null;
            List<int> b = null;
            Assert.IsTrue(Comparaor.AreEquivalent(a, b));
        }

        [TestMethod()]
        public void AreEquivalentIListTest2()
        {
            List<int> a = new List<int> { };
            List<int> b = null;
            Assert.IsFalse(Comparaor.AreEquivalent(a, b));
        }

        [TestMethod()]
        public void AreEquivalentIListTest3()
        {
            List<int> a = null;
            List<int> b = new List<int> { };
            Assert.IsFalse(Comparaor.AreEquivalent(a, b));
        }

        [TestMethod()]
        public void AreEquivalentIListTest4()
        {
            List<int> a = new List<int> { };
            List<int> b = a;
            Assert.IsTrue(Comparaor.AreEquivalent(a, b));
        }

        [TestMethod()]
        public void AreEquivalentIListTest5()
        {
            List<int> a = new List<int> { };
            List<int> b = new List<int> { 1 };
            Assert.IsFalse(Comparaor.AreEquivalent(a, b));
        }

        [TestMethod()]
        public void AreEquivalentIListTest6()
        {
            List<int> a = new List<int> { 0, 0 };
            List<int> b = new List<int> { 0, 1 };
            Assert.IsFalse(Comparaor.AreEquivalent(a, b));
        }

        #endregion
    }
}