using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Gemli.Collections;
using Gemli.Reflection;

namespace Gemli.Data.Providers
{
    /// <summary>
    /// An in-memory <see cref="DataTable"/>-based
    /// database provider for performing basic <see cref="DataModel"/>
    /// testing and for managing queryable in-memory caches of data.
    /// </summary>
    public class MemoryDataProvider : DataProviderBase
    {
        /// <summary>
        /// Constructs a new MemoryDataProvider with an empty
        /// Tables dictionary.
        /// </summary>
        public MemoryDataProvider()
        {
            Tables = new CaseInsensitiveDictionary<DataTable>();
        }

        /// <summary>
        /// Gets or sets a dictionary of string-keyed
        /// <see cref="DataTable"/>s.
        /// </summary>
        public CaseInsensitiveDictionary<DataTable> Tables { get; set; }

        private string QueryToExpression<TModel>(DataModelQuery<TModel> query) where TModel : DataModel
        {
            if (!string.IsNullOrEmpty(query.RawExpression)) return query.RawExpression;
            Type t = typeof (DataModel);
            if (query.GetType().IsGenericType)
            {
                t = query.GetType().GetGenericArguments()[0];
            }
            var map = DataModelMap.GetEntityMapping(t);
            var sb = new StringBuilder();
            foreach (var condition in query.Conditions)
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
                if (condition.FieldMap != null)
                {
                    if (sb.Length > 0) sb.Append(" AND ");
                    sb.Append(condition.FieldMap.ColumnName);
                    sb.Append(CompareOpToExpression(condition.CompareOp));
                    sb.Append(ValueToExpression(condition.FieldMap, condition.CompareValue));
                }
                else
                {
                    if (sb.Length > 0) sb.Append(" AND ");
                    sb.Append(condition.EvalSubject);
                    sb.Append(CompareOpToExpression(condition.CompareOp));
                    if (condition.CompareOp != Compare.Null &&
                        condition.CompareOp != Compare.NotNull)
                    {
                        sb.Append(ValueToExpression(
                            DbTypeConverter.ToDbType(condition.CompareValue.GetType()),
                            condition.CompareValue));
                    }
                }
            }
            return sb.ToString();
        }

