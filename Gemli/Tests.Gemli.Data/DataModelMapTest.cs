using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Gemli.Collections;
using Gemli.Data;
using Gemli.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Gemli.Data
{
    /// <summary>
    /// Tests for working with <see cref="DataModelMap"/>
    /// </summary>
    [TestClass]
    public class DataModelMapTest
    {
        public DataModelMapTest()
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
        public void BasicXmlSerializationTest()
        {
            var m = DataModel.GetMapping<DataModelFieldMappingAttributeTest.MockDataModel>();
            var ss = new XmlSerialized<DataModelMap>(m);
            Assert.IsTrue(ss.SerializedValue.Length > 0);
            m = ss.Deserialize();
            Assert.IsNotNull(m["Field"]);
        }

        [TestMethod]
        public void BasicDataModelMappingsCollectionSerializationTest()
        {
            var mi = DataModelMap.GetEntityMapping(typeof(DataModelFieldMappingAttributeTest.MockDataModel));
            var mis = DataModelMap.MapItems;
            var xs = new XmlSerialized<DataModelMappingsDefinition>(mis);
            Assert.IsNotNull(xs);
            Assert.IsTrue(xs.SerializedValue.Length > 0);
            mis = xs.Deserialize();
            mi = mis[typeof (DataModelFieldMappingAttributeTest.MockDataModel)];
            Assert.IsNotNull(mi);
            Assert.IsTrue(mi.FieldMappings.Count > 0);
        }
    }
}
