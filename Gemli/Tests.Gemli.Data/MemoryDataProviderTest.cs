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
    public class MemoryDataProviderTest
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

        [TestMethod]
        public void LoadMockEntityTest()
        {
            DataProviderBase mockRepos = new MemoryDataProvider();
            
            // load mock data
            var table = ((MemoryDataProvider) mockRepos).AddTable("mock_table");
            PopulateMockTable(table);

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereProperty["ID"].IsEqualTo(2);
            var entity = myMockEntityQuery.SelectFirst(mockRepos); // mockRepos.LoadModel(myMockEntityQuery);
            Assert.IsNotNull(entity);
            Assert.AreEqual("ghi", entity.Entity.MockStringValue);
            Assert.IsTrue(entity.DataProvider == mockRepos);
        }

        [TestMethod]
        public void LoadMockEntityTest2()
        {
            DataProviderBase mockRepos = new MemoryDataProvider();

            // load mock data
            var table = ((MemoryDataProvider)mockRepos).AddTable("mock_table");
            PopulateMockTable(table);

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["string_value"].IsEqualTo("abc");
            var entity = mockRepos.LoadModel(myMockEntityQuery);
            Assert.IsNotNull(entity);
            Assert.AreEqual("abc", entity.Entity.MockStringValue);
            Assert.IsTrue(entity.DataProvider == mockRepos);
        }

        [TestMethod]
        public void LoadMockEntitiesTest()
        {
            DataProviderBase mockRepos = new MemoryDataProvider();

            // load mock data
            var table = ((MemoryDataProvider)mockRepos).AddTable("mock_table");
            PopulateMockTable(table);

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["customentity_id"].IsGreaterThan(-1)
                .AddSortItem("customentity_id");
            var entities = myMockEntityQuery.SelectMany(mockRepos); // mockRepos.LoadModels(myMockEntityQuery);
            Assert.IsNotNull(entities);
            Assert.IsTrue(entities.Count > 0);
            Assert.IsTrue(entities.DataProvider == mockRepos);

            var objects = entities.Unwrap();
            /*always true: 
            Assert.IsTrue(entities[0] is DataModel<MockPoco>);*/
            Assert.IsTrue(objects[0] is MockPoco);

            Assert.IsNotNull(objects);
            Assert.IsTrue(objects.Count > 0);
        }

        [TestMethod]
        public void SaveModifiedEntityTest()
        {
            DataProviderBase mockRepos = new MemoryDataProvider();

            // load mock data
            var table = ((MemoryDataProvider)mockRepos).AddTable("mock_table");
            PopulateMockTable(table);

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["string_value"].IsEqualTo("abc");
            var entity = mockRepos.LoadModel(myMockEntityQuery);
            entity.Entity.MockStringValue = "xyz";
            entity.Save();
            
            myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["string_value"].IsEqualTo("xyz");
            entity = mockRepos.LoadModel(myMockEntityQuery);

            Assert.IsNotNull(entity);
            Assert.AreEqual("xyz", entity.Entity.MockStringValue);
            Assert.IsTrue(entity.DataProvider == mockRepos);
        }

        [TestMethod]
        public void SaveModifiedMockEntitiesTest()
        {
            DataProviderBase mockRepos = new MemoryDataProvider();

            // load mock data
            var table = ((MemoryDataProvider)mockRepos).AddTable("mock_table");
            PopulateMockTable(table);

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["customentity_id"].IsGreaterThan(-1);
            var entities = mockRepos.LoadModels(myMockEntityQuery);
            entities[0].Entity.MockStringValue = "jkl";
            entities[1].Entity.MockStringValue = "mno";
            entities[2].Entity.MockStringValue = "pqr";
            entities.Save();

            entities = mockRepos.LoadModels(myMockEntityQuery);
            Assert.IsTrue(entities[0].Entity.MockStringValue == "jkl");
            Assert.IsTrue(entities[1].Entity.MockStringValue == "mno");
            Assert.IsTrue(entities[2].Entity.MockStringValue == "pqr");
        }

        [TestMethod]
        public void DeleteEntityTest()
        {
            DataProviderBase mockRepos = new MemoryDataProvider();

            // load mock data
            var table = ((MemoryDataProvider)mockRepos).AddTable("mock_table");
            PopulateMockTable(table);

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["string_value"].IsEqualTo("abc");
            var entity = mockRepos.LoadModel(myMockEntityQuery);
            entity.MarkDeleted = true;
            entity.Save();

            entity = mockRepos.LoadModel(myMockEntityQuery);
            Assert.IsNull(entity);
        }

        [TestMethod]
        public void DeleteEntitiesTest()
        {
            DataProviderBase mockRepos = new MemoryDataProvider();

            // load mock data
            var table = ((MemoryDataProvider)mockRepos).AddTable("mock_table");
            PopulateMockTable(table);

            var myMockEntityQuery = DataModel<MockPoco>
                .NewQuery()
                .WhereColumn["customentity_id"].IsGreaterThan(-1);
            var entities = mockRepos.LoadModels(myMockEntityQuery);

            entities[0].MarkDeleted = true;
            entities[2].MarkDeleted = true;
            entities.Save();

            entities = mockRepos.LoadModels(myMockEntityQuery);
            Assert.IsTrue(entities.Count == 2);
        }

        [TestMethod]
        public void DeepLoadEntityTest()
        {
            DataProviderBase mockRepos = new MemoryDataProvider();
            var table = ((MemoryDataProvider)mockRepos).AddTable("mock_table");
            PopulateMockTable(table);
            var table2 = ((MemoryDataProvider)mockRepos).AddTable("mock_table2");
            PopulateMockTable(table2);

            var query = new DataModelQuery<DataModel<DataModelRelationshipMetadataTest.MockPocoChild>>()
                .WhereProperty["ID"].IsEqualTo(2);
            var e = mockRepos.DeepLoadModel(query) 
                as DataModel<DataModelRelationshipMetadataTest.MockPocoChild>;
            Assert.IsNotNull(e, "Nothing was returned from DeepLoad query.");
            Assert.IsNotNull(e.Entity.MockPoco, "Child object not assigned in DeepLoad");
            Assert.IsTrue(e.Entity.MockPoco.MockDecimalValue == e.Entity.MockDecimalValue, 
                "Child object field value mismatch");
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
        public class Contact : DataModel
        {
            [DataModelColumn("contact_id", IsPrimaryKey = true)]
            public int ID {
                get { return (int) base["ID"]; }
                set { base["ID"] = value; }
            }
            [DataModelColumn("contact_name")]
            public string Name
            {
                get { return (string) base["Name"]; }
                set { base["Name"] = value; }
            }

            [DataModelColumn("contact_phone")]
            public string Phone {
                get { return (string) base["Phone"]; }
                set { base["Phone"] = value; }
            }
            [ForeignDataModel(LocalColumn = "company_id", 
                Relationship=Relationship.ManyToOne)]
            public Company Company { get; set; }
        }

        [DataModelTable("Group")]
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

        [DataModelTable("User")]
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
        public void DeepLoadEntitiesTest()
        {
            DataProviderBase mockRepos = new MemoryDataProvider();
            var table = ((MemoryDataProvider)mockRepos).AddTable("mock_table");
            PopulateMockTable(table);
            var table2 = ((MemoryDataProvider)mockRepos).AddTable("mock_table2");
            PopulateMockTable(table2);
            var query = DataModel<DataModelRelationshipMetadataTest.MockPocoChild>.NewQuery();
            DataModelCollection<DataModel<DataModelRelationshipMetadataTest.MockPocoChild>>
                entities = mockRepos.DeepLoadModels(query);
            Assert.IsNotNull(entities);
            Assert.IsTrue(entities.Count == 4);
            Assert.IsTrue(entities[0].Entity.MockPoco != null);
            Assert.IsTrue(entities[3].Entity.MockPoco != null);
            Assert.IsTrue(entities[2].Entity.MockPoco.MockDecimalValue
                          .Equals(entities[2].Entity.MockDecimalValue));
        }

        [TestMethod]
        public void DeepLoadEntityOneToManyTest()
        {
            var companyTable = CreateAndPopulateMockTable("Company");
            var contactTable = CreateAndPopulateMockTable("Contact");

            var memProvider = new MemoryDataProvider();
            memProvider.AddTable(companyTable);
            memProvider.AddTable(contactTable);
            var query = new DataModelQuery<DataModel<Company>>()
                .WhereProperty["ID"].IsEqualTo(1);
            var decompany = memProvider.DeepLoadModel(query) as DataModel<Company>;
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
            var companyTable = CreateAndPopulateMockTable("Company");
            var contactTable = CreateAndPopulateMockTable("Contact");

            var memProvider = new MemoryDataProvider();
            memProvider.AddTable(companyTable);
            memProvider.AddTable(contactTable);
            var query = new DataModelQuery<DataModel<Company>>()
                .WhereProperty["ID"].IsEqualTo(2);
            var decompany = memProvider.DeepLoadModel(query) as DataModel<Company>;
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
            var companyTable = CreateAndPopulateMockTable("Company");
            var contactTable = CreateAndPopulateMockTable("Contact");

            var memProvider = new MemoryDataProvider();
            memProvider.AddTable(companyTable);
            memProvider.AddTable(contactTable);
            var query = new DataModelQuery<Contact>()
                .WhereProperty["ID"].IsEqualTo(1);
            var contact = memProvider.DeepLoadModel(query) as Contact;
            Assert.IsNotNull(contact);
            Assert.IsTrue(contact.Name == "Bobby Joe");
            Assert.IsNotNull(contact.Company);
            Assert.IsTrue(contact.Company.CompanyName == "Foobar, Ltd.");
        }

        [TestMethod]
        public void DeepLoadEntityManyToOneTest2()
        {
            var companyTable = CreateAndPopulateMockTable("Company");
            var contactTable = CreateAndPopulateMockTable("Contact");

            var memProvider = new MemoryDataProvider();
            memProvider.AddTable(companyTable);
            memProvider.AddTable(contactTable);
            var query = new DataModelQuery<Contact>()
                .WhereProperty["ID"].IsEqualTo(2);
            var contact = memProvider.DeepLoadModel(query) as Contact;
            //Assert.IsNotNull(decontact);
            //var contact = decontact.Entity;
            Assert.IsNotNull(contact);
            Assert.IsTrue(contact.Name == "Betty Sue");
            Assert.IsNotNull(contact.Company);
            Assert.IsTrue(contact.Company.CompanyName == "Acme, Inc.");
        }

        [TestMethod]
        public void DeepLoadEntityManyToOneTest3()
        {
            var companyTable = CreateAndPopulateMockTable("Company");
            var contactTable = CreateAndPopulateMockTable("Contact");

            var memProvider = new MemoryDataProvider();
            memProvider.AddTable(companyTable);
            memProvider.AddTable(contactTable);
            var query = new DataModelQuery<Contact>()
                .WhereProperty["ID"].IsEqualTo(3);
            var contact = memProvider.DeepLoadModel(query) as Contact;
            Assert.IsNotNull(contact);
            Assert.IsTrue(contact.Name == "John Doe");
            Assert.IsNotNull(contact.Company);
            Assert.IsTrue(contact.Company.CompanyName == "Acme, Inc.");
        }

        [TestMethod]
        public void DeepLoadEntityManyToOneTest4()
        {
            var companyTable = CreateAndPopulateMockTable("Company");
            var contactTable = CreateAndPopulateMockTable("Contact");

            var memProvider = new MemoryDataProvider();
            memProvider.AddTable(companyTable);
            memProvider.AddTable(contactTable);
            var query = new DataModelQuery<Contact>()
                .WhereProperty["ID"].IsEqualTo(4);
            var contact = memProvider.DeepLoadModel(query) as Contact;
            Assert.IsNotNull(contact);
            Assert.IsTrue(contact.Name == "Jane Lane");
            Assert.IsNotNull(contact.Company);
            Assert.IsTrue(contact.Company.CompanyName == "Foobar, Ltd.");
            Assert.IsTrue(contact.Company.Contacts != null);
            Assert.IsTrue(contact.Company.Contacts.Count > 0);
        }

        [TestMethod]
        public void DeepLoadEntityManyToManyTest()
        {
            var usersTable = CreateAndPopulateMockTable("User");
            var groupsTable = CreateAndPopulateMockTable("Group");
            var groupUserTable = CreateAndPopulateMockTable("GroupUser");

            var memProvider = new MemoryDataProvider();
            memProvider.AddTable(usersTable);
            memProvider.AddTable(groupsTable);
            memProvider.AddTable(groupUserTable);
            var query = new DataModelQuery<DataModel<Group>>();
            var egroups = memProvider.DeepLoadModels(query);
            var groups = new DataModelCollection<DataModel<Group>>(egroups);

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
        public void DeepSaveEntityTest()
        {
            // todo: test all four relationship types

            var companyTable = CreateAndPopulateMockTable("Company");
            var contactTable = CreateAndPopulateMockTable("Contact");

            var memProvider = new MemoryDataProvider();
            memProvider.AddTable(companyTable);
            memProvider.AddTable(contactTable);
            var query = new DataModelQuery<DataModel<Company>>()
                .WhereProperty["ID"].IsEqualTo(1);
            var decompany = memProvider.DeepLoadModel(query);
            var company = decompany.Entity;
            company.CompanyName += "_";
            foreach (var c in company.Contacts)
                c.Name += "_";
            decompany.Save(true);

            query = new DataModelQuery<DataModel<Company>>()
                .WhereProperty["ID"].IsEqualTo(1);
            decompany = memProvider.DeepLoadModel(query);
            company = decompany.Entity;
            Assert.IsTrue(company.CompanyName.Length > 1 &&
                          company.CompanyName.EndsWith("_"));
            Assert.IsTrue(company.Contacts[0]
                              .Name.Length > 1 &&
                          company.Contacts[0]
                              .Name.EndsWith("_"));
            Assert.IsTrue(company.Contacts[company.Contacts.Count-1]
                  .Name.Length > 1 &&
              company.Contacts[company.Contacts.Count-1]
                  .Name.EndsWith("_"));
        }

        [TestMethod]
        public void DeepSaveEntitiesTest()
        {
            var companyTable = CreateAndPopulateMockTable("Company");
            var contactTable = CreateAndPopulateMockTable("Contact");

            var memProvider = new MemoryDataProvider();
            memProvider.AddTable(companyTable);
            memProvider.AddTable(contactTable);

            var query = new DataModelQuery<DataModel<Company>>()
                .WhereProperty["ID"].IsEqualTo(1);
            var decompany = memProvider.DeepLoadModel(query);
            var company = decompany.Entity;
            company.CompanyName += "_";
            foreach (var c in company.Contacts)
                c.Name += "_";

            var query2 = new DataModelQuery<DataModel<Company>>()
                .WhereProperty["ID"].IsEqualTo(2);
            var decompany2 = memProvider.DeepLoadModel(query2);
            var company2 = decompany2.Entity;
            company2.CompanyName = "_" + company2.CompanyName;
            foreach (var c in company2.Contacts)
                c.Name = "_" + c.Name;

            var col = new DataModelCollection<DataModel<Company>>();
            col.Add(decompany);
            col.Add(decompany2);
            col.DataProvider = memProvider;

            col.Save(true);

            query = new DataModelQuery<DataModel<Company>>()
                .WhereProperty["ID"].IsEqualTo(1);
            decompany = memProvider.DeepLoadModel(query);
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
                .WhereProperty["ID"].IsEqualTo(2);
            decompany = memProvider.DeepLoadModel(query);
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

        private static DataTable CreateAndPopulateMockTable(string tableName)
        {
            var ret = new DataTable(tableName);
            switch (tableName)
            {
                // one-to-many
                case "Company":
                    var companyIdCol = new DataColumn("company_id", typeof (int));
                    ret.Columns.Add(companyIdCol);
                    var companyNameCol = new DataColumn("company_name", typeof (string));
                    ret.Columns.Add(companyNameCol);
                    break;
                // one-to-many
                case "Contact":
                    var contactIdCol = new DataColumn("contact_id", typeof (int));
                    ret.Columns.Add(contactIdCol);
                    var contactNameCol = new DataColumn("contact_name", typeof (string));
                    ret.Columns.Add(contactNameCol);
                    var contactPhoneCol = new DataColumn("contact_phone", typeof (string));
                    ret.Columns.Add(contactPhoneCol);
                    var contactCompanyIdCol = new DataColumn("company_id", typeof(int));
                    ret.Columns.Add(contactCompanyIdCol);
                    break;
                // many-to-many
                case "User":
                    var userIdCol = new DataColumn("user_id", typeof (Guid));
                    ret.Columns.Add(userIdCol);
                    var userNameCol = new DataColumn("user_name", typeof (string));
                    ret.Columns.Add(userNameCol);
                    break;
                // many-to-many
                case "Group":
                    var groupIdCol = new DataColumn("group_id", typeof (Guid));
                    ret.Columns.Add(groupIdCol);
                    var groupNameCol = new DataColumn("group_name", typeof (string));
                    ret.Columns.Add(groupNameCol);
                    break;
                // many-to-many
                case "GroupUser":
                    var groupUserIdCol = new DataColumn("groupuser_id", typeof (long));
                    groupUserIdCol.AutoIncrement = true;
                    ret.Columns.Add(groupUserIdCol);
                    var groupUser_userIdCol = new DataColumn("user_id", typeof(Guid));
                    ret.Columns.Add(groupUser_userIdCol);
                    var groupUser_groupIdCol = new DataColumn("group_id", typeof (Guid));
                    ret.Columns.Add(groupUser_groupIdCol);
                    break;
                default:
                    break;
            }
            PopulateMockTable(ret);
            return ret;
        }
        private static void PopulateMockTable(DataTable table)
        {
            DataRow row;
            switch (table.TableName)
            {
                // one-to-many
                case "Company":
                    table.BeginLoadData();
                    row = table.NewRow();
                    row["company_id"] = 1;
                    row["company_name"] = "Acme, Inc.";
                    table.Rows.Add(row);
                    row = table.NewRow();
                    row["company_id"] = 2;
                    row["company_name"] = "Foobar, Ltd.";
                    table.Rows.Add(row);
                    table.EndLoadData();
                    break;
                // one-to-many
                case "Contact":
                    table.BeginLoadData();
                    row = table.NewRow();
                    row["contact_id"] = 1;
                    row["contact_name"] = "Bobby Joe";
                    row["contact_phone"] = "123-456-7890";
                    row["company_id"] = 2;
                    table.Rows.Add(row);
                    row = table.NewRow();
                    row["contact_id"] = 2;
                    row["contact_name"] = "Betty Sue";
                    row["contact_phone"] = "987-654-3210";
                    row["company_id"] = 1;
                    table.Rows.Add(row);
                    row = table.NewRow();
                    row["contact_id"] = 3;
                    row["contact_name"] = "John Doe";
                    row["contact_phone"] = "444-444-4444";
                    row["company_id"] = 1;
                    table.Rows.Add(row);
                    row = table.NewRow();
                    row["contact_id"] = 4;
                    row["contact_name"] = "Jane Lane";
                    row["contact_phone"] = "898-989-8989";
                    row["company_id"] = 2;
                    table.Rows.Add(row);
                    table.EndLoadData();
                    break;
                // many-to-many
                case "User":
                    table.BeginLoadData();
                    row = table.NewRow();
                    row["user_id"] = new Guid("AD62B917-0EF9-48e1-980B-3A717E329E2E");
                    row["user_name"] = "Bob";
                    table.Rows.Add(row);
                    row = table.NewRow();
                    row["user_id"] = new Guid("5A401D28-C2CA-4b3b-9573-D24D2E3FDF27");
                    row["user_name"] = "Chris";
                    table.Rows.Add(row);
                    row = table.NewRow();
                    row["user_id"] = new Guid("7EBC702F-C83A-4f8f-879B-F7D800CF390A");
                    row["user_name"] = "Fred";
                    table.Rows.Add(row);
                    table.EndLoadData();
                    break;
                // many-to-many
                case "Group":
                    table.BeginLoadData();
                    row = table.NewRow();
                    row["group_id"] = new Guid("BF59F59C-26BA-4e31-BEB0-024B5A219D6D");
                    row["group_name"] = "AppUsers";
                    table.Rows.Add(row);
                    row = table.NewRow();
                    row["group_id"] = new Guid("721BCF79-C721-4413-A7EA-F0A75D5E6AA2");
                    row["group_name"] = "Administrators";
                    table.Rows.Add(row);
                    table.EndLoadData();
                    break;
                // many-to-many
                case "GroupUser":
                    table.BeginLoadData();
                    row = table.NewRow(); // Bob -> AppUser
                    row["user_id"] = new Guid("AD62B917-0EF9-48e1-980B-3A717E329E2E");
                    row["group_id"] = new Guid("BF59F59C-26BA-4e31-BEB0-024B5A219D6D");
                    table.Rows.Add(row);
                    row = table.NewRow(); // Chris -> AppUser
                    row["user_id"] = new Guid("5A401D28-C2CA-4b3b-9573-D24D2E3FDF27");
                    row["group_id"] = new Guid("BF59F59C-26BA-4e31-BEB0-024B5A219D6D");
                    table.Rows.Add(row);
                    row = table.NewRow(); // Chris -> Administrator
                    row["user_id"] = new Guid("5A401D28-C2CA-4b3b-9573-D24D2E3FDF27");
                    row["group_id"] = new Guid("721BCF79-C721-4413-A7EA-F0A75D5E6AA2");
                    table.Rows.Add(row);
                    row = table.NewRow(); // Fred -> Administrator
                    row["user_id"] = new Guid("7EBC702F-C83A-4f8f-879B-F7D800CF390A");
                    row["group_id"] = new Guid("721BCF79-C721-4413-A7EA-F0A75D5E6AA2");
                    table.Rows.Add(row);
                    table.EndLoadData();
                    break;
                default:
                    var col = new DataColumn("customentity_id", typeof (int)) {AutoIncrement = true};
                    table.Columns.Add(col);
                    col = new DataColumn("string_value", typeof (string));
                    table.Columns.Add(col);
                    col = new DataColumn("money_value", typeof (decimal));
                    table.Columns.Add(col);
                    table.BeginLoadData();
                    row = table.NewRow(); // 0
                    row["string_value"] = "abc";
                    row["money_value"] = "4.25";
                    table.Rows.Add(row);
                    row = table.NewRow(); // 1
                    row["string_value"] = "def";
                    row["money_value"] = "2.76";
                    table.Rows.Add(row);
                    row = table.NewRow(); // 2
                    row["string_value"] = "ghi";
                    row["money_value"] = "3.99";
                    table.Rows.Add(row);
                    row = table.NewRow(); // 3
                    row["string_value"] = "jkl";
                    row["money_value"] = "9.49";
                    table.Rows.Add(row);
                    table.EndLoadData();
                    break;
            }
        }
    }
}
