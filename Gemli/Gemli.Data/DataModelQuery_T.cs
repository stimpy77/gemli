using System;
using System.Collections.Generic;
using System.Data.Common;
using Gemli.Data.Providers;

namespace Gemli.Data
{
    /// <summary>
    /// Describes the selection and filters 
    /// to be used to collect entity data from a database.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class DataModelQuery<TModel> : IDataModelQuery //: DataModelQuery 
        where TModel : DataModel
    {
        public DataModelQuery()
        {
            Conditions = new List<DataModelQueryCondition<TModel>>();
        }

        /// <summary>
        /// Gets or sets a raw expression such as a SQL literal that can be used
        /// in a WHERE clause.
        /// </summary>
        public string RawExpression { get; set; }

        /// <summary>
        /// Returns a <see cref="DataModelQueryCondition&lt;T&gt;"/> that contains
        /// an indexer and can be used to describe a comparison
        /// against the specified CLR property/field.
        /// <example><code>myQuery.WhereProperty["MyProperty"].IsEqualTo(3)</code>
        /// describes a condition where the database field that is mapped to the CLR
        /// property/field "MyProperty" is exactly equal to 3.</example>
        /// </summary>
        public new DataModelQueryCondition<TModel> WhereProperty
        {
            get
            {
                var ret = new DataModelQueryCondition<TModel>(
                    FieldMappingKeyType.ClrMember, this);
                return ret;
            }
        }

        /// <summary>
        /// Gets or sets the conditions to be used for the query.
        /// </summary>
        public List<DataModelQueryCondition<TModel>> Conditions { get; set; }

        /// <summary>
        /// Returns a <see cref="DataModelQueryCondition&lt;T&gt;"/> that contains
        /// an indexer and can be used to describe a comparison
        /// against the specified database column name.
        /// <example><code>myQuery.WhereMappedColumn["MyField"].IsEqualTo(3)</code>
        /// describes a condition where the value of the database column "MyField"
        /// is exactly equal to 3.</example>
        /// </summary>
        public DataModelQueryCondition<TModel> WhereColumn
        {
            get
            {
                var ret = new DataModelQueryCondition<TModel>(
                    FieldMappingKeyType.DbColumn, this);
                return ret;
            }
        }

        IDataModelQueryCondition IDataModelQuery.WhereColumn
        {
            get
            {
                return WhereColumn;
            }
        }

        IDataModelQueryCondition IDataModelQuery.WhereProperty
        {
            get
            {
                return WhereProperty;
            }
        }

        #region OrderBy / AddSortItem

        /// <summary>
        /// Adds a field to the OrderBy collection.
        /// The <paramref name="fieldName"/> parameter
        /// is assumed to be a CLR member name, not a DB
        /// column name.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public new DataModelQuery<TModel> AddSortItem(string fieldName)
        {
            return AddSortItem(fieldName, false);
        }

        /// <summary>
        /// Adds a field to the OrderBy collection.
        /// The <paramref name="fieldName"/> parameter
        /// is assumed to be a CLR member name, not a DB
        /// column name.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public new DataModelQuery<TModel> AddSortItem(string fieldName, Sort sortDirection)
        {
            return AddSortItem(fieldName, false, sortDirection);
        }

        /// <summary>
        /// Adds a field to the OrderBy collection.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="matchOnDbColumn">
        /// If true, matches on the mapped DB column name 
        /// rather than the mapped CLR member name.
        /// </param>
        /// <returns></returns>
        public new DataModelQuery<TModel> AddSortItem(string fieldName, bool matchOnDbColumn)
        {
            return AddSortItem(fieldName, matchOnDbColumn, Sort.Ascending);
        }

        /// <summary>
        /// Adds a field to the OrderBy collection
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="keyType"></param>
        /// <returns></returns>
        public new DataModelQuery<TModel> AddSortItem(string fieldName, FieldMappingKeyType keyType)
        {
            return AddSortItem(fieldName, keyType, Sort.Ascending);
        }

        /// <summary>
        /// Adds a field to the OrderBy collection.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="matchOnDbColumn">
        /// If true, matches on the mapped DB column name 
        /// rather than the mapped CLR member name.
        /// </param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public DataModelQuery<TModel> AddSortItem(string fieldName, bool matchOnDbColumn, Sort sortOrder)
        {
            return AddSortItem(fieldName, matchOnDbColumn
                ? FieldMappingKeyType.DbColumn
                : FieldMappingKeyType.ClrMember, sortOrder);
        }

        /// <summary>
        /// Adds a field to the OrderBy collection
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="keyType"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public DataModelQuery<TModel> AddSortItem(string fieldName, FieldMappingKeyType keyType, Sort sortOrder)
        {
            (OrderBy ?? (OrderBy = new List<SortItem>()))
                .Add(new SortItem(fieldName, keyType, sortOrder));
            return this;
        }

        #endregion

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel"/>.
        /// </summary>
        /// <returns></returns>
        public TModel SelectFirst()
        {
            return SelectFirst(ProviderDefaults.AppProvider);
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public TModel SelectFirst(DataProviderBase provider)
        {
            return SelectFirst(false, provider);
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="deep"></param>
        /// <returns></returns>
        public TModel SelectFirst(bool deep)
        {
            return SelectFirst(deep, ProviderDefaults.AppProvider, null);
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public TModel SelectFirst(bool deep, DataProviderBase provider)
        {
            return SelectFirst(deep, provider, null);
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public TModel SelectFirst(DataProviderBase provider, DbTransaction transactionContext)
        {
            return SelectFirst(false, provider, transactionContext);
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public TModel SelectFirst(bool deep, DataProviderBase provider, DbTransaction transactionContext)
        {
            return deep ? provider.DeepLoadModel(this, transactionContext)
                        : provider.LoadModel(this, transactionContext);
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public TModel SelectFirst(int? depth, DataProviderBase provider, DbTransaction transactionContext)
        {
            return provider.DeepLoadModel(this, depth, transactionContext);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <returns></returns>
        public DataModelCollection<TModel> SelectMany()
        {
            return SelectMany(false, ProviderDefaults.AppProvider);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> SelectMany(DataProviderBase provider)
        {
            return SelectMany(false, provider);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> SelectMany(DbTransaction transactionContext)
        {
            return SelectMany(false, ProviderDefaults.AppProvider, transactionContext);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="deep"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> SelectMany(bool deep)
        {
            return SelectMany(deep, ProviderDefaults.AppProvider);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> SelectMany(bool deep, DataProviderBase provider)
        {
            return SelectMany(false, provider, null);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> SelectMany(DataProviderBase provider, DbTransaction transactionContext)
        {
            return SelectMany(false, provider, transactionContext);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> SelectMany(bool deep, DbTransaction transactionContext)
        {
            return SelectMany(deep, ProviderDefaults.AppProvider, transactionContext);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> SelectMany(bool deep, DataProviderBase provider,
                                                      DbTransaction transactionContext)
        {
            return deep ? provider.DeepLoadModels(this, transactionContext)
                        : provider.LoadModels(this, transactionContext);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel">TModel</typeparamref>.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> SelectMany(int? depth, DataProviderBase provider,
                                                      DbTransaction transactionContext)
        {
            return provider.DeepLoadModels(this, depth, transactionContext);
        }

        /// <summary>
        /// Returns a record count for the current query using 
        /// the <see cref="ProviderDefaults.AppProvider"/>.
        /// </summary>
        /// <returns></returns>
        public long SelectCount()
        {
            return SelectCount(ProviderDefaults.AppProvider);
        }

        /// <summary>
        /// Returns a record count for the current query.
        /// </summary>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public long SelectCount(DbTransaction transactionContext)
        {
            return Providers.ProviderDefaults.AppProvider.GetCount(this, transactionContext);
        }

        /// <summary>
        /// Returns a record count for the current query.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public long SelectCount(DataProviderBase provider)
        {
            return SelectCount(provider, null);
        }

        /// <summary>
        /// Returns a record count for the current query.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public long SelectCount(DataProviderBase provider, DbTransaction transactionContext)
        {
            return provider.GetCount(this, transactionContext);
        }

        /// <summary>
        /// Gets or sets the <see cref="QueryPagination{TModel}"/> setting for this query.
        /// </summary>
        public QueryPagination<TModel> Pagination { get; set; }

        /// <summary>
        /// Creates a <see cref="QueryPagination{TModel}"/> setting for this query
        /// that chains back to this query after using its indexer and its 
        /// .OfItemsPerPage(..) method.
        /// </summary>
        /// <example>query.Page[3].OfItemsPerPage(20);</example>
        public QueryPagination<TModel> Page
        {
            get
            {
                return Pagination = new QueryPagination<TModel>(this);
            }
        }


        /// <summary>
        /// Gets or sets a list of <see cref="SortItem"/>s
        /// that are used to generate an ORDER BY clause.
        /// </summary>
        public List<SortItem> OrderBy
        {
            get
            {
                return _OrderBy ?? (_OrderBy = new List<SortItem>());
            }
            set { _OrderBy = value; }
        }
        private List<SortItem> _OrderBy { get; set; }

        /// <summary>
        /// Describes a field to be included in an ORDER BY clause in SQL.
        /// </summary>
        public class SortItem
        {
            /// <summary>
            /// Constructs a SortItem using a CLR member name
            /// as the field name to be sorted on.
            /// </summary>
            /// <param name="clrMemberName"></param>
            public SortItem(string clrMemberName)
                : this(clrMemberName, false)
            {
            }

            /// <summary>
            /// Constructs a SortItem using the specified
            /// field name. The <paramref name="matchOnDbColumn"/>
            /// parameter determines whether to match the field
            /// name on the CLR member name or on the DB column
            /// name. 
            /// </summary>
            /// <param name="fieldName"></param>
            /// <param name="matchOnDbColumn">
            /// Determines whether to match the field
            /// name on the CLR member name or on the DB column
            /// name.
            /// </param>
            public SortItem(string fieldName, bool matchOnDbColumn)
                : this(fieldName, matchOnDbColumn
                ? FieldMappingKeyType.DbColumn
                : FieldMappingKeyType.ClrMember)
            { }

            /// <summary>
            /// Constructs a SortItem using the specified field name.
            /// The <paramref name="keyType"/> parameter determines
            /// whether to match the field name on the CLR member name
            /// or on the DB column name.
            /// </summary>
            /// <param name="fieldName"></param>
            /// <param name="keyType">
            /// Determines whether to match the field
            /// name on the CLR member name or on the DB column
            /// name.
            /// </param>
            public SortItem(string fieldName, FieldMappingKeyType keyType)
                : this(fieldName, keyType, Sort.Ascending)
            {
            }

            /// <summary>
            /// Constructs a SortItem using the specified field name.
            /// The <paramref name="matchOnDbColumn"/> parameter determines
            /// whether to match the field name on the CLR member name
            /// or ont the DB Column name.
            /// </summary>
            /// <param name="fieldName"></param>
            /// <param name="matchOnDbColumn">
            /// Determines whether to match the field
            /// name on the CLR member name or on the DB column
            /// name.
            /// </param>
            /// <param name="sortDirection"></param>
            public SortItem(string fieldName, bool matchOnDbColumn, Sort sortDirection)
                : this(fieldName, matchOnDbColumn
                ? FieldMappingKeyType.DbColumn
                : FieldMappingKeyType.ClrMember)
            { }

            /// <summary>
            /// Constructs a SortItem using the specified field name.
            /// </summary>
            /// <param name="fieldName"></param>
            /// <param name="keyType">
            /// Determines whether to match the field
            /// name on the CLR member name or on the DB column
            /// name.
            /// </param>
            /// <param name="sortDirection"></param>
            public SortItem(string fieldName, FieldMappingKeyType keyType, Sort sortDirection)
            {
                FieldName = fieldName;
                FindFieldMappingKeyType = keyType;
                SortDirection = sortDirection;
            }

            /// <summary>
            /// Specifies the field name to sort on.
            /// </summary>
            public string FieldName { get; set; }

            /// <summary>
            /// Determines whether to match the field
            /// name on the CLR member name or on the DB column
            /// name.
            /// </summary>
            public FieldMappingKeyType FindFieldMappingKeyType { get; set; }

            /// <summary>
            /// Determines which direction (ascending or descending)
            /// to sort the result set on the specified field. 
            /// </summary>
            public Sort SortDirection { get; set; }

            /// <summary>
            /// Gets the field mapping used to resolve this field reference.
            /// </summary>
            public DataModelColumnAttribute GetFieldMapping(Type context)
            {
                var fmaps = DataModelMap.GetEntityMapping(context).FieldMappings;
                if (FindFieldMappingKeyType == Data.FieldMappingKeyType.ClrMember)
                {
                    return fmaps[FieldName];
                }
                foreach (var fld in fmaps)
                {
                    if (fld.Value.ColumnName == FieldName) return fld.Value;
                }
                return null;
            }
        }
    }
}