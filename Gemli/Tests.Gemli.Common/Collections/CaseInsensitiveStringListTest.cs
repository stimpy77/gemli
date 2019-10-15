using Gemli.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Gemli.Tests.Core.Collections
{
    /// <summary>
    ///This is a test class for CaseInsensitiveStringListTest and is intended
    ///to contain all CaseInsensitiveStringListTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CaseInsensitiveStringListTest
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
        ///A test for Contains
        ///</summary>
        [TestMethod()]
        public void ContainsTest()
        {
            var target = new CaseInsensitiveStringList();
            string value = "abc";
            target.Add("ABC");
            bool expected = true; 
            bool actual;
            actual = target.Contains(value);
            Assert.AreEqual(expected, actual);
            
        }

        /// <summary>
        ///A test for Contains
        ///</summary>
        [TestMethod()]
        public void ContainsTest2()
        {
            var target = new CaseInsensitiveStringList();
            string value = "abc";
            target.Add("xyz");
            bool expected = false;
            bool actual;
            actual = target.Contains(value);
            Assert.AreEqual(expected, actual);


        }

        /// <summary>
        ///A test for Contains
        ///</summary>
        [TestMethod()]
        public void ContainsTest3()
        {
            var target = new CaseInsensitiveStringList();
            string value = "abc";
            target.Add("abc");
            bool expected = true;
            bool actual;
            actual = target.Contains(value);
            Assert.AreEqual(expected, actual);


        }

        [TestMethod()]
        public void RemoveTest()
        {
            var target = new CaseInsensitiveStringList();
            var startval = "abc";
            target.Add(startval);
            bool pass = false;
            try
            {
                target.Remove("abc");
                pass = true;
            }
            catch { }
            Assert.IsTrue(pass && !((List<string>)target).Contains(startval));
        }
        [TestMethod()]
        public void RemoveTest2()
        {
            var target = new CaseInsensitiveStringList();
            var startval = "abc";
            target.Add(startval);
            bool pass = false;
            try
            {
                target.Remove("ABC");
                pass = true;
            }
            catch { }
            Assert.IsTrue(pass && !((List<string>)target).Contains(startval));
        }
        [TestMethod()]
        public void RemoveTest3()
        {
            var target = new CaseInsensitiveStringList();
            var startval = "abc";
            target.Add(startval);
            bool pass = false;
            try
            {
                target.Remove("XYZ");
                pass = true;
            } catch {}
            Assert.IsTrue(!pass && ((List<string>)target).Contains(startval));
        }
    }
}