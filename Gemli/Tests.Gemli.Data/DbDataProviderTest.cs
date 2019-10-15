using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Gemli.Data;
using Gemli.Data.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Gemli.Data
{
    [TestClass]
    public partial class DbDataProviderTest
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
            [DataModelColumn("customentity_id")] // infer:, IsIdentity = true)]
            public int ID { get; set; }

            [DataModelColumn("string_value")]
            public string MockStringValue { get; set; }

            [DataModelColumn("money_value")]
            public decimal? MockDecimalValue { get; set; }
        }

        [TestMethod]
        public void TestSqlConnectionTest()
        {
            var connString = TestSqlConnection;
            var sqlConnection = new SqlConnection(connString);
            sqlConnection.Open();
            var state = sqlConnection.State;
            Assert.IsTrue(state == ConnectionState.Connecting ||
                          state == ConnectionState.Open);
            sqlConnection.Close();
        }

        [TestMethod]
        public void LoadMockEntityTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var mocks = dbProvider.LoadModels(
                new DataModelQuery<DataModel<MockPoco>>()
                    .WhereProperty["ID"] == 2)
                .Unwrap();
            
            var myMockEntityQuery = DataModel<MockPoco>.NewQuery()
                .WhereProperty["ID"] == 2;
            var entity = dbProvider.LoadModel(myMockEntityQuery);
            Assert.IsNotNull(entity);
            Assert.AreEqual("ghi", entity.Entity.MockStringValue);
            Assert.IsTrue(entity.DataProvider == dbProvider);
        }

        [TestMethod]
        public void LoadMockEntityTest2()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["string_value"] == "abc";
            var entity = dbProvider.LoadModel(myMockEntityQuery);
            Assert.IsNotNull(entity);
            Assert.AreEqual("abc", entity.Entity.MockStringValue);
            Assert.IsTrue(entity.DataProvider == dbProvider);
        }

        [TestMethod]
        public void LoadMockEntitiesTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var myMockEntityQuery = DataModel<MockPoco>.NewQuery()
                .WhereColumn["customentity_id"].IsGreaterThan(-1);
            var entities = dbProvider.LoadModels(myMockEntityQuery);
            Assert.IsNotNull(entities);
            Assert.IsTrue(entities.Count > 0);
            Assert.IsTrue(entities.DataProvider == dbProvider);

            var objects = entities.Unwrap();
            /*always true: 
            Assert.IsTrue(entities[0] is DataModel<MockPoco>);*/
            Assert.IsTrue(objects[0] is MockPoco);

            Assert.IsNotNull(objects);
            Assert.IsTrue(objects.Count > 0);
        }

        [TestMethod]
        public void LoadMockEntitiesTest2()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var myMockEntityQuery = DataModel<MockPoco>.NewQuery()
                .AddSortItem("MockStringValue", Sort.Descending);
            var entities = dbProvider.LoadModels(myMockEntityQuery);
            Assert.IsNotNull(entities);
            Assert.IsTrue(entities.Count > 0);
            var models = entities.Unwrap<MockPoco>();
            Assert.AreEqual(models[0].MockStringValue, "ghi");
            Assert.AreEqual(models[1].MockStringValue, "def");
            Assert.AreEqual(models[2].MockStringValue, "abc");
        }

        [TestMethod]
        public void SaveModifiedEntityTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var myMockEntityQuery = DataModel<MockPoco>.NewQuery()
                .WhereColumn["string_value"] == "abc";
            var entity = dbProvider.LoadModel(myMockEntityQuery);
            entity.Entity.MockStringValue = "xyz";
            entity.Save();
            
            myMockEntityQuery = DataModel<MockPoco>.NewQuery()
                .WhereColumn["string_value"] == "xyz";
            entity = dbProvider.LoadModel(myMockEntityQuery);

            Assert.IsNotNull(entity);
            Assert.AreEqual("xyz", entity.Entity.MockStringValue);
            Assert.IsTrue(entity.DataProvider == dbProvider);

            entity.Entity.MockStringValue = "abc";
            entity.Save();
        }

        [TestMethod]
        public void SaveModifiedMockEntitiesTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["customentity_id"].IsGreaterThan(-1);
            var entities = dbProvider.LoadModels(myMockEntityQuery);
            entities[0].Entity.MockStringValue = "jkl";
            entities[1].Entity.MockStringValue = "mno";
            entities[2].Entity.MockStringValue = "pqr";
            entities.Save();

            entities = dbProvider.LoadModels(myMockEntityQuery);
            Assert.IsTrue(entities[0].Entity.MockStringValue == "jkl");
            Assert.IsTrue(entities[1].Entity.MockStringValue == "mno");
            Assert.IsTrue(entities[2].Entity.MockStringValue == "pqr");

            //cleanup
            entities[0].Entity.MockStringValue = "abc";
            entities[1].Entity.MockStringValue = "ghi";
            entities[2].Entity.MockStringValue = "def";
            entities.Save();
        }

        [TestMethod]
        public void CreateAndDeleteEntityTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var mp = new MockPoco {MockStringValue = "xxx"};
            var dew = new DataModel<MockPoco>(mp);
            dew.DataProvider = dbProvider;
            dew.Save();

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["string_value"] == "xxx";
            var entity = dbProvider.LoadModel(myMockEntityQuery);
            Assert.IsNotNull(entity);
            Assert.IsNotNull(entity.Entity.ID);
            Assert.IsTrue(entity.Entity.ID > 0);
            Assert.IsTrue(entity.Entity.MockStringValue == "xxx");
            entity.MarkDeleted = true;
            entity.Save();

            entity = dbProvider.LoadModel(myMockEntityQuery);
            Assert.IsNull(entity);
        }

        [TestMethod]
        public void CreateAndDeleteEntityTest2()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var mp = new MockPoco { MockStringValue = "xxx" };
            var dew = new DataModel<MockPoco>(mp); // data entity wrapper
            dew.DataProvider = dbProvider;
            dew.Save(); // auto-synchronizes ID
            // or...
            //dbProvider.SaveModel(dew);
            //dew.SynchronizeFields(SyncTo.ClrMembers); // manually sync ID

            // now let's load it and validate that it was saved
            var mySampleEntityQuery = DataModel<MockPoco>.NewQuery()
                .WhereProperty["ID"] == mp.ID; // mp.ID was inferred as IsIdentity so we auto-returned it on Save()
            var data = dbProvider.LoadModel(mySampleEntityQuery);
            Assert.IsNotNull(data);

            // by the way, you can go back to the POCO type, too
            var mp2 = data.Entity;
            // precompiler-checked: Assert.IsTrue(mp2 is MockPoco);
            Assert.IsTrue(mp2.ID > 0);
            Assert.IsTrue(mp2.MockStringValue == "xxx");

            // test passed, let's delete the test record
            data.MarkDeleted = true;
            data.Save();

            // ... and make sure that it has been deleted
            data = dbProvider.LoadModel(mySampleEntityQuery);
            Assert.IsNull(data);
        }

        [TestMethod]
        public void StaticLoadMethodTest()
        {
            var first = DataModel<MockPoco>.Load(DataModel<MockPoco>.NewQuery());
            Assert.IsNotNull(first);
            Assert.IsTrue(first.Entity.MockStringValue == "abc");
        }

        [TestMethod]
        public void StaticLoadManyMethodTest()
        {
            var many = DataModel<MockPoco>.LoadMany(DataModel<MockPoco>.NewQuery());
            var first = many.First();
            Assert.IsNotNull(first);
            Assert.IsTrue(first.Entity.MockStringValue == "abc");
            var last = many.Last();
            Assert.IsNotNull(last);
            Assert.IsFalse(last.Entity.MockStringValue == "abc");
        }

        [TestMethod]
        public void StaticLoadAllMethodTest()
        {
            var all = DataModel<MockPoco>.LoadAll();
            Assert.IsNotNull(all);
            Assert.IsTrue(all.Count > 0);
            Assert.IsTrue(all.First().Entity.MockStringValue == "abc");
            Assert.IsTrue(all.Last().Entity.MockStringValue != "abc");
        }

        [TestMethod]
        public void StaticSaveMethodTest()
        {
            var entity = new MockPoco
                             {
                                 MockStringValue = "zzz"
                             };
            DataModel<MockPoco>.Save(entity);

            var model = DataModel<MockPoco>.LoadMany(DataModel<MockPoco>.NewQuery()).Last();
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Entity.MockStringValue == "zzz");
            model.MarkDeleted = true;
            model.Save();
        }

        [TestMethod]
        public void QueryLoadMethodTest()
        {
            var first = DataModel<MockPoco>.NewQuery().SelectFirst();
            Assert.IsNotNull(first);
            Assert.IsTrue(first.Entity.MockStringValue == "abc");
        }

        [TestMethod]
        public void QueryLoadManyMethodTest()
        {
            var many = DataModel<MockPoco>.NewQuery().SelectMany();
            var first = many.First();
            Assert.IsNotNull(first);
            Assert.IsTrue(first.Entity.MockStringValue == "abc");
            var last = many.Last();
            Assert.IsNotNull(last);
            Assert.IsFalse(last.Entity.MockStringValue == "abc");
        }

        [TestMethod]
        public void CreateAndDeleteEntitiesTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var mockPoco1 = new MockPoco { MockStringValue = "MockPoco1_xxx" };
            var mockPoco1_de = new DataModel<MockPoco>(mockPoco1);
            mockPoco1_de.DataProvider = dbProvider;
            mockPoco1_de.Save();
            var mockPoco2 = new MockPoco { MockStringValue = "MockPoco2_xxx" };
            var mockPoco2_de = new DataModel<MockPoco>(mockPoco2);
            mockPoco2_de.DataProvider = dbProvider;
            mockPoco2_de.Save();
            var mockPoco3 = new MockPoco { MockStringValue = "MockPoco3_xxx" };
            var mockPoco3_de = new DataModel<MockPoco>(mockPoco3);
            mockPoco3_de.DataProvider = dbProvider;
            mockPoco3_de.Save();

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["string_value"].IsLike("%xxx");
            var entities = dbProvider.LoadModels(myMockEntityQuery);
            var e = entities.Unwrap<MockPoco>()[0];

            entities[0].MarkDeleted = true;
            entities[2].MarkDeleted = true;
            entities.Save();

            var entities2 = dbProvider.LoadModels(myMockEntityQuery);
            if (entities2.Count!= 1)
            {
                
            }
            Assert.IsTrue(entities2.Count == 1);

            //cleanup
            entities[1].MarkDeleted = true;
            entities.Save();
        }

        [TestMethod]
        public void SelectWithProcTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var oldMapping = DataModelMap.GetEntityMapping(typeof(MockPoco));
            DataModelMap.MapItems.Remove(typeof (MockPoco));
            var newMapping = DataModelMap.GetEntityMapping(typeof (MockPoco));
            newMapping.TableMapping.SelectProcedure = "sp_mock_table_select";

            try
            {

                var q = DataModel<MockPoco>.NewQuery().WhereProperty["ID"] == 1;
                var entity = dbProvider.LoadModel(q);

                Assert.IsNotNull(entity);
                Assert.IsTrue(entity.Entity.ID == 1);
            }
            finally
            {

                DataModelMap.MapItems.Remove(typeof (MockPoco));
                DataModelMap.MapItems.Add(typeof (MockPoco), oldMapping);
            }
        }

        [TestMethod]
        public void SelectManyWithProcTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var oldMapping = DataModelMap.GetEntityMapping(typeof(MockPoco));
            DataModelMap.MapItems.Remove(typeof(MockPoco));
            var newMapping = DataModelMap.GetEntityMapping(typeof(MockPoco));
            newMapping.TableMapping.SelectManyProcedure = "sp_mock_table_SelectMany";

            try
            {

                var q = DataModel<MockPoco>.NewQuery().AddSortItem("ID", Sort.Descending);
                var entities = dbProvider.LoadModels(q);

                Assert.IsNotNull(entities);
                Assert.IsTrue(entities.Count > 1);
                Assert.IsTrue(entities[0].Entity.ID == 3);

            }
            finally
            {

                DataModelMap.MapItems.Remove(typeof (MockPoco));
                DataModelMap.MapItems.Add(typeof (MockPoco), oldMapping);
            }
        }

        [TestMethod]
        public void SelectManyWithProcTest2()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var oldMapping = DataModelMap.GetEntityMapping(typeof (MockPoco));
            DataModelMap.MapItems.Remove(typeof (MockPoco));
            var newMapping = DataModelMap.GetEntityMapping(typeof (MockPoco));
            newMapping.TableMapping.SelectManyProcedure = "sp_mock_table_SelectMany";

            try
            {

                var q = DataModel<MockPoco>.NewQuery().WhereProperty["ID"] == 3;
                var entities = dbProvider.LoadModels(q);

                Assert.IsNotNull(entities);
                Assert.IsTrue(entities.Count == 1);
                Assert.IsTrue(entities[0].Entity.ID == 3);

            }
            finally
            {

                DataModelMap.MapItems.Remove(typeof (MockPoco));
                DataModelMap.MapItems.Add(typeof (MockPoco), oldMapping);
            }
        }

        [TestMethod]
        public void CreateWithProcAndDeleteEntityTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var oldMapping = DataModelMap.GetEntityMapping(typeof(MockPoco));
            DataModelMap.MapItems.Remove(typeof(MockPoco));
            var newMapping = DataModelMap.GetEntityMapping(typeof(MockPoco));
            newMapping.TableMapping.InsertProcedure = "sp_mock_table_Insert";

            try
            {

                var mp = new MockPoco { MockStringValue = "xxx" };
                var dew = new DataModel<MockPoco>(mp);
                dew.DataProvider = dbProvider;
                dew.Save();

                var myMockEntityQuery = DataModel<MockPoco>
                    .NewQuery()
                    .WhereColumn["string_value"] == "xxx";
                var entity = dbProvider.LoadModel(myMockEntityQuery);
                Assert.IsNotNull(entity);
                Assert.IsNotNull(entity.Entity.ID);
                Assert.IsTrue(entity.Entity.ID > 0);
                Assert.IsTrue(entity.Entity.MockStringValue == "xxx");
                entity.MarkDeleted = true;
                entity.Save();

                entity = dbProvider.LoadModel(myMockEntityQuery);
                Assert.IsNull(entity);

            }
            finally
            {
                DataModelMap.MapItems.Remove(typeof (MockPoco));
                DataModelMap.MapItems.Add(typeof (MockPoco), oldMapping);
            }
        }

        [TestMethod]
        public void SaveModifiedEntityWithProcTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var oldMapping = DataModelMap.GetEntityMapping(typeof(MockPoco));
            DataModelMap.MapItems.Remove(typeof(MockPoco));
            var newMapping = DataModelMap.GetEntityMapping(typeof(MockPoco));
            newMapping.TableMapping.UpdateProcedure = "sp_mock_table_Update";

            try
            {

                var myMockEntityQuery = DataModel<MockPoco>.NewQuery()
                    .WhereColumn["string_value"] == "abc";
                var entity = dbProvider.LoadModel(myMockEntityQuery);
                entity.Entity.MockStringValue = "xyz";
                entity.Save();

                myMockEntityQuery = DataModel<MockPoco>.NewQuery()
                    .WhereColumn["string_value"] == "xyz";
                entity = dbProvider.LoadModel(myMockEntityQuery);

                Assert.IsNotNull(entity);
                Assert.AreEqual("xyz", entity.Entity.MockStringValue);
                Assert.IsTrue(entity.DataProvider == dbProvider);

                entity.Entity.MockStringValue = "abc";
                entity.Save();
            }
            finally
            {

                DataModelMap.MapItems.Remove(typeof (MockPoco));
                DataModelMap.MapItems.Add(typeof (MockPoco), oldMapping);
            }
        }

        [TestMethod]
        public void CreateWithoutProcAndDeleteWithProcEntityTest()
        {
            var dbProvider = ProviderDefaults.AppProvider;

            var oldMapping = DataModelMap.GetEntityMapping(typeof(MockPoco));
            DataModelMap.MapItems.Remove(typeof(MockPoco));
            var newMapping = DataModelMap.GetEntityMapping(typeof(MockPoco));
            newMapping.TableMapping.DeleteProcedure = "sp_mock_table_Delete";

            try
            {

                var mp = new MockPoco { MockStringValue = "xxx" };
                var dew = new DataModel<MockPoco>(mp);
                dew.DataProvider = dbProvider;
                dew.Save();

                var myMockEntityQuery = DataModel<MockPoco>
                    .NewQuery()
                    .WhereColumn["string_value"] == "xxx";
                var entity = dbProvider.LoadModel(myMockEntityQuery);
                Assert.IsNotNull(entity);
                Assert.IsNotNull(entity.Entity.ID);
                Assert.IsTrue(entity.Entity.ID > 0);
                Assert.IsTrue(entity.Entity.MockStringValue == "xxx");
                entity.MarkDeleted = true;
                entity.Save();

                entity = dbProvider.LoadModel(myMockEntityQuery);
                Assert.IsNull(entity);

            }
            finally
            {
                DataModelMap.MapItems.Remove(typeof(MockPoco));
                DataModelMap.MapItems.Add(typeof(MockPoco), oldMapping);
            }
        }

        [DataModelTable("company")]
        public class Company
        {
            [DataModelColumn("company_id",
                IsPrimaryKey = true, DbType = DbType.Int32)]
            public int ID { get; set; }

            [DataModelColumn("name")]
            public string CompanyName { get; set; }
            [ForeignDataModel(Relationship = Relationship.OneToMany)]
            public List<Contact> Contacts { get; set; }
        }

        [DataModelTable("contact")]
        public class Contact : DataModel
        {
            [DataModelColumn("contact_id", IsPrimaryKey = true)]
            public int ID
            {
                get { return (int)base["ID"]; }
                set { base["ID"] = value; }
            }
            [DataModelColumn("name")]
            public string Name
            {
                get { return (string)base["Name"]; }
                set { base["Name"] = value; }
            }

            [DataModelColumn("phone")]
            public string Phone
            {
                get { return (string)base["Phone"]; }
                set { base["Phone"] = value; }
            }
            [ForeignDataModel(LocalColumn = "company_id",
                Relationship = Relationship.ManyToOne)]
            public Company Company { get; set; }
        }

        [TestMethod]
        public void DeepLoadEntityOneToManyTest()
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);

            var query = new DataModelQuery<DataModel<Company>>()
                .WhereProperty["ID"] == 1;
            var decompany = dbProvider.DeepLoadModel(query) as DataModel<Company>;
            Assert.IsNotNull(decompany);
            Company company = decompany.Entity;
            Assert.IsNotNull(company);
            Assert.IsNotNull(company.Contacts, "Contacts not populated");
            Assert.IsTrue(company.Contacts.Count == 2, company.Contacts.Count
                + " loaded (expected 2).");
            Assert.IsTrue(company.Contacts[0].Name == "Betty Sue" ||
                          company.Contacts[0].Name == "John Doe");
            Assert.IsTrue(company.Contacts[1].Name == "Betty Sue" ||
                          company.Contacts[1].Name == "John Doe");
        }

        [TestMethod]
        public void DeepLoadEntityOneToManyTest2()
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);

            var query = new DataModelQuery<DataModel<Company>>()
                .WhereProperty["ID"] == 2;
            var decompany = dbProvider.DeepLoadModel(query) as DataModel<Company>;
            Assert.IsNotNull(decompany);
            Company company = decompany.Entity;
            Assert.IsNotNull(company);
            Assert.IsNotNull(company.Contacts, "Contacts not populated");
            Assert.IsTrue(company.Contacts.Count == 2, company.Contacts.Count.ToString() + " loaded (expected 2).");
            Assert.IsTrue(company.Contacts[0].Name == "Bobby Joe" ||
                          company.Contacts[0].Name == "Jane Lane");
            Assert.IsTrue(company.Contacts[1].Name == "Bobby Joe" ||
                          company.Contacts[1].Name == "Jane Lane");
        }

        [TestMethod]
        public void DeepLoadEntityManyToOneTest1()
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);
            var query = new DataModelQuery<Contact>()
                .WhereProperty["ID"] == 1;
            var contact = dbProvider.DeepLoadModel(query) as Contact;
            Assert.IsNotNull(contact);
            Assert.IsTrue(contact.Name == "Bobby Joe");
            Assert.IsNotNull(contact.Company);
            Assert.IsTrue(contact.Company.CompanyName == "Foobar, Ltd.");
        }

        [TestMethod]
        public void DeepLoadEntityManyToOneTest2()
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);

            var query = new DataModelQuery<Contact>()
                .WhereProperty["ID"] == 2;
            var contact = dbProvider.DeepLoadModel(query) as Contact;
            Assert.IsNotNull(contact);
            Assert.IsTrue(contact.Name == "Betty Sue");
            Assert.IsNotNull(contact.Company);
            Assert.IsTrue(contact.Company.CompanyName == "Acme, Inc.");
        }

        [TestMethod]
        public void DeepLoadEntityManyToOneTest3()
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);

            var query = new DataModelQuery<Contact>()
                .WhereProperty["ID"] == 3;
            var contact = dbProvider.DeepLoadModel(query) as Contact;
            Assert.IsNotNull(contact);
            Assert.IsTrue(contact.Name == "John Doe");
            Assert.IsNotNull(contact.Company);
            Assert.IsTrue(contact.Company.CompanyName == "Acme, Inc.");
        }

        [TestMethod]
        public void DeepLoadEntityManyToOneTest4()
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);

            var query = new DataModelQuery<Contact>()
                .WhereProperty["ID"] == 4;
            var contact = dbProvider.DeepLoadModel(query) as Contact;
            Assert.IsNotNull(contact);
            Assert.IsTrue(contact.Name == "Jane Lane");
            Assert.IsNotNull(contact.Company);
            Assert.IsTrue(contact.Company.CompanyName == "Foobar, Ltd.");
            Assert.IsTrue(contact.Company.Contacts != null);
            Assert.IsTrue(contact.Company.Contacts.Count > 0);
        }

        [TestMethod]
        public void DeepSaveEntityTest()
        {
            // todo: test all four relationship types

            try
            {

                var sqlFactory = SqlClientFactory.Instance;
                var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);

                var query = new DataModelQuery<DataModel<Company>>()
                                .WhereProperty["ID"] == 1;
                var decompany = dbProvider.DeepLoadModel(query);
                var company = decompany.Entity;
                company.CompanyName += "_";
                foreach (var c in company.Contacts)
                    c.Name += "_";
                decompany.Save(true);

                query = new DataModelQuery<DataModel<Company>>()
                            .WhereProperty["ID"] == 1;
                decompany = dbProvider.DeepLoadModel(query);
                company = decompany.Entity;
                Assert.IsTrue(company.CompanyName.Length > 1 &&
                              company.CompanyName.EndsWith("_"));
                Assert.IsTrue(company.Contacts[0]
                                  .Name.Length > 1 &&
                              company.Contacts[0]
                                  .Name.EndsWith("_"));
                Assert.IsTrue(company.Contacts[company.Contacts.Count - 1]
                                  .Name.Length > 1 &&
                              company.Contacts[company.Contacts.Count - 1]
                                  .Name.EndsWith("_"));

            }
            finally
            {
                // clean-up
                ReloadData_CompanyContact();
            }
        }

        [TestMethod]
        public void DeepSaveEntitiesTest()
        {
            try
            {
                var sqlFactory = SqlClientFactory.Instance;
                var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);

                var query = new DataModelQuery<DataModel<Company>>()
                                .WhereProperty["ID"] == 1;
                var decompany = dbProvider.DeepLoadModel(query);
                var company = decompany.Entity;
                company.CompanyName += "_";
                foreach (var c in company.Contacts)
                    c.Name += "_";

                var query2 = new DataModelQuery<DataModel<Company>>()
                                 .WhereProperty["ID"] == 2;
                var decompany2 = dbProvider.DeepLoadModel(query2);
                var company2 = decompany2.Entity;
                company2.CompanyName = "_" + company2.CompanyName;
                foreach (var c in company2.Contacts)
                    c.Name = "_" + c.Name;

                var col = new DataModelCollection<DataModel<Company>>();
                col.Add(decompany);
                col.Add(decompany2);
                col.DataProvider = dbProvider;

                col.Save(true);

                query = new DataModelQuery<DataModel<Company>>()
                            .WhereProperty["ID"] == 1;
                decompany = dbProvider.DeepLoadModel(query);
                company = decompany.Entity;
                Assert.IsTrue(company.CompanyName.Length > 1 &&
                              company.CompanyName.EndsWith("_"));
                Assert.IsTrue(company.Contacts[0]
                                  .Name.Length > 1 &&
                              company.Contacts[0]
                                  .Name.EndsWith("_"));
                Assert.IsTrue(company.Contacts[company.Contacts.Count - 1]
                                  .Name.Length > 1 &&
                              company.Contacts[company.Contacts.Count - 1]
                                  .Name.EndsWith("_"));

                query = new DataModelQuery<DataModel<Company>>()
                            .WhereProperty["ID"] == 2;
                decompany = dbProvider.DeepLoadModel(query);
                company = decompany.Entity;
                Assert.IsTrue(company.CompanyName.Length > 1 &&
                              company.CompanyName.StartsWith("_"));
                Assert.IsTrue(company.Contacts[0]
                                  .Name.Length > 1 &&
                              company.Contacts[0]
                                  .Name.StartsWith("_"));
                Assert.IsTrue(company.Contacts[company.Contacts.Count - 1]
                                  .Name.Length > 1 &&
                              company.Contacts[company.Contacts.Count - 1]
                                  .Name.StartsWith("_"));

            }
            finally
            {
                ReloadData_CompanyContact(); // clean-up
            }
        }

        public class Group
        {
            [DataModelColumn("group_id", IsPrimaryKey = true)]
            public Guid ID { get; set; }
            [DataModelColumn("group_name")]
            public string Name { get; set; }
            [ForeignDataModel(Relationship = Relationship.ManyToMany,
                MappingTable = "GroupUser")]
            public List<User> Users { get; set; }
        }

        public class User
        {
            [DataModelColumn("user_id", IsPrimaryKey = true)]
            public Guid ID { get; set; }
            [DataModelColumn("user_name")]
            public string Name { get; set; }
            [ForeignDataModel(Relationship = Relationship.ManyToMany,
                MappingTable = "GroupUser")]
            public List<Group> Groups { get; set; }
        }

        [TestMethod]
        public void DeepLoadEntityManyToManyTest()
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);

            var query = new DataModelQuery<DataModel<Group>>();
            var groups = dbProvider.DeepLoadModels(query);

            Assert.IsTrue(groups.Count == 2, "Groups did not load");
            Assert.IsNotNull(groups[0].Entity, "Group 0 Entity was not set");
            Assert.IsNotNull(groups[0].Entity.Users, "Group 0 Users were not set");
            Assert.IsTrue(groups[0].Entity.Users.Count == 2, "Users count (group 0 of 0,1) is not 2");
            Assert.IsNotNull(groups[1].Entity, "Group 1 Entity was not set");
            Assert.IsNotNull(groups[0].Entity.Users, "Group 1 Users were not set");
            Assert.IsTrue(groups[1].Entity.Users.Count == 2, "Users count (group 1 of 0,1) is not 2");

            Assert.IsTrue(groups[0].Entity.Users[0].ID
                       != groups[1].Entity.Users[0].ID, "Same user loaded between groups");
            Assert.IsTrue(groups[0].Entity.Users[0].ID
                       != groups[1].Entity.Users[1].ID, "Same user loaded between groups");
            Assert.IsTrue(groups[1].Entity.Users[0].ID
                       != groups[0].Entity.Users[0].ID, "Same user loaded between groups");
        }

        [TestMethod]
        public void DefaultDBProviderTest()
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);
            ProviderDefaults.AppProvider = dbProvider;
            var poco = new MockPoco();
            poco.MockStringValue = "Provider wuz here";
            var pocoModel = new DataModel<MockPoco>(poco);
            pocoModel.Save();
            var q = DataModel<MockPoco>.NewQuery()
                .WhereProperty["MockStringValue"]
                .IsEqualTo("Provider wuz here");
            pocoModel = dbProvider.LoadModel(q);
            Assert.IsNotNull(poco);
            Assert.IsTrue(pocoModel.Entity.MockStringValue == "Provider wuz here");
            Assert.IsTrue(pocoModel.Entity.ID > 0);

            // clean-up
            
        }

        public class idlist
        {
            public int id { get; set; }
        }

        [TestMethod]
        public void PaginatePage1()
        {
            var items = DataModel<idlist>.NewQuery().Page[1].OfItemsPerPage(20)
                .AddSortItem("id").SelectMany().Unwrap<idlist>();
            Assert.IsTrue(items.Count == 20);
            Assert.IsTrue(items[0].id==0);
            Assert.IsTrue(items[items.Count - 1].id == items.Count - 1);
        }

        [TestMethod]
        public void PaginatePage2()
        {
            var query = DataModel<idlist>.NewQuery().Page[2].OfItemsPerPage(10)
                .AddSortItem("id");
            var items = DataModel<idlist>.LoadMany(query).Unwrap<idlist>();
            Assert.IsTrue(items.Count == 10);
            Assert.IsTrue(items[0].id == 10);
            Assert.IsTrue(items[items.Count - 1].id == 19);
        }

        [TestMethod]
        public void CountTest()
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);

            var count = dbProvider.GetCount(DataModel<idlist>.NewQuery());
            Assert.IsTrue(count == 1000);
        }


        [TestMethod]
        public void QueryCountTest()
        {
            var count = DataModel<idlist>.NewQuery().SelectCount();
            Assert.IsTrue(count == 1000);
        }

        [TestMethod]
        public void CountWithWhereFilterTest()
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);

            var count = dbProvider.GetCount(DataModel<idlist>.NewQuery().WhereColumn["id"] < 4);
            Assert.IsTrue(count == 4);
        }

        [TestMethod]
        public void QueryCountWithWhereFilterTest()
        {
            var count = DataModel<idlist>.NewQuery().WhereColumn["id"].IsLessThan(4).SelectCount();
            Assert.IsTrue(count == 4);
        }

        public class HierarchyRoot
        {
            public int ID { get; set; }
            public string Name { get; set; }
            [ForeignDataModel(RelatedTableColumn="root_id", Relationship = Relationship.OneToMany)]
            public List<HierarchyChild> Children { get; set; }
        }

        public class HierarchyChild
        {
            public int ID { get; set; }
            [ForeignDataModel("root_id", Relationship = Relationship.ManyToOne)]
            public HierarchyRoot Root { get; set; }
            public string Name { get; set; }
            [ForeignDataModel(RelatedTableColumn = "parent_id", Relationship = Relationship.OneToMany)]
            public List<HierarchyGrandChild> Children { get; set; }
        }

        public class HierarchyGrandChild
        {
            public int ID { get; set; }
            [ForeignDataModel("parent_id", Relationship = Relationship.ManyToOne)]
            public HierarchyChild Parent { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void ThreeLevelHierarchyTest1()
        {
            try
            {
                var root = new HierarchyRoot { Name = "Bob, Sr." };
                DataModel<HierarchyRoot>.Save(root);
                var child = new HierarchyChild { Name = "Bob, Jr.", Root = root };
                DataModel<HierarchyChild>.Save(child);
                var grandChild = new HierarchyGrandChild { Name = "Bob, 3rd", Parent = child };
                DataModel<HierarchyGrandChild>.Save(grandChild);

                Assert.IsTrue(grandChild.Name == "Bob, 3rd");
                Assert.IsNotNull(grandChild.Parent);
                Assert.IsTrue(grandChild.Parent.Name == "Bob, Jr.");
                Assert.IsNotNull(grandChild.Parent.Root);
                Assert.IsTrue(grandChild.Parent.Root.Name == "Bob, Sr.");
            }
            finally
            {
                // clean-up
                ReloadData_HierarchyTables();
            }
        }

        [TestMethod]
        public void ThreeLevelHierarchyTest2()
        {
            try
            {
                var root = new HierarchyRoot { Name = "Bob, Sr." };
                DataModel<HierarchyRoot>.Save(root);
                var child = new HierarchyChild { Name = "Bob, Jr.", Root = root };
                DataModel<HierarchyChild>.Save(child);
                var grandChild = new HierarchyGrandChild { Name = "Bob, 3rd", Parent = child };
                DataModel<HierarchyGrandChild>.Save(grandChild);

                root = DataModel<HierarchyRoot>.NewQuery().SelectFirst(true).Entity;
                Assert.IsNotNull(root);
                Assert.IsTrue(root.Name == "Bob, Sr.");
                Assert.IsNotNull(root.Children);
                Assert.IsTrue(root.Children.Count > 0);
                Assert.IsTrue(root.Children[0].Name == "Bob, Jr.");
                Assert.IsNotNull(root.Children[0].Children);
                Assert.IsTrue(root.Children[0].Children.Count > 0);
                Assert.IsTrue(root.Children[0].Children[0].Name == "Bob, 3rd");
            }
            finally
            {
                // clean-up
                ReloadData_HierarchyTables();
            }
        }

        [TestMethod]
        public void ThreeLevelHierarchyTest3()
        {
            try
            {
                var root = new HierarchyRoot { Name = "Bob, Sr." };
                DataModel<HierarchyRoot>.Save(root);
                var child = new HierarchyChild { Name = "Bob, Jr.", Root = root };
                DataModel<HierarchyChild>.Save(child);
                var grandChild = new HierarchyGrandChild { Name = "Bob, 3rd", Parent = child };
                DataModel<HierarchyGrandChild>.Save(grandChild);

                root = DataModel<HierarchyRoot>.NewQuery().SelectFirst(1, ProviderDefaults.AppProvider, null).Entity;
                Assert.IsNotNull(root);
                Assert.IsTrue(root.Name == "Bob, Sr.");
                Assert.IsNotNull(root.Children);
                Assert.IsTrue(root.Children.Count > 0);
                Assert.IsTrue(root.Children[0].Name == "Bob, Jr.");
                Assert.IsNull(root.Children[0].Children);
            }
            finally
            {
                // clean-up
                ReloadData_HierarchyTables();
            }
        }

    }
}
