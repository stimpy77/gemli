using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gemli.Data;

namespace Tests.Gemli.Data
{
    [TestClass]
    public class DataModelQueryTest
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

        [DataModelTable(Table="mock_table")]
        public class MockObject
        {
            [DataModelColumn("string_value")]
            public string MockStringValue { get; set; }
        }

        [TestMethod]
        public void GetTypedQueryTest()
        {
            var myCustomEntityQuery = new DataModelQuery<DataModel<MockObject>>();
            Assert.IsNotNull(myCustomEntityQuery);
            Assert.AreEqual(myCustomEntityQuery.GetType(), 
                typeof(DataModelQuery<DataModel<MockObject>>));
        }

        [TestMethod]
        public void GetTypedQueryTest2()
        {
            var myCustomEntityQuery = DataModel<MockObject>.NewQuery();
            Assert.IsNotNull(myCustomEntityQuery);
            Assert.AreEqual(myCustomEntityQuery.GetType(),
                typeof(DataModelQuery<DataModel<MockObject>>));
        }

        [TestMethod]
        public void SetQueryConditionTest()
        {
            var myCustomEntityQuery = new DataModelQuery<DataModel<MockObject>>();
            myCustomEntityQuery.WhereProperty["ID"].IsEqualTo(3);
            Assert.AreEqual(myCustomEntityQuery.Conditions[0].CompareOp, Compare.Equal);
            Assert.AreEqual(myCustomEntityQuery.Conditions[0].CompareValue, 3);
            Assert.AreEqual(myCustomEntityQuery.Conditions[0].EvalSubject, "ID");
        }

        [TestMethod]
        public void SetQueryConditionTest2()
        {
            var myCustomEntityQuery = new DataModelQuery<DataModel<MockObject>>();
            myCustomEntityQuery.WhereColumn["customentity_id"].IsGreaterThan(-1);
            Assert.AreEqual(myCustomEntityQuery.Conditions[0].CompareOp, Compare.GreaterThan);
            Assert.AreEqual(myCustomEntityQuery.Conditions[0].CompareValue, -1);
            Assert.AreEqual(myCustomEntityQuery.Conditions[0].EvalSubject, "customentity_id");
        }

        [TestMethod]
        public void SetQueryConditionTest3()
        {
            var myCustomEntityQuery = new DataModelQuery<DataModel<MockObject>>()
                .WhereProperty["ID"] == (object)3;
            Assert.AreEqual(myCustomEntityQuery.Conditions[0].CompareOp, Compare.Equal);
            Assert.AreEqual(myCustomEntityQuery.Conditions[0].CompareValue, 3);
            Assert.AreEqual(myCustomEntityQuery.Conditions[0].EvalSubject, "ID");
        }

        [TestMethod]
        public void QueryConditionsImplementedTest()
        {
            var vals = (Compare[])Enum.GetValues(typeof(Compare));
            Assert.IsNotNull(vals);
            Assert.IsTrue(vals.Length > 0);
            var qc = new DataModelQueryCondition<DataModel<MockObject>>(
                FieldMappingKeyType.ClrMember, null);
            foreach (var compare in vals)
            {
                string methodName = "Is" + compare.ToString()
                    .Replace("Equals", "Equal");
                switch (methodName)
                {
                    case "IsEqual":
                    case "IsNotEqual":
                    case "IsGreaterThanOrEqual":
                    case "IsLessThanOrEqual":
                        methodName += "To";
                        break;
                }
                var mi = typeof(DataModelQueryCondition<DataModel<MockObject>>)
                    .GetMethod(methodName);
                Assert.IsNotNull(mi, "DataModelQueryCondition." 
                    + methodName + "() is not implemented.");
                if (mi.GetParameters().Length == 1)
                {
                    mi.Invoke(qc, new object[] {"3"});
                    Assert.AreEqual(qc.CompareOp, compare);
                }
            }
        }
        
    }
}
