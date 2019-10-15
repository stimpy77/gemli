using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Gemli.Data.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Gemli.Data
{
    public partial class DbDataProviderTest
    {

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            var sqlFactory = SqlClientFactory.Instance;
            var dbProvider = new DbDataProvider(sqlFactory, TestSqlConnection);
            ProviderDefaults.AppProvider = dbProvider;
        }

        [TestInitialize()]
        public void DataModelDbRepositoryTest_Initialize()
        {
            if (TestContext.DataConnection != null)
            {
                TestSqlConnection = TestContext.DataConnection.ConnectionString;
            }
            using (var conn = new SqlConnection(TestSqlConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM mock_table";
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText
                        = "SET IDENTITY_INSERT mock_table ON\n"
                          + "INSERT INTO mock_table (customentity_id, string_value, money_value)\n"
                          + "VALUES (1, 'abc', NULL)\n"
                          + "INSERT INTO mock_table (customentity_id, string_value, money_value)\n"
                          + "VALUES (2, 'ghi', 2.2000)\n"
                          + "INSERT INTO mock_table (customentity_id, string_value, money_value)\n"
                          + "VALUES (3, 'def', 2222.0000)\n"
                          + "SET IDENTITY_INSERT mock_table OFF\n";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string _testSqlConnection;
        private static string TestSqlConnection
        {
            get
            {
                if (_testSqlConnection == null)
                {
                    var ret =
                        @"Server=.\SQLExpress;AttachDbFilename={0};Trusted_Connection=Yes;";
                    var execPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var projPath = execPath;
                    while (!projPath.ToLower().EndsWith("bin") && !projPath.ToLower().EndsWith("testresults"))
                    {
                        if (projPath.EndsWith(":")) return execPath;
                        if (projPath.Contains("/")) projPath = projPath.Substring(0, projPath.LastIndexOf("/"));
                        if (projPath.Contains("\\"))
                            projPath = projPath.Substring(0, projPath.LastIndexOf("\\"));
                    }
                    if (projPath.Contains("/")) projPath = projPath.Substring(0, projPath.LastIndexOf("/"));
                    if (projPath.Contains("\\"))
                        projPath = projPath.Substring(0, projPath.LastIndexOf("\\"));
                    var dbPath = projPath
                        + (projPath.Contains("/") ? "/" : "\\")
                        + "Tests.Gemli.Data"
                        + (projPath.Contains("/") ? "/" : "\\")
                        + @"DBDataProviderTestDB.mdf";
                    _testSqlConnection = string.Format(ret, dbPath);
                    var srcDbPath = dbPath.Replace("DBDataProviderTestDB.mdf", @"EmptyMDF\DBDataProviderTestDB.mdf");
                    var srcLogPath = srcDbPath.Replace("DBDataProviderTestDB.mdf", "DBDataProviderTestDB_log.ldf");
                    var destLogPath = dbPath.Replace("DBDataProviderTestDB.mdf", "DBDataProviderTestDB_log.ldf");
                    SetupTestDB(srcDbPath, srcLogPath, dbPath, destLogPath);
                }
                return _testSqlConnection;
            }
            set
            {
                _testSqlConnection = value;
            }
        }

        private static void SetupTestDB(
            string src_mdf_file_path, string src_log_file_path,
            string dest_mdf_file_path, string dest_log_file_path)
        {
            try
            {
                var tmp = _testSqlConnection
                    .Substring(0, _testSqlConnection.IndexOf("AttachDbFilename"))
                    + _testSqlConnection.Substring(_testSqlConnection.IndexOf(";",
                    _testSqlConnection.IndexOf("Attach")) + 1);
                using (var conn = new SqlConnection(tmp))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "master.dbo.sp_detach_db @dbname='" + dest_mdf_file_path + "'";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
            if (System.IO.File.Exists(dest_mdf_file_path))
                System.IO.File.Delete(dest_mdf_file_path);
            if (System.IO.File.Exists(dest_log_file_path))
                System.IO.File.Delete(dest_log_file_path);
            System.IO.File.Copy(src_mdf_file_path, dest_mdf_file_path);
            System.IO.File.Copy(src_log_file_path, dest_log_file_path);
            System.IO.File.SetAttributes(dest_mdf_file_path, System.IO.FileAttributes.Normal);
            System.IO.File.SetAttributes(dest_log_file_path, System.IO.FileAttributes.Normal);
            System.Threading.Thread.Sleep(100); // let system catch up with itself
            using (var conn = new SqlConnection(TestSqlConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText =
                        @"SET ANSI_NULLS ON
                        SET QUOTED_IDENTIFIER ON

                        CREATE TABLE [dbo].[mock_table](
	                        [customentity_id] [int] IDENTITY(1,1) NOT NULL,
	                        [string_value] [nvarchar](50) NULL,
	                        [money_value] [money] NULL,
                         CONSTRAINT [PK_mock_table] PRIMARY KEY CLUSTERED 
                        (
	                        [customentity_id] ASC
                        ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE 
                            = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS 
                            = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"CREATE PROCEDURE sp_mock_table_Select
                            @customentity_id int
                        AS
                        BEGIN
                            SET NOCOUNT ON;

                            SELECT [customentity_id]
                                  ,[string_value]
                                  ,[money_value]
                              FROM [dbo].[mock_table]
                             WHERE [customentity_id] = @customentity_id

                        END";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"CREATE PROCEDURE sp_mock_table_SelectMany
                            @customentity_id int = NULL,
                            @string_value nvarchar(50) = NULL,
                            @money_value money = NULL
                        AS 
                        BEGIN
                            SET NOCOUNT ON;

                            SELECT [customentity_id]
                                  ,[string_value]
                                  ,[money_value]
                              FROM [dbo].[mock_table]
                             WHERE (@customentity_id IS NULL OR customentity_id = @customentity_id)
                               AND (@string_value IS NULL OR string_value = @string_value)
                               AND (@money_value IS NULL OR money_value = @money_value)
                        END";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"CREATE PROCEDURE sp_mock_table_Insert
                            @customentity_id int OUTPUT,
                            @string_value nvarchar(50) = NULL,
                            @money_value money = NULL
                        AS
                        BEGIN
                            INSERT INTO mock_table
                            (
                                string_value,
                                money_value
                            ) VALUES (
                                @string_value,
                                @money_value
                            )

                            SET @customentity_id = SCOPE_IDENTITY()
                        END";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"CREATE PROCEDURE sp_mock_table_Update
                            @customentity_id int,
                            @string_value nvarchar(50) = NULL,
                            @money_value money = NULL
                        AS
                        BEGIN
                            UPDATE  mock_table
                            SET     string_value = @string_value,
                                    money_value = @money_value
                            WHERE   customentity_id = @customentity_id
                        END";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"CREATE PROCEDURE sp_mock_table_Delete
                        (
                            @customentity_id int
                        )
                        AS
                        BEGIN
                            DELETE FROM mock_table
                            WHERE customentity_id = @customentity_id
                        END
                        ";
                    cmd.ExecuteNonQuery();

                    // company + contact
                    cmd.CommandText =
                        @"CREATE TABLE [dbo].[company](
	                        [company_id] [int] NOT NULL,
	                        [name] [nvarchar](50) NULL,
                         CONSTRAINT [PK_company] PRIMARY KEY CLUSTERED 
                        (
	                        [company_id] ASC
                        ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"CREATE TABLE [dbo].[contact](
	                        [contact_id] [int] NOT NULL,
	                        [company_id] [int] NOT NULL,
	                        [name] [nvarchar](50) NULL,
	                        [phone] [varchar](12) NULL,
                         CONSTRAINT [PK_contact] PRIMARY KEY CLUSTERED 
                        (
	                        [contact_id] ASC
                        ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"ALTER TABLE [dbo].[contact]  WITH CHECK ADD  CONSTRAINT [FK_contact_company] FOREIGN KEY([company_id])
                        REFERENCES [dbo].[company] ([company_id])";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"ALTER TABLE [dbo].[contact] CHECK CONSTRAINT [FK_contact_company]";
                    cmd.ExecuteNonQuery();

                    ReloadData_CompanyContact();


                    // User / Group / GroupUser (many-to-many)
                    cmd.CommandText =
                        @"CREATE TABLE [dbo].[User](
	                        [user_id] [uniqueidentifier] NOT NULL,
	                        [user_name] [varchar](250) NULL,
                         CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
                        (
	                        [user_id] ASC
                        ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"CREATE TABLE [dbo].[Group](
	                        [group_id] [uniqueidentifier] NOT NULL,
	                        [group_name] [varchar](250) NULL,
                         CONSTRAINT [PK_Group] PRIMARY KEY CLUSTERED 
                        (
	                        [group_id] ASC
                        ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"CREATE TABLE [dbo].[GroupUser](
	                        [groupuser_id] [bigint] IDENTITY(1,1) NOT NULL,
	                        [user_id] [uniqueidentifier] NOT NULL,
	                        [group_id] [uniqueidentifier] NOT NULL,
                         CONSTRAINT [PK_GroupUser] PRIMARY KEY CLUSTERED 
                        (
	                        [groupuser_id] ASC
                        ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"ALTER TABLE [dbo].[GroupUser]  WITH CHECK ADD  CONSTRAINT [FK_GroupUser_Group] FOREIGN KEY([group_id])
                        REFERENCES [dbo].[Group] ([group_id])";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"ALTER TABLE [dbo].[GroupUser] CHECK CONSTRAINT [FK_GroupUser_Group]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"ALTER TABLE [dbo].[GroupUser]  WITH CHECK ADD  CONSTRAINT [FK_GroupUser_User] FOREIGN KEY([user_id])
                        REFERENCES [dbo].[User] ([user_id])";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"ALTER TABLE [dbo].[GroupUser] CHECK CONSTRAINT [FK_GroupUser_User]";
                    cmd.ExecuteNonQuery();

                    ReloadData_UserGroup();

                    cmd.CommandText =
                        @"CREATE TABLE [dbo].[idlist](
	                        [id] [int] NOT NULL,
                         CONSTRAINT [PK_idlist] PRIMARY KEY CLUSTERED 
                        (
	                        [id] ASC
                        ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                    cmd.ExecuteNonQuery();

                    ReloadData_IDList();


                    // 3-level hierarchy tables
                    cmd.CommandText =
                        @"CREATE TABLE [dbo].[HierarchyRoot](
	                        [id] [int] IDENTITY(1,1) NOT NULL,
	                        [name] [nvarchar](50) NULL,
                         CONSTRAINT [PK_HierarchyRoot] PRIMARY KEY CLUSTERED 
                        (
	                        [id] ASC
                        ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"CREATE TABLE [dbo].[HierarchyChild](
	                        [id] [int] IDENTITY(1,1) NOT NULL,
	                        [root_id] [int] NOT NULL,
	                        [name] [nvarchar](50) NULL,
                         CONSTRAINT [PK_HierarchyChild] PRIMARY KEY CLUSTERED 
                        (
	                        [id] ASC
                        ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"ALTER TABLE [dbo].[HierarchyChild]  WITH CHECK ADD  CONSTRAINT [FK_HierarchyChild_HierarchyRoot] FOREIGN KEY([root_id])
                            REFERENCES [dbo].[HierarchyRoot] ([id])";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"ALTER TABLE [dbo].[HierarchyChild] CHECK CONSTRAINT [FK_HierarchyChild_HierarchyRoot]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"CREATE TABLE [dbo].[HierarchyGrandChild](
	                        [id] [int] IDENTITY(1,1) NOT NULL,
	                        [parent_id] [int] NOT NULL,
	                        [name] [nvarchar](50) NULL
                        ) ON [PRIMARY]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"ALTER TABLE [dbo].[HierarchyGrandChild]  WITH CHECK ADD  CONSTRAINT [FK_HierarchyGrandChild_HierarchyChild] FOREIGN KEY([parent_id])
                            REFERENCES [dbo].[HierarchyChild] ([id])";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"ALTER TABLE [dbo].[HierarchyGrandChild] CHECK CONSTRAINT [FK_HierarchyGrandChild_HierarchyChild]";
                    cmd.ExecuteNonQuery();

                    ReloadData_HierarchyTables();

                }
            }
        }

        private static void ReloadData_IDList()
        {
            using (var conn = new SqlConnection(TestSqlConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    var sb = new StringBuilder();
                    for (int i=0; i<1000; i++)
                    {
                        sb.AppendLine("INSERT INTO idlist (id) VALUES (" + i.ToString() + ")");
                    }
                    cmd.CommandText = sb.ToString();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void ReloadData_CompanyContact()
        {
            using (var conn = new SqlConnection(TestSqlConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "DELETE FROM [dbo].[contact]";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM [dbo].[company]";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"INSERT INTO [dbo].[company] 
                        (
                            company_id,
                            name
                        ) VALUES (
                            1,
                            'Acme, Inc.'
                        )";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"INSERT INTO [dbo].[company] 
                        (
                            company_id,
                            name
                        ) VALUES (
                            2,
                            'Foobar, Ltd.'
                        )";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"INSERT INTO [dbo].[contact] 
                        (
                            contact_id,
                            company_id,
                            name,
                            phone
                        ) VALUES (
                            1,
                            2,
                            'Bobby Joe',
                            '123-456-7890'
                        )";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"INSERT INTO [dbo].[contact] 
                        (
                            contact_id,
                            company_id,
                            name,
                            phone
                        ) VALUES (
                            2,
                            1,
                            'Betty Sue',
                            '987-654-3210'
                        )";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"INSERT INTO [dbo].[contact] 
                        (
                            contact_id,
                            company_id,
                            name,
                            phone
                        ) VALUES (
                            3,
                            1,
                            'John Doe',
                            '444-444-4444'
                        )";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"INSERT INTO [dbo].[contact] 
                        (
                            contact_id,
                            company_id,
                            name,
                            phone
                        ) VALUES (
                            4,
                            2,
                            'Jane Lane',
                            '898-989-8989'
                        )";
                    cmd.ExecuteNonQuery();

                }
            }
        }

        private static void ReloadData_UserGroup()
        {
            using (var conn = new SqlConnection(TestSqlConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;

                    // users
                    cmd.CommandText =
                        @"INSERT INTO [dbo].[User]
                        (
                            user_id,
                            user_name
                        ) VALUES (
                            'AD62B917-0EF9-48e1-980B-3A717E329E2E',
                            'Bob'
                        )";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"INSERT INTO [dbo].[User]
                        (
                            user_id,
                            user_name
                        ) VALUES (
                            '5A401D28-C2CA-4b3b-9573-D24D2E3FDF27',
                            'Chris'
                        )";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"INSERT INTO [dbo].[User]
                        (
                            user_id,
                            user_name
                        ) VALUES (
                            '7EBC702F-C83A-4f8f-879B-F7D800CF390A',
                            'Fred'
                        )";
                    cmd.ExecuteNonQuery();

                    // groups
                    cmd.CommandText =
                        @"INSERT INTO [dbo].[Group]
                        (
                            group_id,
                            group_name
                        ) VALUES (
                            'BF59F59C-26BA-4e31-BEB0-024B5A219D6D',
                            'AppUsers'
                        )";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        @"INSERT INTO [dbo].[Group]
                        (
                            group_id,
                            group_name
                        ) VALUES (
                            '721BCF79-C721-4413-A7EA-F0A75D5E6AA2',
                            'Administrators'
                        )";
                    cmd.ExecuteNonQuery();

                    // groups-users
                    cmd.CommandText =
                        @"INSERT INTO [dbo].[GroupUser]
                        (
                            user_id,
                            group_id
                        ) VALUES (
                            'AD62B917-0EF9-48e1-980B-3A717E329E2E',
                            'BF59F59C-26BA-4e31-BEB0-024B5A219D6D'
                        )";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"INSERT INTO [dbo].[GroupUser]
                        (
                            user_id,
                            group_id
                        ) VALUES (
                            '5A401D28-C2CA-4b3b-9573-D24D2E3FDF27',
                            'BF59F59C-26BA-4e31-BEB0-024B5A219D6D'
                        )";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"INSERT INTO [dbo].[GroupUser]
                        (
                            user_id,
                            group_id
                        ) VALUES (
                            '5A401D28-C2CA-4b3b-9573-D24D2E3FDF27',
                            '721BCF79-C721-4413-A7EA-F0A75D5E6AA2'
                        )";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        @"INSERT INTO [dbo].[GroupUser]
                        (
                            user_id,
                            group_id
                        ) VALUES (
                            '7EBC702F-C83A-4f8f-879B-F7D800CF390A',
                            '721BCF79-C721-4413-A7EA-F0A75D5E6AA2'
                        )";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void ReloadData_HierarchyTables()
        {
            using (var conn = new SqlConnection(TestSqlConnection))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = @"DELETE FROM [dbo].[HierarchyGrandChild]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"DELETE FROM [dbo].[HierarchyChild]";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"DELETE FROM [dbo].[HierarchyRoot]";
                    cmd.ExecuteNonQuery();

                }
            }
        }
    }
}
