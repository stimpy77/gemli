using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Gemli.Data.Providers
{
    /// <summary>
    /// Provides an ADO.NET database provider
    /// for Gemli.Data using ANSI SQL when supplied with a
    /// <see cref="DbProviderFactory"/>. This provider
    /// class is not optimized.
    /// </summary>
    public class DbDataProvider : DataProviderBase
    {
        #region Constructor variations

        /// <summary>
        /// Constructs a DbProvider object using the
        /// specified <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <param name="dbFactory"></param>
        public DbDataProvider(DbProviderFactory dbFactory)
        {
            DbFactory = dbFactory;
        }

        /// <summary>
        /// Constructs a DbProvider object using the
        /// specified <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <param name="dbFactory"></param>
        /// <param name="connString"></param>
        public DbDataProvider(DbProviderFactory dbFactory, string connString)
            : this(dbFactory)
        {
            ConnectionString = connString;
        }

        /// <summary>
        /// Constructs a DbProvider object using the
        /// specified <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <param name="dbFactory"></param>
        /// <param name="connStringSettings">
        /// The connection string settings in app.config or web.config as
        /// referenced by <see cref="System.Configuration.ConfigurationManager" />
        /// </param>
        public DbDataProvider(DbProviderFactory dbFactory,
                              ConnectionStringSettings connStringSettings)
            : this(dbFactory, connStringSettings.Name, connStringSettings.ConnectionString)
        {
        }

        /// <summary>
        /// Constructs a DbProvider object using the
        /// specified <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <param name="dbFactory"></param>
        /// <param name="connectionName"></param>
        /// <param name="connectionString"></param>
        public DbDataProvider(DbProviderFactory dbFactory,
                              string connectionName, string connectionString)
            : this(dbFactory)
        {
            ConnectionName = connectionName;
            ConnectionString = connectionString;
        }

        #endregion

        /// <summary>
        /// Gets or sets the connection name associated with the <see cref="ConnectionString"/>.
        /// </summary>
        public string ConnectionName { get; set; }

        /// <summary>
        /// Returns the <see cref="DbProviderFactory"/> used for
        /// performing database operations.
        /// </summary>
        public DbProviderFactory DbFactory { get; private set; }

        /// <summary>
        /// Gets or sets the connection string used to set up
        /// a new connection to this database provider.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connString))
                {
                    if (string.IsNullOrEmpty(ConnectionName))
                    {
                        return null;
                    }
                    return (_connString =
                            ConfigurationManager.ConnectionStrings[ConnectionName]
                                .ConnectionString);
                }
                return _connString;
            }
            set { _connString = value; }
        }

        private string _connString { get; set; }

        /// <summary>
        /// Returns true, indicating that this data provider
        /// supports creating and working with DbTransactions.
        /// </summary>
        public override bool SupportsTransactions
        {
            get { return true; }
        }

        private DbConnection CreateAndOpenConnection()
        {
            DbConnection ret = DbFactory.CreateConnection();
            ret.ConnectionString = ConnectionString;
            ret.Open();
            return ret;
        }

        /// <summary>
        /// Overridable. Generates an instance of a <see cref="DbDataProviderCommandBuilder{TModel}"/>
        /// which builds <see cref="DbCommand"/>s for execution by a vendor-specific
        /// <see cref="DbDataProvider"/> instance.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        protected virtual DbDataProviderCommandBuilder<TModel> CreateCommandBuilder<TModel>(TModel dataModel)
            where TModel : DataModel
        {
            var ret = new DbDataProviderCommandBuilder<TModel>(this, dataModel);
            return ret;
        }

        /// <summary>
        /// Overridable. Generates an instance of a <see cref="DbDataProviderCommandBuilder{TModel}"/>
        /// which builds <see cref="DbCommand"/>s for execution by a vendor-specific
        /// <see cref="DbDataProvider"/> instance.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual DbDataProviderCommandBuilder<TModel> CreateCommandBuilder<TModel>(DataModelQuery<TModel> query)
            where TModel : DataModel
        {
            var ret = new DbDataProviderCommandBuilder<TModel>(this, query);
            return ret;
        }

        /// <summary>
        /// Loads the first <see cref="DataModel"/> that
        /// the specified <paramref name="query"/> finds,
        /// within the specified database <paramref name="transactionContext"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public override TModel LoadModel<TModel>(DataModelQuery<TModel> query, DbTransaction transactionContext)
        {
            DbConnection tmpConn = null;
            if (transactionContext == null)
            {
                tmpConn = CreateAndOpenConnection();
                transactionContext = tmpConn.BeginTransaction(ProviderDefaults.IsolationLevel);
            }
            try
            {
                DbConnection conn = transactionContext.Connection;
                DbDataProviderCommandBuilder<TModel> cmdBuilder = CreateCommandBuilder(query);
                using (DbCommand cmd = cmdBuilder.CreateDbCommand(transactionContext)) // conn.CreateCommand())
                {
                    cmd.Transaction = transactionContext;

                    using (DbDataReader dr = cmd.ExecuteReader(cmdBuilder.ExecuteBehavior ?? CommandBehavior.SingleRow))
                    {
                        if (dr.Read())
                        {
                            var ret = (TModel) Activator.CreateInstance(typeof (TModel));
                            ret.DataProvider = this;
                            ret.Load(dr);
                            return ret;
                        }
                        return null;
                    }
                }
            }
            finally
            {
                if (tmpConn != null)
                {
                    transactionContext.Commit();
                    tmpConn.Close();
                }
            }
        }

        /// <summary>
        /// Loads a set of <see cref="DataModel"/> objects
        /// using the specified <paramref name="query"/> within the
        /// specified database <paramref name="transactionContext"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public override DataModelCollection<TModel> LoadModels<TModel>(
            DataModelQuery<TModel> query, DbTransaction transactionContext)
        {
            DataModelMap demap = DataModel.GetMapping<TModel>();

            DbConnection tmpConn = null;
            if (transactionContext == null)
            {
                tmpConn = CreateAndOpenConnection();
                transactionContext = tmpConn.BeginTransaction(ProviderDefaults.IsolationLevel);
            }
            DbConnection conn = transactionContext.Connection;
            try
            {
                DbDataProviderCommandBuilder<TModel> cmdBuilder = CreateCommandBuilder(query);
                using (DbCommand cmd = cmdBuilder.CreateDbCommand(transactionContext))
                {
                    cmd.Transaction = transactionContext;

                    using (DbDataReader dr = cmd.ExecuteReader(cmdBuilder.ExecuteBehavior ?? CommandBehavior.Default))
                    {
                        var ret = new DataModelCollection<TModel> {DataProvider = this};

                        if (query.Pagination == null ||
                            query.Pagination.ItemsPerPage == int.MaxValue ||
                            query.Pagination.ItemsPerPage == 0 ||
                            cmdBuilder.PaginationIsHandled)
                        {
                            while (dr.Read())
                            {
                                var obj = (TModel) Activator.CreateInstance(typeof (TModel));
                                obj.DataProvider = this;
                                obj.Load(dr);
                                ret.Add(obj);
                            }
                        }
                        else
                        {
                            int start = query.Pagination.Page*query.Pagination.ItemsPerPage -
                                        query.Pagination.ItemsPerPage;
                            bool eof = false;
                            for (int i = 0; i < start; i++)
                            {
                                if (!dr.Read())
                                {
                                    eof = true;
                                    break;
                                }
                            }
                            for (int i = 0; i < query.Pagination.ItemsPerPage && !eof; i++)
                            {
                                if (!dr.Read())
                                {
                                    eof = true;
                                    break;
                                }
                                var obj = (TModel) Activator.CreateInstance(typeof (TModel));
                                obj.DataProvider = this;
                                obj.Load(dr);
                                ret.Add(obj);
                            }
                        }
                        if (!string.IsNullOrEmpty(demap.TableMapping.SelectManyProcedure) &&
                            query.OrderBy.Count > 0)
                        {
                            // exec sort from last sort field to first to get correct end result
                            for (int i = query.OrderBy.Count - 1; i >= 0; i--)
                            {
                                string fld = query.OrderBy[i].GetFieldMapping(typeof (TModel)).TargetMember.Name;
                                switch (query.OrderBy[i].SortDirection)
                                {
                                    case Sort.Ascending:
                                        TModel[] asc = (from item in ret
                                                        orderby item[fld] ascending
                                                        select item).ToArray();
                                        ret = new DataModelCollection<TModel>(asc);
                                        break;
                                    case Sort.Descending:
                                        TModel[] desc = (from item in ret
                                                         orderby item[fld] descending
                                                         select item).ToArray();
                                        ret = new DataModelCollection<TModel>(desc);
                                        break;
                                }
                            }
                        }
                        return ret;
                    }
                }
            }
            finally
            {
                if (tmpConn != null)
                {
                    transactionContext.Commit();
                    tmpConn.Close();
                }
            }
        }

        /// <summary>
        /// Saves the changes that were made to the
        /// specified <paramref name="dataModel"/>, within the specified
        /// database <paramref name="transactionContext"/>. 
        /// <remarks>
        /// The specified object must have the Deleted property set to true,
        /// IsDirty evaluated as true, or IsNew evaluated as true, or else
        /// the entity will not be saved.
        /// </remarks>
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="transactionContext"></param>
        public override void SaveModel<TModel>(TModel dataModel, DbTransaction transactionContext)
        {
            if (!dataModel.IsNew &&
                !dataModel.IsDirty &&
                !dataModel.MarkDeleted)
                return;

            DataModelMap demap = DataModelMap.GetEntityMapping(dataModel.GetType());

            DbConnection tmpConn = null;
            if (transactionContext == null)
            {
                tmpConn = CreateAndOpenConnection();
                transactionContext = tmpConn.BeginTransaction();
            }
            try
            {
                DbDataProviderCommandBuilder<TModel> cmdBuilder = CreateCommandBuilder(dataModel);
                using (DbCommand cmd = cmdBuilder.CreateDbCommand(transactionContext))
                {
                    cmd.Transaction = transactionContext;
                    bool wasNew = dataModel.IsNew;

                    cmd.ExecuteNonQuery();

                    if (wasNew)
                    {
                        foreach (var field in demap.FieldMappings)
                        {
                            DataModelColumnAttribute f = field.Value;
                            if (f.ReturnAsOutputOnInsert)
                            {
                                dataModel[f.TargetMember.Name] = cmd.Parameters[f.InsertParam].Value;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (tmpConn != null)
                {
                    transactionContext.Commit();
                    tmpConn.Close();
                }
            }
        }

        /// <summary>
        /// Saves the changes that were made to each of the
        /// specified <paramref name="dataEntities"/>, within the specified
        /// database <paramref name="transactionContext"/>. 
        /// <remarks>
        /// The specified entities must have the Deleted property set to true,
        /// IsDirty evaluated as true, or IsNew evaluated as true, or else
        /// the entity will not be saved.
        /// </remarks>
        /// </summary>
        /// <param name="dataEntities"></param>
        /// <param name="transactionContext"></param>
        public override void SaveModels<TModel>(DataModelCollection<TModel> dataEntities,
                                                DbTransaction transactionContext)
        {
            DbConnection tmpConn = null;
            if (transactionContext == null)
            {
                tmpConn = CreateAndOpenConnection();
                transactionContext = tmpConn.BeginTransaction();
            }
            try
            {
                foreach (TModel entity in dataEntities)
                {
                    SaveModel(entity, transactionContext);
                }
            }
            finally
            {
                if (tmpConn != null)
                {
                    transactionContext.Commit();
                    tmpConn.Close();
                }
            }
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <returns></returns>
        public override DbTransaction BeginTransaction()
        {
            return CreateAndOpenConnection().BeginTransaction();
        }

        /// <summary>
        /// Starts a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public override DbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return CreateAndOpenConnection().BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Returns the count of records that match the specified query criteria.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public override long GetCount<TModel>(DataModelQuery<TModel> query, DbTransaction transactionContext)
        {
            DbConnection tmpConn = null;
            if (transactionContext == null)
            {
                tmpConn = CreateAndOpenConnection();
                transactionContext = tmpConn.BeginTransaction();
            }
            try
            {
                var cmdBuilder = CreateCommandBuilder(query);
                cmdBuilder.SelectionItems = "COUNT(*) AS ResultValue";
                using (var cmd = cmdBuilder.CreateDbCommand(transactionContext))
                {
                    var retval = cmd.ExecuteScalar();
                    return Convert.ToInt64(retval);
                }
            }
            finally
            {
                if (tmpConn != null)
                {
                    transactionContext.Commit();
                    tmpConn.Close();
                }
            }
        }
    }
}