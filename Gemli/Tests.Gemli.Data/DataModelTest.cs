using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Gemli.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Gemli.Data
{
    [TestClass]
    public class DataModelTest
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

        public class MockDataModel : DataModel
        {
            [DataModelColumn(IsIdentity = true)]
            public int ID
            {
                get { return (int) base["ID"]; }
                set { base["ID"] = value;}
            }

            [DataModelColumn("SV")]
            public string StringValue
            {
                get { return (string) base["StringValue"]; }
                set { base["StringValue"] = value; }
            }

            [DataModelColumn]
            public decimal DecimalValue
            {
                get { return (decimal)base["DecimalValue"]; }
                set { base["DecimalValue"] = value; }
            }

            [DataModelColumn]
            public decimal? NullableDecimalValue
            {
                get { return (decimal?)base["NullableDecimalValue"]; }
                set { base["NullableDecimalValue"] = value; }
            }
        }

        public class MockPocoDataModel
        {
            public int ID { get; set; }

            public string StringValue;

            public decimal DecimalValue { get; set; }

            public decimal? NullableDecimalValue { get; set; }
        }

        //public class MockRelationshipEntity : DataModel
        //{
        //    [DataModelRelationship(typeof(MockDataModel))]
        //    public MockDataModel VagueRelationship { get; set; }

        //    [DataModelRelationship(typeof(MockDataModel), Relation.OneToOne)]
        //    public MockDataModel OneToOneRelationship { get; set; }

        //    [DataModelRelationship(typeof(MockDataModel), Relation.ManyToOne)]
        //    public MockDataModel ManyToOneRelationship { get; set; }

        //    [DataModelRelationship(typeof(MockDataModel), Relation.OneToMany)]
        //    public DataModelCollection<MockDataModel> OneToManyRelationship { get; set; }
        //}

        [TestMethod]
        public void InnerDictionaryTest()
        {
            var target = new MockDataModel();
            var value = "abc";
            target.StringValue = value;
            Assert.AreSame(value, target.StringValue);
        }

        [TestMethod]
        public void InnerDictionaryTest2()
        {
            var target = new MockDataModel();
            var value = 3m;
            target.DecimalValue = value;
            Assert.AreEqual(value, target.DecimalValue);
        }

        [TestMethod]
        public void TargetInfoTest()
        {
            var target = new MockDataModel();
            Assert.AreEqual(target.EntityMappings.FieldMappings["StringValue"]
                                .ColumnName, "SV");
        }

        [TestMethod]
        public void TargetInfoTest2()
        {
            var target = new MockDataModel();
            Assert.AreEqual(target.EntityMappings.FieldMappings["StringValue"]
                                .TargetMemberType, typeof(string));
        }

        [TestMethod]
        public void MemberTypeDefaultValueTest()
        {
            var target = new MockDataModel();
            Assert.AreEqual(target.EntityMappings.FieldMappings["DecimalValue"]
                                .DefaultValue, 0m);
        }

        [TestMethod]
        public void MemberTypeDefaultValueTest2()
        {
            var target = new MockDataModel();
            Assert.AreEqual(target.EntityMappings.FieldMappings["NullableDecimalValue"]
                                .DefaultValue, null);
        }

        [TestMethod]
        public void ToDataRowTest()
        {
            var target = new MockDataModel();
            target.StringValue = "abc";
            var dr = target.Convert.ToDataRow();
            Assert.AreEqual(dr["sv"], "abc");
        }

        [TestMethod]
        public void PocoToDataRowTest()
        {
            var pretarget = new MockPocoDataModel();
            pretarget.StringValue = "abc";
            var target = new DataModel<MockPocoDataModel>(pretarget);
            var dr = target.Convert.ToDataRow();
            Assert.AreEqual(dr["StringValue"], "abc");
        }

        [TestMethod]
        public void PocoToDataRowTest2()
        {
            var pretarget = new MockPocoDataModel();
            pretarget.DecimalValue = 3m;
            var target = new DataModel<MockPocoDataModel>(pretarget);
            var dr = target.Convert.ToDataRow();
            Assert.AreEqual(dr["DecimalValue"], 3m);
        }

        [TestMethod]
        public void PocoToDataRow_DefaultValueTest()
        {
            var pretarget = new MockPocoDataModel();
            var target = new DataModel<MockPocoDataModel>(pretarget);
            var dr = target.Convert.ToDataRow();
            Assert.AreEqual(dr["DecimalValue"], 0m);
        }

        [TestMethod]
        public void GetMappingsTest()
        {
            var mappings = DataModel.GetMapping<MockDataModel>();
            Assert.IsTrue(mappings.FieldMappings.Count > 0);
            Assert.IsNotNull(mappings.TableMapping);
        }

        [TestMethod]
        public void EqualsTest()
        {
            var targetA = new MockDataModel();
            targetA.StringValue = "xx";
            targetA.DecimalValue = 3m;
            var targetB = new MockDataModel();
            targetB.StringValue = "xx";
            targetB.DecimalValue = 3m;
            Assert.IsTrue(targetA.Equals(targetB));
            targetB.MarkDeleted = true;
            Assert.IsFalse(targetA.Equals(targetB));
        }

        [Serializable]
        public class BinarySerializableDataModel : DataModel
        {
            public string Whateva
            {
                get { return (string) base["Whateva"]; }
                set { base["Whateva"] = value; }
            }
        }

        [TestMethod]
        public void BasicBinarySerializationTest()
        {
            var dm = new BinarySerializableDataModel();
            dm.Whateva = "yo whateva";
            var bs = new global::Gemli.Serialization.BinarySerialized<BinarySerializableDataModel>(dm);
            Assert.IsTrue(bs.SerializedValue.Length > 0);
            dm = bs.Deserialize();
            Assert.IsTrue(dm.Whateva == "yo whateva");
        }

        [TestMethod]
        public void BasicXmlSerializationTest()
        {
            var dm = new MockPocoDataModel();
            dm.StringValue = "it works";
            dm.DecimalValue = 3;
            var dmw = new DataModel<MockPocoDataModel>(dm);
            var xs = new global::Gemli.Serialization.XmlSerialized<DataModel<MockPocoDataModel>>(dmw);
            Assert.IsTrue(xs.SerializedValue.Length > 0);
            dmw = xs.Deserialize();
            Assert.IsTrue(dmw.Entity.StringValue == "it works");
            Assert.IsTrue(dmw.Entity.DecimalValue == 3);
        }

        //[TestMethod]
        //public void BasicJsonSerializationTest()
        //{
        //    var dm = new MockPocoDataModel();
        //    dm.StringValue = "JSON works";
        //    dm.DecimalValue = 3;

        //    var xx = new global::Gemli.Serialization.JsonSerialized<MockPocoDataModel>(dm);
            

        //    var dmw = new DataModel<MockPocoDataModel>(dm);
        //    var xs = new global::Gemli.Serialization.JsonSerialized<DataModel<MockPocoDataModel>>(dmw);
        //    Assert.IsTrue(xs.SerializedValue.Length > 0);
        //    dmw = xs.Deserialize();
        //    Assert.IsTrue(dmw.Entity.StringValue == "JSON works");
        //    Assert.IsTrue(dmw.Entity.DecimalValue == 3);
        //}
    }
}