        private static string QueryToSortExpression<TModel>(DataModelQuery<TModel> query) where TModel : DataModel
        {
            var sb = new StringBuilder();
            foreach (var item in query.OrderBy)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(item.FieldName);
                sb.Append(" ");
                switch (item.SortDirection)
                {
                    case Sort.Ascending:
                        sb.Append("ASC");
                        break;
                    case Sort.Descending:
                        sb.Append("DESC");
                        break;
                }
            }
            return sb.ToString();
        }

        private string ValueToExpression(DbType dbType, object value)
        {
            if (value == null || value == DBNull.Value) return "NULL";
            var sb = new StringBuilder();
            bool apos = false;
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTimeOffset:
                case DbType.Guid:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Time:
                case DbType.Xml:
                    apos = true;
                    sb.Append("'");
                    break;
            }
            sb.Append(value.ToString().Replace("'", "''"));
            if (apos) sb.Append("'");
            return sb.ToString();
        }

        private string ValueToExpression(DataModelColumnAttribute fieldMetadata, object value)
        {
            return ValueToExpression(fieldMetadata.DbType, value);
        }

        private string CompareOpToExpression(Compare compare)
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

        /// <summary>
        /// Returns a loaded instance of the specified type using the specified
        /// <paramref name="query"/> and <paramref name="transactionContext"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public override T LoadModel<T>(DataModelQuery<T> query, DbTransaction transactionContext)
        {
            if (transactionContext != null) throw new NotImplementedException("MemoryDataProvider does not support DbTransactions");
            var map = DataModel.GetMapping<T>();
            var table = Tables[map.TableMapping.Table];
            var rows = table.Select(QueryToExpression(query));
            if (rows.Length == 0) return null;
            var t = DataModel.GetUnwrappedType(typeof(T));
            T ret = null;
            if (t != typeof(T))
            {
                var subret = Activator.CreateInstance(t);
                var retT = typeof(DataModel<>).MakeGenericType(t);
                ret = (T)Activator.CreateInstance(retT, new[] { subret });
                ret.Load(rows[0]);
                ret.DataProvider = this;
            }
            if (ret == null)
            {
                ret = (T)Activator.CreateInstance(typeof(T));
                ret.Load(rows[0]);
                ret.DataProvider = this;
            }
            return ret;
        }

        /// <summary>
        /// Returns a collection of loaded instances of the specified type using the specified
        /// <paramref name="query"/> and <paramref name="transactionContext"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public override DataModelCollection<T> LoadModels<T>(DataModelQuery<T> query, DbTransaction transactionContext)
        {
            if (transactionContext != null) throw new NotImplementedException("MemoryDataProvider does not support DbTransactions");
            var map = DataModel.GetMapping<T>();
            var table = Tables[map.TableMapping.Table];
            var rows = table.Select(QueryToExpression(query), QueryToSortExpression(query));
            var retcol = new DataModelCollection<T>();
            if (rows.Length == 0) return retcol;
            var t = DataModel.GetUnwrappedType(typeof(T));
            foreach (DataRow row in rows)
            {
                T ret;
                while (t.IsGenericType && t.IsOrInherits(typeof(IEnumerable)))
                    t = t.GetGenericArguments().Last();
                if (t != typeof (T))
                {
                    var subret = Activator.CreateInstance(t);
                    var retT = typeof (DataModel<>).MakeGenericType(t);
                    ret = (T) Activator.CreateInstance(retT, new[] {subret});
                    ret.Load(row);
                    ret.DataProvider = this;
                }
                else
                {
                    ret = (T) Activator.CreateInstance(typeof (T));
                    ret.Load(row);
                    ret.DataProvider = this;
                }
                retcol.Add(ret);
            }
            retcol.DataProvider = this;
            return retcol;
        }
        
        /// <summary>
        /// Creates a new <see cref="DataTable"/> with 
        /// the specified name and adds it to the list 
        /// of <see cref="Tables"/>.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable AddTable(string tableName)
        {
            if (Tables.ContainsKey(tableName))
                throw new ArgumentException("Table already exists.", "tableName");
            return AddTable(new DataTable(tableName));
        }

        /// <summary>
        /// Adds the specified <paramref name="table"/> 
        /// to the list of <see cref="Tables"/>.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public DataTable AddTable(DataTable table)
        {
            if (Tables.ContainsKey(table.TableName))
                throw new ArgumentException("A table with that name already exists.", "table");
            Tables[table.TableName] = table;
            return table;
        }

        /// <summary>
        /// Applies the appropriate changes to the database with respect to
        /// any in-memory <see cref="DataTable"/> record associated with
        /// the specified <paramref name="dataModel"/>.
        /// The <paramref name="transactionContext"/> parameter is not supported
        /// and must be null.
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="transactionContext"></param>
        public override void SaveModel<TModel>(TModel dataModel, DbTransaction transactionContext)
        {
            if (transactionContext != null) throw new NotImplementedException("MemoryDataProvider does not support DbTransactions");
            var tableName = dataModel.EntityMappings.TableMapping.Table;
            var table = Tables[tableName];
            if (dataModel.IsNew && !dataModel.MarkDeleted)
            {
                var row = table.NewRow();
                foreach (var field_kvp in dataModel.EntityMappings.FieldMappings)
                {
                    var field = field_kvp.Value;
                    if (!field.IsIdentity)
                    {
                        row[field.ColumnName] = dataModel.ColumnMappedValue[field.ColumnName];
                    }
                }
                table.Rows.Add(row);
                row.AcceptChanges();
            }
            else if (dataModel.MarkDeleted)
            {
                var identExpr = GetRowIdentifierExpression(dataModel);
                var rows = table.Select(identExpr);
                foreach (var row in rows)
                {
                    table.Rows.Remove(row);
                }
            }
            else if (dataModel.IsDirty)
            {
                var identExpr = GetRowIdentifierExpression(dataModel);
                var rows = table.Select(identExpr);
                foreach (var row in rows)
                {
                    row.BeginEdit();
                    foreach (var field_kvp in dataModel.EntityMappings.FieldMappings)
                    {
                        var field = field_kvp.Value;
                        row[field.ColumnName] = dataModel.ColumnMappedValue[field.ColumnName];
                    }
                    row.EndEdit();
                    row.AcceptChanges();
                }
            }
            table.AcceptChanges();
        }

        /// <summary>
        /// Applies the appropriate changes to the database with respect to
        /// any in-memory <see cref="DataTable"/> records associated with
        /// the specified <paramref name="dataEntities"/>.
        /// The <paramref name="transactionContext"/> parameter is not supported
        /// and must be null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataEntities"></param>
        /// <param name="transactionContext"></param>
        public override void SaveModels<T>(
            DataModelCollection<T> dataEntities, 
            DbTransaction transactionContext)
        {
            if (transactionContext != null) throw new NotImplementedException("MemoryDataProvider does not support DbTransactions");
            foreach (var dataModel in dataEntities)
            {
                SaveModel(dataModel, transactionContext);
            }
        }

        /// <summary>
        /// Returns false, indicating that this data provider
        /// does not support database transactions.
        /// </summary>
        public override bool SupportsTransactions
        {
            get { return false; }
        }

        /// <summary>
        /// Not implemented nor supported.
        /// </summary>
        /// <returns></returns>
        public override DbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented nor supported.
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public override DbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
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
            return (long)Tables[DataModelMap.GetEntityMapping(typeof (TModel)).TableMapping.ToString()].Rows.Count;
        }

        private string GetRowIdentifierExpression(DataModel dataModel)
        {
            var sb = new StringBuilder();
            foreach (var field_kvp in dataModel.EntityMappings.FieldMappings)
            {
                var field = field_kvp.Value;
                if (field.IsPrimaryKey)
                {
                    if (sb.Length > 0) sb.Append(" AND ");
                    sb.Append(field.ColumnName);
                    sb.Append(CompareOpToExpression(Compare.Equal));
                    sb.Append(ValueToExpression(field,
                        dataModel[field_kvp.Key]));
                }
            }
            if (sb.Length == 0)
            {   // match where all values match
                foreach (var field_kvp in dataModel.EntityMappings.FieldMappings)
                {
                    var field = field_kvp.Value;
                    if (sb.Length > 0) sb.Append(" AND ");
                    sb.Append(field.ColumnName);
                    sb.Append(CompareOpToExpression(Compare.Equal));

                    if (dataModel.ModifiedProperties.Contains(field_kvp.Key))
                    {
                        sb.Append(ValueToExpression(field,
                            dataModel.OriginalData[field_kvp.Key]));
                    }
                    else
                    {
                        sb.Append(ValueToExpression(field,
                            dataModel[field_kvp.Key]));
                    }
                }
            }
            return sb.ToString();
        }
    }
}
