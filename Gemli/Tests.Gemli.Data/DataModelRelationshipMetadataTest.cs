using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Gemli.Data;
using Gemli.Data.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Gemli.Data
{
    [TestClass]
    public class DataModelRelationshipMetadataTest
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

        [DataModelTable(Table = "mock_table")]
        public class MockPoco
        {
            [DataModelColumn("customentity_id")]
            public int ID { get; set; }

            [DataModelColumn("string_value")]
            public string MockStringValue { get; set; }

            [DataModelColumn("money_value")]
            public decimal MockDecimalValue { get; set; }
        }

        [DataModelTable(Table = "mock_table2")]
        public class MockPocoChild
        {
            [DataModelColumn("customentity_id")]
            [ForeignKey(
                ForeignEntity = typeof(MockPoco),
                ForeignEntityProperty = "ID",
                Relationship = Relationship.OneToOne,
                AssignToMember = "MockPoco")]
            public int ID { get; set; }

            [DataModelColumn("string_value")]
            public string MockStringValue { get; set; }

            [DataModelColumn("money_value")]
            public decimal MockDecimalValue { get; set; }

            public MockPoco MockPoco { get; set; }
        }

        [DataModelTable(Table = "mock_table3")]
        public class MockPocoChild2 : DataModel
        {
            [DataModelColumn("mocktable3_id")]
            [ForeignKey(
                ForeignEntity = typeof(MockPoco),
                ForeignColumn = "customentity_id",
                Relationship = Relationship.OneToMany)]
            public int ID { get; set; }

            [DataModelColumn("string_value")]
            public string MockStringValue { get; set; }

            [DataModelColumn("money_value")]
            public decimal MockDecimalValue { get; set; }
        }

        [DataModelTable(Table = "mock_table4")]
        public class MockPocoChild3 : DataModel
        {
            [DataModelColumn("mocktable3_id")]
            [ForeignKey(
                ForeignSchemaName = "dbo",
                ForeignTableName = "mock_table",
                ForeignColumn = "customentity_id",
                Relationship = Relationship.ManyToMany)]
            public int ID { get; set; }

            [DataModelColumn("string_value")]
            public string MockStringValue { get; set; }

            [DataModelColumn("money_value")]
            public decimal MockDecimalValue { get; set; }
        }

        [DataModelTable("Company")]
        public class Company
        {
            [DataModelColumn("company_id",
                IsPrimaryKey = true, DbType = DbType.Int32)]
            public int ID { get; set; }

            [DataModelColumn("company_name")]
            public string CompanyName { get; set; }
            [ForeignDataModel(Relationship = Relationship.OneToMany)]
            public List<Contact> Contacts { get; set; }
        }

        [DataModelTable("Contact")]
        public class Contact
        {
            [DataModelColumn("contact_id", IsPrimaryKey = true)]
            public int ID { get; set; }
            [DataModelColumn("contact_name")]
            public string Name { get; set; }
            [DataModelColumn("contact_phone")]
            public string Phone { get; set; }
            [ForeignDataModel(LocalColumn = "company_id",
                Relationship = Relationship.ManyToOne)]
            public Company Company { get; set; }
        }

        [TestMethod]
        public void GetRelationshipInfoTest()
        {
            //var pretarget = DataModelMap.GetEntityMapping(typeof (MockPoco));
            var target = DataModelMap.GetEntityMapping(typeof(MockPocoChild));
            var fkfs = new List<DataModelColumnAttribute>();
            foreach (var field_kvp in target.FieldMappings)
            {
                var field = field_kvp.Value;
                if (field.IsForeignKey) fkfs.Add(field);
            }
            Assert.IsTrue(fkfs.Count == 1, "No foreign key mapping found (or wrong count)");
            var mapping = fkfs[0].ForeignKeyMapping;
            Assert.AreEqual(mapping.ForeignEntity, typeof(MockPoco), "RelatesTo");
            Assert.AreEqual(mapping.ForeignEntityProperty, "ID", "OnMatchProperty");
            Assert.AreEqual(mapping.Relationship, Relationship.OneToOne, "Relationship");
            // resolve OnMatchDataField
            Assert.AreEqual(mapping.ForeignColumn, "customentity_id", "OnMatchDataField");
        }

        [TestMethod]
        public void GetRelationshipInfoTest2()
        {
            var target = DataModelMap.GetEntityMapping(typeof(Company));
            var targetFEs = target.ForeignModelMappings;
            Assert.IsNotNull(targetFEs);
            var contactsMeta = targetFEs["Contacts"];
            Assert.IsNotNull(contactsMeta);
            Assert.IsTrue(contactsMeta.Relationship == Relationship.OneToMany);
            Assert.IsTrue(contactsMeta.RelatedTable == "Contact");
            Assert.IsTrue(contactsMeta.RelatedTableColumn == "company_id");
        }

        [TestMethod]
        public void GetRelationshipInfoTest3()
        {
            var target = DataModelMap.GetEntityMapping(typeof(Contact));
            var targetFEs = target.ForeignModelMappings;
            Assert.IsNotNull(targetFEs);
            var contactsMeta = targetFEs["Company"];
            Assert.IsNotNull(contactsMeta);
            Assert.IsTrue(contactsMeta.Relationship == Relationship.ManyToOne);
            Assert.IsTrue(contactsMeta.RelatedTable == "Company");
            Assert.IsTrue(contactsMeta.LocalColumn == "company_id");
        }
    }
}
