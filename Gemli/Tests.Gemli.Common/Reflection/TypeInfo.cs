using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gemli.Reflection;

namespace Tests.Gemli.Common.Reflection
{
    /// <summary>
    /// Summary description for TypeInfo
    /// </summary>
    [TestClass]
    public class TypeInfo
    {
        public TypeInfo()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ObjectIsObjectTest()
        {
            Assert.IsTrue(typeof(object).IsOrInherits(typeof(object)));
        }

        [TestMethod]
        public void ObjectIsNotStringTest()
        {
            Assert.IsFalse(typeof(object).IsOrInherits(typeof(string)));
        }

        [TestMethod]
        public void StringIsObjectTest()
        {
            Assert.IsTrue(typeof(string).IsOrInherits(typeof(object)));
        }

        public class Foo {}

        [TestMethod]
        public void FooIsObjectTest()
        {
            Assert.IsTrue(typeof(Foo).IsOrInherits(typeof(object)));
        }

        [TestMethod]
        public void ObjectIsNotFooTest()
        {
            Assert.IsFalse(typeof(object).IsOrInherits(typeof(Foo)));
        }
    }
}
