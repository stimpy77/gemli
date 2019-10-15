using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gemli.Data;

namespace Tests.Gemli.Data
{
    [TestClass]
    public class DataModelTableMappingAttributeTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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

        [DataModelTable]
        public class MockDataModel : DataModel
        {
            
        }

        [DataModelTable("abc")]
        public class MockDataModel2 : DataModel
        {

        }

        public class MockDataModel3 : MockDataModel
        {
            
        }

        public class MockDataModel4 : MockDataModel2
        {

        }

        [DataModelTable(Schema = "xxx")]
        public class MockDataModel5 : MockDataModel4
        {

        }

        [DataModelTable(Schema = "def", ClearBaseObjectMapping = true)]
        public class MockDataModel6 : MockDataModel5
        {

        }

        [TestMethod]
        public void LoadDataModelTableMapping_DefaultTableNameTest()
        {
            var target = new MockDataModel();
            Assert.AreEqual("MockDataModel", target.EntityMappings.TableMapping.Table);
            Assert.AreEqual("dbo", target.EntityMappings.TableMapping.Schema);
        }

        [TestMethod]
        public void LoadDataModelTableMapping_DefaultTableNameTest2()
        {
            var target = new MockDataModel3();
            Assert.AreEqual("MockDataModel3", target.EntityMappings.TableMapping.Table);
            Assert.AreEqual("dbo", target.EntityMappings.TableMapping.Schema);
        }

        [TestMethod]
        public void LoadDataModelTableMapping_AssignedTableNameTest()
        {
            var target = new MockDataModel2();
            Assert.AreEqual("abc", target.EntityMappings.TableMapping.Table);
            Assert.AreEqual("dbo", target.EntityMappings.TableMapping.Schema);
        }

        [TestMethod]
        public void LoadDataModelTableMapping_InheritTableNameTest()
        {
            var target = new MockDataModel4();
            Assert.AreEqual("abc", target.EntityMappings.TableMapping.Table);
            Assert.AreEqual("dbo", target.EntityMappings.TableMapping.Schema);
        }

        [TestMethod]
        public void LoadDataModelTableMapping_InheritTableNameTest2()
        {
            var target = new MockDataModel5();
            Assert.AreEqual("abc", target.EntityMappings.TableMapping.Table);
            Assert.AreEqual("xxx", target.EntityMappings.TableMapping.Schema);
        }

        [TestMethod]
        public void LoadDataModelTableMapping_ClearInherits()
        {
            var target = new MockDataModel6();
            Assert.AreEqual("MockDataModel6", target.EntityMappings.TableMapping.Table);
            Assert.AreEqual("def", target.EntityMappings.TableMapping.Schema);
        }
    }
}