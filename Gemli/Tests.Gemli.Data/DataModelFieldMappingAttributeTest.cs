using System;
using System.Collections.Generic;
using System.Linq;
using Gemli.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Gemli.Data
{
    [TestClass]
    public class DataModelFieldMappingAttributeTest
    {
        // ReSharper disable PossibleNullReferenceException
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
            [DataModelColumn]
            public string Field
            {
                get { return (string)base["Field"]; }
                set { base["Field"] = value; }
            }
        }

        public class MockDataModel2 : DataModel
        {
            [DataModelColumn]
            public string Field2
            {
                get { return (string)base["Field"]; }
                set { base["Field"] = value; }
            }
        }

        public class MockDataModel3 : DataModel
        {
            [DataModelColumn("00ga")]
            public virtual string Field3
            {
                get { return (string)base["00ga"]; }
                set { base["00ga"] = value; }
            }
        }

        public class MockDataModel4 : MockDataModel3
        {
        }

        public class MockDataModel5 : MockDataModel4
        {
            [DataModelColumn("00ga2", ClearBaseObjectMapping = true)]
            public override string Field3
            {
                get { return (string)base["00ga"]; }
                set { base["00ga"] = value; }
            }
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_DefaultNameTest()
        {
            var target = new MockDataModel();
            Assert.AreEqual(target.EntityMappings.FieldMappings["Field"].ColumnName, "Field");
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_DefaultNameTest2()
        {
            var target = new MockDataModel2();
            Assert.AreEqual(target.EntityMappings.FieldMappings["Field2"].ColumnName, "Field2");
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_SetNameTest()
        {
            var target = new MockDataModel3();
            Assert.AreEqual(target.EntityMappings.FieldMappings["Field3"].ColumnName, "00ga");
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_InheritAttribsTest()
        {
            var target = new MockDataModel4();
            Assert.AreEqual(target.EntityMappings.FieldMappings["Field3"].ColumnName, "00ga");
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_ClearInheritAttribsTest()
        {
            var target = new MockDataModel5();
            Assert.AreEqual(target.EntityMappings.FieldMappings["Field3"].ColumnName, "00ga2");
            Assert.IsFalse(target.EntityMappings.FieldMappings.ContainsKey("00ga"));
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_MemberAssignedTest()
        {
            var target = new MockDataModel5();
            var fieldmap = target.EntityMappings.FieldMappings["Field3"];
            Assert.IsNotNull(fieldmap.TargetMember);
        }

        public class MockClassForNullNotNullChecking
        {
            public int? ID { get; set; }
            public int NotNullInt { get; set; }
            public int? NullInt { get; set; }
            public string NullString { get; set; }
            public decimal NotNullDecimal { get; set; }
            public decimal? NullDecimal { get; set; }
            public DateTime NotNullDateTime { get; set; }
            public DateTime? NullDateTime { get; set; }
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_NullNotNullTest()
        {
            var map = DataModelMap.GetEntityMapping(typeof (MockClassForNullNotNullChecking));
            var fields = map.FieldMappings;
            Assert.IsFalse(fields["ID"].IsNullable ); // because it's an inferred primary key
            Assert.IsFalse(fields["NotNullInt"].IsNullable);
            Assert.IsTrue(fields["NullInt"].IsNullable);
            Assert.IsTrue(fields["NullString"].IsNullable);
            Assert.IsFalse(fields["NotNullDecimal"].IsNullable);
            Assert.IsTrue(fields["NullDecimal"].IsNullable);
            Assert.IsFalse(fields["NotNullDateTime"].IsNullable);
            Assert.IsTrue(fields["NullDateTime"].IsNullable);
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_DefaultValueInferences()
        {
            var map = DataModelMap.GetEntityMapping(typeof (MockClassForNullNotNullChecking));
            var fields = map.FieldMappings;
            Assert.AreEqual(fields["ID"].DefaultValue, default(int)); // because it's an inferred primary key
            Assert.AreEqual(fields["NotNullInt"].DefaultValue, default(int));
            Assert.AreEqual(fields["NullInt"].DefaultValue, default(int?));
            Assert.AreEqual((string)fields["NullString"].DefaultValue, default(string));
            Assert.AreEqual(fields["NotNullDecimal"].DefaultValue, default(decimal));
            Assert.AreEqual(fields["NullDecimal"].DefaultValue, default(decimal?));
            Assert.AreEqual(fields["NotNullDateTime"].DefaultValue, default(DateTime));
            Assert.AreEqual(fields["NullDateTime"].DefaultValue, default(DateTime?));
        }

        public class MockClassForAutoRelationshipMapping
        {
            // should map to column
            public int ID { get; set; }
            // should NOT map to column
            MockChildClassForAutoRelationshipMapping ChildProp { get; set; }
        }

        public class MockChildClassForAutoRelationshipMapping
        {
            public int ID { get; set; }
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_AutoRelationshipMapping_NoInferenceOnChild()
        {
            var map = DataModelMap.GetEntityMapping(typeof(MockClassForAutoRelationshipMapping));
            var fields = map.FieldMappings;
            Assert.IsTrue(fields.Count == 1);
            Assert.IsTrue(fields.ToList()[0].Key == "ID");
        }



        public class MockClassForAutoRelationshipMapping2
        {
            // should map to column
            public int ID { get; set; }
            [ForeignDataModel("FKChildID", "PKChildID")]
            public MockChildClassForAutoRelationshipMapping2 ChildProp { get; set; }
        }

        public class MockChildClassForAutoRelationshipMapping2
        {
            [DataModelColumn("PKChildID")]
            public int ID { get; set; }
            public MockClassForAutoRelationshipMapping2 MockClassForAutoRelationshipMapping2_ID { get; set; }
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_AutoRelationshipMapping_ChildPropRef()
        {
            var map = DataModelMap.GetEntityMapping(typeof (MockClassForAutoRelationshipMapping2));
            Assert.IsTrue(map.ForeignModelMappings.Count == 1);
            Assert.IsTrue(map.ForeignModelMappings.ToList()[0].Key == "ChildProp");
            Assert.IsTrue(map.ForeignModelMappings["ChildProp"].TargetMember != null);
            Assert.IsTrue(map.ForeignModelMappings["ChildProp"].TargetMember.Name == "ChildProp");
            Assert.IsTrue(map.ForeignModelMappings["ChildProp"].Relationship == Relationship.OneToOne);
            Assert.IsTrue(map.ForeignModelMappings["ChildProp"].LocalColumn == "FKChildID");
            Assert.IsTrue(map.ForeignModelMappings["ChildProp"].RelatedTableColumn == "PKChildID");
        }

        public class MockClassForAutoRelationshipMapping3
        {
            // should map to column
            public int ID { get; set; }
            [ForeignDataModel("FKChildID", "ID")]
            public List<MockChildClassForAutoRelationshipMapping3> ChildProp { get; set; }
        }

        public class MockChildClassForAutoRelationshipMapping3
        {
            public int ID { get; set; }
            public MockClassForAutoRelationshipMapping3 MockClassForAutoRelationshipMapping3_ID { get; set; }
        }

        [TestMethod]
        public void DataModelFieldMappingAttribute_AutoRelationshipMapping_ChildrenPropRef()
        {
            var map = DataModelMap.GetEntityMapping(typeof(MockClassForAutoRelationshipMapping3));
            Assert.IsTrue(map.ForeignModelMappings.Count == 1);
            Assert.IsTrue(map.ForeignModelMappings.ToList()[0].Key == "ChildProp");
            Assert.IsTrue(map.ForeignModelMappings["ChildProp"].TargetMember != null);
            Assert.IsTrue(map.ForeignModelMappings["ChildProp"].TargetMember.Name == "ChildProp");
            Assert.IsTrue(map.ForeignModelMappings["ChildProp"].Relationship == Relationship.OneToMany);
            Assert.IsTrue(map.ForeignModelMappings["ChildProp"].LocalColumn == "FKChildID");
            Assert.IsTrue(map.ForeignModelMappings["ChildProp"].RelatedTableColumn == "ID");
        }

        public class MockModelForObservingBehaviorOfAttributedPropWithUnattributedProp
        {
            [DataModelColumn]
            public string A { get; set; }

            public string B { get; set; }
        }

        /// <summary>
        /// Validates the assertion that an unattribited member beside
        /// an attributed member will actually be ignored.
        /// </summary>
        [TestMethod]
        public void UnattributedPropBesideAttributedPropGetsIgnored()
        {
            var mapping = DataModel.GetMapping<MockModelForObservingBehaviorOfAttributedPropWithUnattributedProp>();
            Assert.IsTrue(mapping.FieldMappings.Count == 1);
        }
    }
}