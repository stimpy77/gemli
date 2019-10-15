using System.Collections.Generic;
using Gemli.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gemli.Tests.Core.Collections
{
    /// <summary>
    ///This is a test class for CaseInsensitiveDictionaryTest and is intended
    ///to contain all CaseInsensitiveDictionaryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CaseInsensitiveDictionaryTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Item
        ///</summary>
        public void ItemTestHelper<TValue>()
        {
            var target = new CaseInsensitiveDictionary<TValue>();
            string key = "a";
            string ukey = "A";
            target.Add(key, default(TValue));
            TValue expected = default(TValue);
            TValue actual;
            target[key] = expected;
            actual = target[ukey];
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ItemTest()
        {
            ItemTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Remove
        ///</summary>
        public void RemoveTestHelper<TValue>()
        {
            var target = new CaseInsensitiveDictionary<TValue>();
            var key = "a";
            TValue value = default(TValue);
            target.Add(key, value);
            target.Remove(key);
            Assert.IsFalse(((Dictionary<string, TValue>)target).ContainsKey(key));
        }

        [TestMethod()]
        public void RemoveTest()
        {
            RemoveTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Remove
        ///</summary>
        public void RemoveTest2Helper<TValue>()
        {
            var target = new CaseInsensitiveDictionary<TValue>();
            var key = "a";
            var ukey = "A";
            TValue value = default(TValue);
            target.Add(key, value);
            target.Remove(ukey);
            Assert.IsFalse(((Dictionary<string, TValue>)target).ContainsKey(key));
        }

        [TestMethod()]
        public void RemoveTest2()
        {
            RemoveTest2Helper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Remove
        ///</summary>
        public void RemoveTest3Helper<TValue>()
        {
            var target = new CaseInsensitiveDictionary<TValue>();
            var key = "a";
            var zkey = "z";
            TValue value = default(TValue);
            target.Add(key, value);
            try
            {
                target.Remove(zkey);
            } catch {}
            Assert.IsTrue(((Dictionary<string, TValue>)target).ContainsKey(key));
        }

        [TestMethod()]
        public void RemoveTest3()
        {
            RemoveTest3Helper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for ContainsKey
        ///</summary>
        public void ContainsKeyTestHelper<TValue>()
        {
            var target = new CaseInsensitiveDictionary<TValue>();
            string key = "a"; 
            target.Add(key, default(TValue));
            bool actual;
            actual = target.ContainsKey(key);
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void ContainsKeyTest()
        {
            ContainsKeyTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for ContainsKey
        ///</summary>
        public void ContainsKeyTest2Helper<TValue>()
        {
            var target = new CaseInsensitiveDictionary<TValue>();
            string key = "a";
            string ukey = "A";
            target.Add(key, default(TValue));
            bool actual;
            actual = target.ContainsKey(ukey);
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void ContainsKeyTest2()
        {
            ContainsKeyTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for ContainsKey
        ///</summary>
        public void ContainsKey3TestHelper<TValue>()
        {
            var target = new CaseInsensitiveDictionary<TValue>();
            string key = "a";
            string zkey = "z";
            target.Add(key, default(TValue));
            bool actual;
            actual = target.ContainsKey(zkey);
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void ContainsKey3Test()
        {
            ContainsKeyTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for CaseInsensitiveDictionary`1 Constructor
        ///</summary>
        public void CaseInsensitiveDictionaryConstructorTestHelper<TValue>()
        {
            var target = new CaseInsensitiveDictionary<TValue>();
            Assert.IsTrue(target.Count==0);
        }

        [TestMethod()]
        public void CaseInsensitiveDictionaryConstructorTest()
        {
            CaseInsensitiveDictionaryConstructorTestHelper<GenericParameterHelper>();
        }
    }
}