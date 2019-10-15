using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Gemli.Reflection;

namespace Gemli.Data.Providers
{
    /// <summary>
    /// Consolidates the command and mapping properties that are required to
    /// create a <see cref="DbCommand"/> that will work properly with a
    /// <see cref="DbDataProvider"/> implementation.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class DbDataProviderCommandBuilder<TModel> where TModel : DataModel
    {
        #region SqlStatement enum

        /// <summary>
        /// The standard CRUD operations, in SQL semantics: 
        /// SELECT, INSERT, UPDATE, and DELETE
        /// </summary>
        public enum SqlStatement
        {
            /// <summary>
            /// SELECT statement
            /// </summary>
            SELECT,
            /// <summary>
            /// INSERT statement
            /// </summary>
            INSERT,
            /// <summary>
            /// UPDATE statement
            /// </summary>
            UPDATE,
            /// <summary>
            /// DELETE statement
            /// </summary>
            DELETE
        }

        #endregion

        private DbCommandBuilder _CommandBuilder;
        private bool Inited;

        /// <summary>
        /// Constructs the builder object with the specified provider.
        /// </summary>
        /// <param name="provider"></param>
        public DbDataProviderCommandBuilder(DbDataProvider provider)
        {
            Provider = provider;
            StatementCommand = SqlStatement.SELECT;
            MaxRowCount = long.MaxValue;
        }

        /// <summary>
        /// Constructs the builder object with the specified provider and model.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="model"></param>
        public DbDataProviderCommandBuilder(DbDataProvider provider, DataModel model)
            : this(provider)
        {
            DataModel = model;
        }

        /// <summary>
        /// Constructs the builder object with the specifeid provider and query.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="query"></param>
        public DbDataProviderCommandBuilder(DbDataProvider provider, DataModelQuery<TModel> query)
            : this(provider)
        {
            StatementCommand = SqlStatement.SELECT;
            Query = query;
        }

        /// <summary>
        /// Gets or sets the command statement to execute, i.e. SELECT or INSERT.
        /// </summary>
        public SqlStatement StatementCommand { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataModel"/> that is associated
        /// with this command.
        /// </summary>
        public DataModel DataModel { get; set; }

        /// <summary>
        /// Gets or sets the table mapping object from which a FROM 
        /// or INTO subclause can be built in SQL.
        /// </summary>
        public DataModelTableAttribute Table { get; set; }

        /// <summary>
        /// If specified, indicates instructions to add after the
        /// <see cref="StatementCommand"/> is added to the command text.
        /// For example, if this property value is "TOP 1", and
        /// the <see cref="StatementCommand"/> property is 
        /// <see cref="SqlStatement.SELECT"/>, the resulting 
        /// command text would contain "SELECT TOP 1".
        /// </summary>
        public string StatementCommandSuffix { get; set; }

        /// <summary>
        /// If specified, indicates semantics to add after the
        /// selection field list.
        /// </summary>
        public string SelectionItemsListSuffix { get; set; }

        /// <summary>
        /// Gets or sets the query object that specifies the conditions
        /// and sort order.
        /// </summary>
        public DataModelQuery<TModel> Query { get; set; }

        /// <summary>
        /// If specified, appends the specified value to the 
        /// WHERE clause, and creates a WHERE clause with it
        /// if one did not already exist. For example, if the
        /// where clause is "WHERE A=B" and this property's
        /// value is set to "rowid &lt; 2" then the resulting
        /// WHERE clause would become 
        /// "WHERE A=B AND rowid &lt; 2".
        /// </summary>
        public string WhereClauseAdditionalCondition { get; set; }

        /// <summary>
        /// If specified, prepends the entire command text
        /// with the specified prefix.
        /// </summary>
        public string CommandTextPrefix { get; set; }

        /// <summary>
        /// If specified, appends the entire command text
        /// with the specified suffix. For example, if the
        /// generated command text is 
        /// "SELECT A FROM B WHERE A>3 ORDER BY A",
        /// and this property's value is set to "LIMIT 1",
        /// then the resulting comamnd text would be
        /// "SELECT A FROM B WHERE A>3 ORDER BY A LIMIT 1".
        /// </summary>
        public string CommandTextSuffix { get; set; }

        /// <summary>
        /// If specified, follows the <see cref="CommandTextSuffix"/>
        /// with another command.
        /// </summary>
        public string CommandTextSecondaryCommand { get; set; }

        /// <summary>
        /// Gets the <see cref="DbDataProvider"/> that will be consuming
        /// this object.
        /// </summary>
        public DbDataProvider Provider { get; private set; }

        /// <summary>
        /// Gets or sets the maximum number of rows to return.
        /// </summary>
        public long MaxRowCount { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CommandBehavior"/> that should be used when the generated
        /// command is executed.
        /// </summary>
        public virtual CommandBehavior? ExecuteBehavior { get; set; }

        /// <summary>
        /// Specifies a delegate to set up this object for
        /// dealing with a limited row count (off the top)
        /// so that the <see cref="Provider"/> can properly
        /// process the command. 
        /// </summary>
        /// <seealso cref="HandlePaginationForProvider"/>
        public Action<long> HandleLimitedRowCountSelectionForProvider { get; set; }

        private bool IsSqlServer
        {
            get { return Provider.DbFactory is SqlClientFactory; }
        }

        /// <summary>
        /// Specifies a delegate to set up this object for
        /// dealing with pagination so that the <see cref="Provider"/>
        /// can properly process the command.
        /// </summary>
        public Action<QueryPagination<TModel>> HandlePaginationForProvider { get; set; }

        /// <summary>
        /// Returns the columns list (as a delimeted string) for a SELECT statement.
        /// </summary>
        protected virtual string ColumnsListForSelect
        {
            get
            {
                if (!string.IsNullOrEmpty(SelectionItems)) return SelectionItems;
                var sb = new StringBuilder();
                foreach (var columnMapping_kvp in ModelMap.FieldMappings)
                {
                    DataModelColumnAttribute columnMapping = columnMapping_kvp.Value;
                    if (sb.Length > 0)
                    {
                        sb.AppendLine(",");
                        sb.Append("\t");
                    }
                    sb.Append(CommandBuilder.QuotePrefix);
                    sb.Append(columnMapping.ColumnName);
                    sb.Append(CommandBuilder.QuoteSuffix);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Returns the columns list (as a delimtied string) for an INSERT statement.
        /// </summary>
        protected virtual string ColumnsListForInsert
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var columnMapping_kvp in ModelMap.FieldMappings)
                {
                    DataModelColumnAttribute columnMapping = columnMapping_kvp.Value;
                    if (columnMapping.IncludeOnInsert)
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine(",");
                            sb.Append("\t");
                        }
                        sb.Append(CommandBuilder.QuotePrefix);
                        sb.Append(columnMapping.ColumnName);
                        sb.Append(CommandBuilder.QuoteSuffix);
                    }
                }

                foreach (var foreignMapping_kvp in ModelMap.ForeignModelMappings)
                {
                    ForeignDataModelAttribute foreignMapping = foreignMapping_kvp.Value;
                    if (ModelMap.GetFieldMappingByDbColumnName(foreignMapping.LocalColumn) != null) continue;
                    if (sb.Length > 0)
                    {
                        sb.AppendLine(",");
                        sb.Append("\t");
                    }
                    sb.Append(CommandBuilder.QuotePrefix);
                    sb.Append(foreignMapping.LocalColumn);
                    sb.Append(CommandBuilder.QuoteSuffix);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Returns the values param list (as a delimtied string) for an INSERT statement.
        /// </summary>
        protected virtual string ValueParamsListForInsert
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var columnMapping_kvp in ModelMap.FieldMappings)
                {
                    DataModelColumnAttribute columnMapping = columnMapping_kvp.Value;
                    if (columnMapping.IncludeOnInsert)
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine(",");
                            sb.Append("\t");
                        }
                        sb.Append(columnMapping.InsertParam);
                    }
                }
                foreach (var foreignMapping_kvp in ModelMap.ForeignModelMappings)
                {
                    ForeignDataModelAttribute foreignMapping = foreignMapping_kvp.Value;
                    if (ModelMap.GetFieldMappingByDbColumnName(foreignMapping.LocalColumn) != null) continue;
                    if (sb.Length > 0)
                    {
                        sb.AppendLine(",");
                        sb.Append("\t");
                    }
                    sb.Append("@" + foreignMapping.LocalColumn);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Returns the column assignment by param list (as a delimtied string) for an UPDATE statement.
        /// </summary>
        protected virtual string ParamsAssignmentListForUpdate
        {
            get
            {
                var sb = new StringBuilder();
                bool changedOnly = !CommandBuilder.SetAllValues;
                if (changedOnly)
                {
                    foreach (string prop in DataModel.ModifiedProperties)
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine(",");
                            sb.Append("\t");
                        }
                        DataModelColumnAttribute colmap = ModelMap.FieldMappings[prop];
                        sb.Append(CommandBuilder.QuotePrefix);
                        sb.Append(colmap.ColumnName);
                        sb.Append(CommandBuilder.QuoteSuffix);
                        sb.Append(" = ");
                        sb.Append(colmap.UpdateParam);
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// A <see cref="System.Data.Common.DbCommandBuilder"/> that
        /// comes from the <see cref="Provider"/>'s
        /// <see cref="DbDataProvider.DbFactory"/>.<see 
        /// cref="DbProviderFactory.CreateCommandBuilder">CreateCommandBuilder()</see>.
        /// </summary>
        protected DbCommandBuilder CommandBuilder
        {
            get
            {
                if (_CommandBuilder == null)
                {
                    _CommandBuilder = Provider.DbFactory.CreateCommandBuilder();
                }
                return _CommandBuilder;
            }
        }

        /// <summary>
        /// Gets or sets the delegate that handles getting the generated command
        /// to return the resulting ID from an INSERT statement
        /// as an output parameter value.
        /// </summary>
        public Action<string> HandleInsertOutputParamCommand { get; set; }

        /// <summary>
        /// Returns the <see cref="DataModelMap"/> that is associated with the
        /// <see cref="DataModel"/> or <see cref="DataModelQuery"/>.
        /// </summary>
        public DataModelMap ModelMap
        {
            get { return DataModelMap.GetEntityMapping(typeof (TModel)); }
        }

        /// <summary>
        /// Returns true if the command that is returned will be executing
        /// a stored procedure instead of ad hoc text.
        /// </summary>
        protected bool UsesSproc
        {
            get
            {
                if (!Inited) InnerInit();
                switch (StatementCommand)
                {
                    case SqlStatement.SELECT:
                        if (MaxRowCount == 1)
                            return !string.IsNullOrEmpty(ModelMap.TableMapping.SelectProcedure);
                        return !string.IsNullOrEmpty(ModelMap.TableMapping.SelectManyProcedure);
                    case SqlStatement.INSERT:
                        return !string.IsNullOrEmpty(ModelMap.TableMapping.InsertProcedure);
                    case SqlStatement.UPDATE:
                        return !string.IsNullOrEmpty(ModelMap.TableMapping.UpdateProcedure);
                    case SqlStatement.DELETE:
                        return !string.IsNullOrEmpty(ModelMap.TableMapping.DeleteProcedure);
                }
                return false;
            }
        }

        /// <summary>
        /// Returns true if a delegate is assigned to <see cref="HandlePaginationForProvider"/>.
        /// </summary>
        public virtual bool PaginationIsHandled
        {
            get { return HandlePaginationForProvider != null; }
        }

        /// <summary>
        /// Gets or sets explicit selection items for a SELECT statement.
        /// </summary>
        public string SelectionItems { get; set; }

        private void InnerInit()
        {
            CrudOp op = CrudOp.Read;
            DataModel model = DataModel;
            if (model != null)
            {
                if (model.IsNew) op = CrudOp.Create;
                else if (model.IsDirty) op = CrudOp.Update;
                else if (model.MarkDeleted) op = CrudOp.Delete;

                switch (op)
                {
                    case CrudOp.Create:
                        StatementCommand = SqlStatement.INSERT;
                        break;
                    case CrudOp.Read:
                        StatementCommand = SqlStatement.SELECT;
                        break;
                    case CrudOp.Update:
                        StatementCommand = SqlStatement.UPDATE;
                        break;
                    case CrudOp.Delete:
                        StatementCommand = SqlStatement.DELETE;
                        break;
                }
            }
            Init();
            Inited = true;
        }

        /// <summary>
        /// When overridden, pre-loads the inferenced state information for this builder.
        /// </summary>
        protected virtual void Init()
        {
        }

        /// <summary>
        /// Overridable. The base implementation sets the 
        /// <see cref="ExecuteBehavior"/> to <see cref="CommandBehavior.SingleRow"/>
        /// and invokes <see cref="PrepareLimitedRowCountSelection">PrepareLimitedRowCountSelection(1)</see>.
        /// </summary>
        protected virtual void PrepareSingleRowSelection()
        {
            ExecuteBehavior = CommandBehavior.SingleRow;
            PrepareLimitedRowCountSelection(1);
        }

        /// <summary>
        /// Overridable. The base implementation executes
        /// <see cref="HandleLimitedRowCountSelectionForProvider"/> or if that is null then
        /// sets the
        /// <see cref="StatementCommandSuffix"/> to "TOP n" where
        /// "n" is <paramref name="maxRowCount"/>.
        /// </summary>
        /// <param name="maxRowCount"></param>
        protected virtual void PrepareLimitedRowCountSelection(long maxRowCount)
        {
            if (HandleLimitedRowCountSelectionForProvider == null)
            {
                if (IsSqlServer)
                {
                    HandleLimitedRowCountSelectionForProvider
                        = max => { StatementCommandSuffix = "TOP " + max.ToString(); };
                }
            }
            if (HandleLimitedRowCountSelectionForProvider != null)
            {
                HandleLimitedRowCountSelectionForProvider(maxRowCount);
            }
        }

        /// <summary>
        /// Overridable. The base implementation
        /// makes sure that there is a sort order in the query, then
        /// executes <see cref="HandlePaginationForProvider"/>.
        /// </summary>
        /// <remarks>
        /// Inheritor note: If this is overridden,
        /// <see cref="PaginationIsHandled"/> should also
        /// be overridden.
        /// </remarks>
        /// <param name="pagination"></param>
        protected virtual void PreparePagination(QueryPagination<TModel> pagination)
        {
            if (HandlePaginationForProvider == null)
            {
                if (IsSqlServer)
                {
                    // todo: implement a SQL Server implementation *shrug*
                }
            }
            if (HandlePaginationForProvider != null &&
                pagination != null)
            {
                if (Query.OrderBy == null) Query.OrderBy = new List<DataModelQuery<TModel>.SortItem>();
                if (Query.OrderBy.Count == 0)
                {
                    foreach (string col in ModelMap.PrimaryKeyColumns)
                    {
                        Query.AddSortItem(col, true);
                    }
                }
                HandlePaginationForProvider(pagination);
            }
        }

        /// <summary>
        /// Prepares this object for being serialized into command text
        /// that the <see cref="Provider"/> can process.
        /// </summary>
        protected virtual void PrepareForCommandText()
        {
            bool pkAsQuery = false;
            switch (StatementCommand)
            {
                case SqlStatement.SELECT:
                    if (Query.Pagination != null &&
                        (Query.Pagination.Page > 1 ||
                         Query.Pagination.ItemsPerPage < int.MaxValue))
                    {
                        PreparePagination(Query.Pagination);
                    }
                    else if (MaxRowCount < long.MaxValue)
                    {
                        if (MaxRowCount == 1) PrepareSingleRowSelection();
                        else PrepareLimitedRowCountSelection(MaxRowCount);
                    }
                    break;
                case SqlStatement.INSERT:
                    foreach (var p in ModelMap.FieldMappings)
                    {
                        if (p.Value.ReturnAsOutputOnInsert)
                        {
                            PrepareInsertOutputParamCommand(p.Value.InsertParam);
                            break;
                        }
                    }
                    break;
                case SqlStatement.UPDATE:
                    pkAsQuery = Query == null;
                    break;
                case SqlStatement.DELETE:
                    pkAsQuery = Query == null;
                    break;
            }
            if (pkAsQuery)
            {
                Query = new DataModelQuery<TModel>();
                foreach (string pk in ModelMap.PrimaryKeyColumns)
                {
                    Query.WhereColumn[pk].IsEqualTo(DataModel.ColumnMappedValue[pk]);
                }
            }
        }

        /// <summary>
        /// Overridable. In the base implementation, adds 
        /// <code>SET @ID = SCOPE_IDENTITY()</code>
        /// where "@ID" is the <see cref="DataModelColumnAttribute.InsertParam"/>.
        /// </summary>
        /// <param name="param"></param>
        protected virtual void PrepareInsertOutputParamCommand(string param)
        {
            if (HandleInsertOutputParamCommand == null)
            {
                if (IsSqlServer)
                {
                    HandleInsertOutputParamCommand = h =>
                                                         {
                                                             if (!h.StartsWith("@")) h = "@" + h;
                                                             CommandTextSecondaryCommand = "\n\n" + "SET " + h
                                                                                           + " = SCOPE_IDENTITY()";
                                                         };
                }
            }
            if (HandleInsertOutputParamCommand != null) HandleInsertOutputParamCommand(param);
        }

        /// <summary>
        /// Generates the appropriate WHERE clause text, if any, for the command.
        /// </summary>
        /// <returns></returns>
        protected virtual string GenerateWhereClause()
        {
            if (Query == null) return string.Empty;
            if (!string.IsNullOrEmpty(Query.RawExpression)) return Query.RawExpression;
            Type t = typeof (DataModel);
            if (Query.GetType().IsGenericType)
            {
                t = Query.GetType().GetGenericArguments()[0];
            }
            DataModelMap map = DataModelMap.GetEntityMapping(t);
            var sb = new StringBuilder();
            foreach (DataModelQueryCondition<TModel> condition in Query.Conditions)
            {
                if (condition.EvalSubject != null &&
                    condition.FieldMap == null)
                {
                    switch (condition.FindFieldMappingBy)
                    {
                        case FieldMappingKeyType.ClrMember:
                            condition.FieldMap = map.FieldMappings[condition.EvalSubject];
                            break;
                        case FieldMappingKeyType.DbColumn:
                            condition.FieldMap = map.GetFieldMappingByDbColumnName(condition.EvalSubject);
                            break;
                    }
                }
                if (sb.Length > 0) sb.AppendLine("  AND\t");
                sb.Append(CommandBuilder.QuotePrefix);
                if (condition.FieldMap != null)
                {
                    sb.Append(condition.FieldMap.ColumnName);
                }
                else
                {
                    sb.Append(condition.EvalSubject);
                }
                sb.Append(CommandBuilder.QuoteSuffix);
                sb.Append(CompareOpToExpression(condition.CompareOp));
                if (condition.CompareOp != Compare.Null &&
                    condition.CompareOp != Compare.NotNull)
                {
                    if (condition.FieldMap != null)
                    {
                        sb.Append("@");
                        sb.Append(condition.FieldMap.ColumnName);
                    }
                    else
                    {
                        sb.Append("@");
                        sb.Append(condition.EvalSubject);
                    }
                }
            }
            return sb.Length == 0 ? string.Empty : " WHERE\t" + sb.ToString();
        }

        /// <summary>
        /// Generates the ORDER BY clause text, if any, for the command.
        /// </summary>
        /// <returns></returns>
        protected virtual string GenerateOrderByClause()
        {
            List<DataModelQuery<TModel>.SortItem> orderByCollection = Query.OrderBy;
            Type context = typeof (TModel);
            if (orderByCollection == null) return string.Empty;
            var sb = new StringBuilder();
            foreach (DataModelQuery<TModel>.SortItem item in orderByCollection)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(item.GetFieldMapping(context).ColumnName);
                switch (item.SortDirection)
                {
                    case Sort.Ascending:
                        sb.Append(" ASC");
                        break;
                    case Sort.Descending:
                        sb.Append(" DESC");
                        break;
                }
            }
            if (sb.Length > 0) return "ORDER BY " + sb;
            return "";
        }

        /// <summary>
        /// Serializes this object into command text that the
        /// <see cref="Provider"/> can process.
        /// </summary>
        /// <returns></returns>
        public virtual string GenerateCommandText()
        {
            if (!Inited) InnerInit();

            PrepareForCommandText();

            var cmd = new StringBuilder();

            string sprocName = null;

            switch (StatementCommand)
            {
                case SqlStatement.SELECT:
                    sprocName = MaxRowCount == 1 
                        ? ModelMap.TableMapping.SelectProcedure 
                        : ModelMap.TableMapping.SelectManyProcedure;
                    break;
                case SqlStatement.INSERT:
                    sprocName = ModelMap.TableMapping.InsertProcedure;
                    break;
                case SqlStatement.UPDATE:
                    sprocName = ModelMap.TableMapping.UpdateProcedure;
                    break;
                case SqlStatement.DELETE:
                    sprocName = ModelMap.TableMapping.DeleteProcedure;
                    break;
            }
            bool useSproc = !string.IsNullOrEmpty(sprocName);
            if (useSproc) return sprocName;

            if (!string.IsNullOrEmpty(CommandTextPrefix))
                cmd.Append(CommandTextPrefix);

            cmd.Append(StatementCommand.ToString());
            cmd.Append("\t");

            if (!string.IsNullOrEmpty(StatementCommandSuffix))
            {
                cmd.Append(StatementCommandSuffix);
                cmd.Append(" ");
            }

            switch (StatementCommand)
            {
                case SqlStatement.SELECT:
                    cmd.AppendLine(ColumnsListForSelect);
                    if (!string.IsNullOrEmpty(SelectionItemsListSuffix))
                        cmd.AppendLine("\t\t" + SelectionItemsListSuffix);
                    cmd.Append("  FROM\t");
                    cmd.AppendLine(ModelMap.TableMapping.ToString(Provider.DbFactory));
                    break;
                case SqlStatement.INSERT:
                    cmd.AppendLine("INTO " + ModelMap.TableMapping.ToString(Provider.DbFactory));
                    cmd.AppendLine("(");
                    cmd.Append("\t");
                    cmd.AppendLine(ColumnsListForInsert);
                    cmd.AppendLine(") VALUES (");
                    cmd.Append("\t");
                    cmd.AppendLine(ValueParamsListForInsert);
                    cmd.AppendLine(")");
                    break;
                case SqlStatement.UPDATE:
                    cmd.AppendLine(ModelMap.TableMapping.ToString(Provider.DbFactory));
                    cmd.Append("   SET\t");
                    cmd.AppendLine(ParamsAssignmentListForUpdate);
                    break;
                case SqlStatement.DELETE:
                    cmd.Append("  FROM ");
                    cmd.AppendLine(ModelMap.TableMapping.ToString(Provider.DbFactory));
                    break;
            }
            string where = GenerateWhereClause();
            if (!string.IsNullOrEmpty(WhereClauseAdditionalCondition))
            {
                if (string.IsNullOrEmpty(where)) where = " WHERE\t";
                else where += "  AND ";
                where += WhereClauseAdditionalCondition;
            }
            if (!string.IsNullOrEmpty(where)) cmd.AppendLine(where);
            if (StatementCommand == SqlStatement.SELECT)
            {
                string orderBy = GenerateOrderByClause();
                if (!string.IsNullOrEmpty(orderBy))
                    cmd.AppendLine(GenerateOrderByClause());
            }

            if (!string.IsNullOrEmpty(CommandTextSuffix)) cmd.AppendLine(CommandTextSuffix);

            if (!string.IsNullOrEmpty(CommandTextSecondaryCommand))
            {
                cmd.AppendLine(CommandTextSecondaryCommand);
            }

            return cmd.ToString();
        }

        /// <summary>
        /// Returns a generated array of <see cref="DbParameter"/>s
        /// that should be passed in with the command.
        /// </summary>
        /// <returns></returns>
        protected virtual DbParameter[] CreateParameters()
        {
            // add insert/update values
            var ret = new List<DbParameter>();
            foreach (var colmap_kvp in ModelMap.FieldMappings)
            {
                string fld = colmap_kvp.Key;
                DataModelColumnAttribute colmap = colmap_kvp.Value;
                DbParameter param = null;
                switch (StatementCommand)
                {
                    case SqlStatement.INSERT:
                        if (string.IsNullOrEmpty(colmap.InsertParam)) continue;
                        if (colmap.IncludeOnInsert ||
                            colmap.ReturnAsOutputOnInsert)
                        {
                            param = Provider.DbFactory.CreateParameter();
                            param.ParameterName = !colmap.InsertParam.StartsWith("@")
                                                      ? "@" + colmap.InsertParam
                                                      : colmap.InsertParam;
                            if (colmap.ReturnAsOutputOnInsert)
                            {
                                param.Direction = ParameterDirection.Output;
                            }
                        }
                        break;
                    case SqlStatement.UPDATE:
                        if (string.IsNullOrEmpty(colmap.UpdateParam)) continue;
                        bool add = true;
                        if (!CommandBuilder.SetAllValues)
                        {
                            add = DataModel.ModifiedProperties.Contains(fld);
                        }
                        if (add)
                        {
                            param = Provider.DbFactory.CreateParameter();
                            param.ParameterName = !colmap.UpdateParam.StartsWith("@")
                                                      ? "@" + colmap.UpdateParam
                                                      : colmap.UpdateParam;
                        }
                        break;
                }
                if (param != null && ret.Find(p=>p.ParameterName == param.ParameterName) == null)
                {
                    if (param is SqlParameter)
                        ((SqlParameter) param).SqlDbType = colmap.SqlDbType;
                    else param.DbType = colmap.DbType;
                    if (colmap.ColumnSize.HasValue) param.Value = colmap.ColumnSize.Value;
                    else if (colmap.DataType == typeof (string))
                    {
                        param.Size = (DataModel[fld] ?? string.Empty).ToString().Length;
                    }
                    if (param.Direction == ParameterDirection.Input ||
                        param.Direction == ParameterDirection.InputOutput)
                    {
                        param.IsNullable = colmap.IsNullable;
                        param.Value = DataModel[fld] ?? DBNull.Value;
                    }
                    ret.Add(param);
                }
            }

            foreach (var foreignMapping_kvp in ModelMap.ForeignModelMappings)
            {
                ForeignDataModelAttribute foreignMapping = foreignMapping_kvp.Value;
                if (foreignMapping.TargetMemberType.IsOrInherits(typeof(IEnumerable))) continue;
                switch (StatementCommand)
                {
                    case SqlStatement.INSERT:
                        var fieldValue = DataModel.ColumnMappedValue[foreignMapping.LocalColumn];
                        var paramName = "@" + foreignMapping.LocalColumn;
                        if (ret.Find(p => p.ParameterName == paramName) == null)
                        {
                            if (ret.Find(p => p.ParameterName == paramName) != null) continue;
                            var param = Provider.DbFactory.CreateParameter();
                            param.ParameterName = paramName;
                            if (param is SqlParameter)
                                ((SqlParameter) param).SqlDbType = foreignMapping.LocalColumnSqlDbType;
                            else param.DbType = foreignMapping.LocalColumnDbType;
                            if (foreignMapping.LocalColumnSize.HasValue)
                                param.Value = foreignMapping.LocalColumnSize.Value;
                            else if (foreignMapping.TargetMemberType == typeof (string))
                            {
                                param.Size = (fieldValue ?? string.Empty).ToString().Length;
                            }
                            if (param.Direction == ParameterDirection.Input ||
                                param.Direction == ParameterDirection.InputOutput)
                            {
                                param.IsNullable = foreignMapping.LocalColumnIsNullable;
                                param.Value = fieldValue ?? DBNull.Value;
                            }
                            ret.Add(param);
                        }
                        break;
                }
            }

            // add query conditions
            if (Query != null)
            {
                foreach (DataModelQueryCondition<TModel> cond in Query.Conditions)
                {
                    DataModelColumnAttribute fm = cond.FieldMap;
                    if (fm == null)
                    {
                        switch (cond.FindFieldMappingBy)
                        {
                            case FieldMappingKeyType.ClrMember:
                                fm = ModelMap[cond.EvalSubject];
                                break;
                            case FieldMappingKeyType.DbColumn:
                                fm = ModelMap.GetFieldMappingByDbColumnName(cond.EvalSubject);
                                break;
                        }
                    }
                    string paramName = string.Empty;
                    if (fm != null)
                    {
                        switch (StatementCommand)
                        {
                            case SqlStatement.SELECT:
                                if (string.IsNullOrEmpty(fm.SelectParam)) continue;
                                paramName = fm.SelectParam;
                                break;
                            case SqlStatement.INSERT:
                                if (string.IsNullOrEmpty(fm.InsertParam)) continue;
                                paramName = fm.InsertParam;
                                break;
                            case SqlStatement.UPDATE:
                                if (string.IsNullOrEmpty(fm.UpdateParam)) continue;
                                paramName = fm.UpdateParam;
                                break;
                            case SqlStatement.DELETE:
                                if (string.IsNullOrEmpty(fm.DeleteParam)) continue;
                                paramName = fm.DeleteParam;
                                break;
                        }
                        if (ret.Find(p => p.ParameterName == paramName) == null)
                        {
                            if (UsesSproc && cond.CompareOp != Compare.Equal)
                            {
                                throw new InvalidOperationException(
                                    "Cannot produce an ad hoc WHERE clause with a SQL Stored Procedure.");
                            }
                            DbParameter param = Provider.DbFactory.CreateParameter();
                            param.ParameterName = !paramName.StartsWith("@")
                                                      ? "@" + paramName
                                                      : paramName;
                            if (param is SqlParameter)
                                ((SqlParameter) param).SqlDbType = fm.SqlDbType;
                            else param.DbType = fm.DbType;
                            if (fm.ColumnSize.HasValue) param.Value = fm.ColumnSize.Value;
                            else if (fm.DataType == typeof (string))
                            {
                                fm.ColumnSize =
                                    (cond.CompareValue ?? string.Empty).ToString().Length;
                            }
                            param.IsNullable = fm.IsNullable;
                            param.Value = cond.CompareValue ?? DBNull.Value;
                            ret.Add(param);
                        }
                    }
                    else
                    {
                        if (cond.CompareValue != null)
                        {
                            paramName = "@" + cond.EvalSubject;
                            if (ret.Find(p => p.ParameterName == paramName) == null)
                            {
                                DbParameter param = Provider.DbFactory.CreateParameter();
                                param.ParameterName = paramName;
                                if (param is SqlParameter)
                                    ((SqlParameter) param).SqlDbType
                                        = DbTypeConverter.ToSqlDbType(cond.CompareValue);
                                else param.DbType = DbTypeConverter.ToDbType(cond.CompareValue);
                                if (cond.CompareValue is string)
                                    param.Size = ((string) cond.CompareValue).Length;
                                param.IsNullable = true;
                                param.Value = cond.CompareValue;
                                ret.Add(param);
                            }
                        }
                    }
                }
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Returns the generated command text.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GenerateCommandText();
        }

        /// <summary>
        /// Returns a <see cref="DbCommand"/> that is prepared
        /// for executing the Load or Save action.
        /// </summary>
        /// <returns></returns>
        public DbCommand CreateDbCommand(DbTransaction transaction)
        {
            var connection = transaction.Connection;
            DbCommand ret = connection.CreateCommand();
            ret.Transaction = transaction;
            ret.CommandType = UsesSproc ? CommandType.StoredProcedure : CommandType.Text;
            ret.CommandText = GenerateCommandText();
            ret.Parameters.AddRange(CreateParameters());
            return ret;
        }

        /// <summary>
        /// Returns a SQL-formatted binary comparison operator.
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
        protected virtual string CompareOpToExpression(Compare compare)
        {
            switch (compare)
            {
                case Compare.Equal:
                    return " = ";
                case Compare.NotEqual:
                    return " != ";
                case Compare.GreaterThan:
                    return " > ";
                case Compare.GreaterThanOrEqual:
                    return " >= ";
                case Compare.LessThan:
                    return " < ";
                case Compare.LessThanOrEqual:
                    return " <= ";
                case Compare.Like:
                    return " LIKE ";
                case Compare.Null:
                    return " IS NULL";
                case Compare.NotNull:
                    return " IS NOT NULL";
            }
            throw new NotImplementedException("Enum value not handled: " + compare);
        }
    }
}