using System;
using System.Data;

namespace Gemli.Data
{
    public partial class DataModel
    {
        /// <summary>
        /// Utility class used for converting data to/from CLR (reflection) and database (ADO.NET) types.
        /// </summary>
        public class DataModelConverter
        {
            internal DataModelConverter(DataModel dataModel)
            {
                DataModelContext = dataModel;
            }

            private readonly DataModel DataModelContext;

            internal void FromDataReader(IDataReader dataReader)
            {
                var dt = dataReader.GetSchemaTable();
                FromDataReader(dataReader, dt);
            }

            internal void FromDataReader(IDataReader dataReader, DataTable schema)
            {
                FromDataReader(dataReader, schema, true, true);
            }

            internal void FromDataReader(IDataReader dataReader, DataTable schema, bool withReset, bool throwOnFieldMismatch)
            {
                var bLoading = DataModelContext.Loading;
                if (withReset) DataModelContext.Reset(ResetMode.ClearAndNew);
                DataModelContext.Loading = bLoading;
                foreach (DataRow row in schema.Rows)
                {
                    DataModelContext.ColumnMappedValue[row["ColumnName"].ToString()] 
                        = dataReader[row["ColumnName"].ToString()];
                }
            }

            internal void FromDataRow(DataRow dr)
            {
                FromDataRow(dr, true, true);
            }

            internal void FromDataRow(DataRow dr, bool withReset, bool throwOnFieldMismatch)
            {
                var dt = dr.Table;
                var bLoading = DataModelContext.Loading;
                if (withReset) DataModelContext.Reset(ResetMode.ClearAndNew);
                DataModelContext.Loading = bLoading;
                foreach (DataColumn column in dt.Columns)
                {
                    DataModelContext.ColumnMappedValue[column.ColumnName] = dr[column];
                }
            }

            /// <summary>
            /// Creates a <see cref="DataTable"/> object
            /// with columns matching the field mappings
            /// for this object. This is just a schema
            /// export.
            /// </summary>
            /// <returns></returns>
            public DataTable ToDataTable()
            {
                string tableName = DataModelContext.EntityMappings.TableMapping.Table;
                if (!string.IsNullOrEmpty(DataModelContext.EntityMappings.TableMapping.Schema) &&
                    DataModelContext.EntityMappings.TableMapping.Schema != "dbo")
                {
                    string schema = DataModelContext.EntityMappings.TableMapping.Schema;
                    if (!schema.EndsWith(".")) schema += ".";
                    tableName = schema + tableName;
                }
                var dt = new DataTable(tableName);
                foreach (var field_kvp in DataModelContext.EntityMappings.FieldMappings)
                {
                    DataModelColumnAttribute field = field_kvp.Value;
                    DataColumn dc;
                    try
                    {
                        dc = new DataColumn(field.ColumnName, field.TargetMemberType);
                    }
                    catch (NotSupportedException)
                    {
                        dc = new DataColumn(field.ColumnName);
                    }
                    dt.Columns.Add(dc);
                }
                return dt;
            }

            /// <summary>
            /// Converts the data stored by this object to
            /// a <see cref="DataRow"/>.
            /// </summary>
            /// <returns></returns>
            public DataRow ToDataRow()
            {
                return ToDataRow(ToDataTable(), true);
            }

            /// <summary>
            /// Converts the data stored by this object to
            /// a <see cref="DataRow"/>.
            /// </summary>
            /// <param name="dt">A schema-matching data table</param>
            /// <param name="throwOnFieldMismatch">Determines whether
            /// to throw a <see cref="FieldAccessException"/> if the
            /// <see cref="DataTable"/> parameter <paramref name="dt"/>
            /// contains a field that does not match any field mappings.</param>
            /// <returns></returns>
            public DataRow ToDataRow(DataTable dt, bool throwOnFieldMismatch)
            {
                DataModelContext.SynchronizeFields(SyncTo.FieldMappedData);
                dt.BeginLoadData();
                var objarr = new object[dt.Columns.Count];
                for (int i = 0; i < objarr.Length; i++)
                {
                    var col = dt.Columns[i];
                    object val = DataModelContext.ColumnMappedValue[col.ColumnName];
                    objarr[i] = val;
                }
                dt.Rows.Add(objarr);
                dt.EndLoadData();
                return dt.Rows[0];
            }
        }
    }
}
