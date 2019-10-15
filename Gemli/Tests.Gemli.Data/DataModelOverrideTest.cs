using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Gemli.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Gemli.Data
{
    /// <summary>
    /// Summary description for DataModelOverrideWrapperTest
    /// </summary>
    [TestClass]
    public class DataModelOverrideTest
    {
        public DataModelOverrideTest()
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

        public class MyMock
        {
            public virtual int IntValue { get; set;}
        }

        [DataModelTable(Table = "overridden_table")]
        public class MyMockOverride : MyMock
        {
            //(this works too ...)
            //public MyMockOverride()
            //{
            //    var mapping = DataModelMap.GetEntityMapping(this.GetType());
            //    mapping.TableMapping.Table = "overridden_table";
            //    mapping.ColumnMappings["IntValue"].ColumnName = "intvalue2";
            //}

            [DataModelColumn(ColumnName = "intvalue2")]
            public override int IntValue
            {
                get
                {
                    return base.IntValue;
                }
                set
                {
                    base.IntValue = value;
                }
            }
        }

        [TestMethod]
        public void RunDataModelOverrideWrapperTest()
        {
            var myMock = new MyMock();
            var myMockDataModel = new DataModel<MyMock>(myMock);
            var myMockOverride = new MyMockOverride();
            var myMockOverrideDataModel = new DataModel<MyMockOverride>(myMockOverride);
            var origMapping = DataModelMap.GetEntityMapping(myMockDataModel.GetType());
            var overriddenMapping = DataModelMap.GetEntityMapping(myMockOverrideDataModel.GetType());
            Assert.IsTrue(overriddenMapping.FieldMappings["IntValue"].ColumnName == "intvalue2");
            Assert.IsTrue(origMapping.FieldMappings["IntValue"].ColumnName == "IntValue");
            Assert.IsTrue(overriddenMapping.FieldMappings["IntValue"].ColumnName == "intvalue2");
            Assert.IsTrue(origMapping.TableMapping.Table == "MyMock");
            Assert.IsTrue(overriddenMapping.TableMapping.Table == "overridden_table");
        }
    }
}
