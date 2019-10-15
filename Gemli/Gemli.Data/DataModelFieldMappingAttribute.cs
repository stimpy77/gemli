using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using Gemli.Reflection;

namespace Gemli.Data
{
    /// <summary>
    /// Assigns basic field mapping between a database table field
    /// and a CLR member (property/field).
    /// </summary>
    [Serializable]
    [XmlRoot("FieldMapping")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DataModelFieldMappingAttribute : DataModelMemberAttributeBase
    {
        /// <summary>
        /// Constructs the mapping attribute with the specified 
        /// <paramref name="columnName"/> as the database column
        /// name that the associated CLR member is mapped to.
        /// </summary>
        /// <param name="columnName"></param>
        public DataModelFieldMappingAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        /// <summary>
        /// Constructs the mapping attribute with no initial
        /// mapping information.
        /// </summary>
        public DataModelFieldMappingAttribute()
        {
            this.DataType = base.DeclaringType;
        }

        /// <summary>
        /// Replicates the properties from this object
        /// to the specified <paramref name="attribute"/> object.
        /// </summary>
        /// <param name="attribute"></param>
        protected internal override void CopyDeltaTo(DataModelMappingAttributeBase attribute)
        {
            var attr = attribute as DataModelFieldMappingAttribute;
            if (attr != null)
            {
                attr.ClearBaseObjectMapping = this.ClearBaseObjectMapping;
                attr._DbType = this._DbType;
                attr._SqlDbType = this._SqlDbType;
                attr._DataType = this._DataType;
                attr._DefaultValue = this._DefaultValue;
                attr.ColumnName = this.ColumnName;
                attr.ForeignKeyMapping = this.ForeignKeyMapping;
                attr._IsIdentity = this._IsIdentity;
                attr._IsPrimaryKey = this._IsPrimaryKey;
                attr.TargetMember = this.TargetMember;
                attr.TargetMemberType = this.TargetMemberType;
            }
        }

        private bool _isColumnNameInferred;
        internal bool IsColumnNameInferred
        {
            get { return _ColumnName == null; }
            set { _isColumnNameInferred = value; }
        }
        private string _ColumnName;
        /// <summary>
        /// Gets or sets the database column name that the
        /// associated CLR member is mapped to.
        /// </summary>
        [XmlElement("ColumnName")]
        public string ColumnName
        {
            get 
            { 
                if (_ColumnName == null) return TargetMember.Name;
                return _ColumnName ?? "";
            }
            set
            {
                _ColumnName = value;
                _isColumnNameInferred = false;
            }
        }

        private DbType? _DbType;
        private SqlDbType? _SqlDbType;
        private Type _DataType;

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DbType"/> for this mapping.
        /// Setting this value will automatically update the 
        /// <see cref="SqlDbType"/> and <see cref="DataType"/> properties
        /// with translated values.
        /// </summary>
        public DbType DbType
        {
            get { return _DbType ?? DbType.Object; }
            set
            {
                _DbType = value;
                try
                {
                    _SqlDbType = DbTypeConverter.ToSqlDbType(value);
                }
                catch (ArgumentException)
                {
                    _SqlDbType = null;
                }
                try
                {
                    _DataType = DbTypeConverter.ToClrType(value);
                }
                catch (ArgumentException)
                {
                    _DataType = null;
                }
                
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Type"/> for this mapping.
        /// Setting this value will automatically update the 
        /// <see cref="SqlDbType"/> and <see cref="DbType"/> properties
        /// with translated values.
        /// </summary>
        [XmlIgnore]
        public Type DataType
        {
            get { return _DataType; }
            set
            {
                _DataType = value ?? typeof(object);
                try
                {
                    _DbType = DbTypeConverter.ToDbType(value);
                }
                catch (ArgumentException)
                {
                    _DbType = null;
                }
                try
                {
                    _SqlDbType = DbTypeConverter.ToSqlDbType(value);
                }
                catch (ArgumentException)
                {
                    _SqlDbType = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.SqlDbType"/> for this mapping. 
        /// Setting this value will automatically update the 
        /// <see cref="SqlDbType"/> and <see cref="DataType"/> properties
        /// with translated values.
        /// </summary>
        public SqlDbType SqlDbType
        {
            get { return _SqlDbType ?? System.Data.SqlDbType.Variant; }
            set
            {
                _SqlDbType = value;
                try
                {
                    _DbType = DbTypeConverter.ToDbType(value);
                }
                catch (ArgumentException)
                {
                    _DbType = null;
                }
                try
                {
                    _DataType = DbTypeConverter.ToClrType(value);
                } catch (ArgumentException)
                {
                    _DataType = null;
                }
            }
        }

        private bool? _IsNullable;
        /// <summary>
        /// Returns whether or not the data can be set to NULL.
        /// </summary>
        public bool IsNullable
        {
            get { return _IsNullable ?? !(IsPrimaryKey); }
            set { _IsNullable = value; }
        }

        internal bool IsNullableDefined { get { return _IsNullable != null; }}

        private bool? _IsPrimaryKey;
        /// <summary>
        /// Determines whether this field should be treated
        /// as one of the primary key fields.
        /// </summary>
        public bool IsPrimaryKey
        {
            get { return _IsPrimaryKey ?? IsIdentity; }
            set { _IsPrimaryKey = value; }
        }

        private bool? _IsIdentity;
        /// <summary>
        /// Determines whether this field should be treated
        /// as an identity column.
        /// </summary>
        public bool IsIdentity
        {
            get { return _IsIdentity ?? false; }
            set { _IsIdentity = value; }
        }

        internal bool IsIdentityDefined { get { return _IsIdentity != null; }}

        internal bool IsPrimaryKeyDefined { get { return _IsPrimaryKey != null; }}

        private object _DefaultValue;

        /// <summary>
        /// Gets or sets the default value for the data field.
        /// </summary>
        public object DefaultValue
        {
            get
            {
                if (_DefaultValue != null) return _DefaultValue;
                if (IsNullable) return null;
                var t = TargetMemberType;
                if (t == null || !t.IsValueType)
                {
                    return null;
                }
                var ret = Activator.CreateInstance(t);
                if (ret == null && !IsNullable && t.IsNullableWrappedValueType())
                {
                    var gp = t.GetGenericArguments()[0];
                    ret = Activator.CreateInstance(gp);
                }
                return ret;
            }
            set { _DefaultValue = value; }
        }

        /// <summary>
        /// Returns true if a <see cref="ForeignKeyMapping"/> has been assigned
        /// to the associated CLR member with this mapping.
        /// </summary>
        public bool IsForeignKey { get { return ForeignKeyMapping != null; } }

        /// <summary>
        /// Gets or sets a <see cref="DataModelForeignKeyAttribute"/> that
        /// is associated with the CLR member that this mapping is also
        /// associated with.
        /// </summary>
        public DataModelForeignKeyAttribute ForeignKeyMapping { get; set; }

        private bool? _includeOnInsert;
        /// <summary>
        /// If set to false, this field is excluded from INSERTs
        /// to the database. If the entity mapping references a
        /// stored procedure, this field is excluded from the
        /// parameters list. 
        /// This property defaults to the opposite value
        /// of the property <see cref="IsIdentity"/>.
        /// Note that setting this to 
        /// true forces the ReturnOnInsert
        /// value to be false.
        /// </summary>
        public bool IncludeOnInsert
        {
            get
            {
                if (_includeOnInsert == null) return !IsIdentity;
                return _includeOnInsert.Value;
            }
            set
            {
                _includeOnInsert = value;
                if (value)
                {
                    if (_returnOnInsert == null || !_returnOnInsert.Value)
                        _returnOnInsert = false;
                    else
                        throw new InvalidOperationException(
                            "Cannot IncludeOnInsert and ReturnOnInsert at the same time.");
                }
            }
        }

        private bool? _returnOnInsert;
        /// <summary>
        /// If set to true, this field is included in INSERTs
        /// to the database as a return value. 
        /// This property defaults to the value of the property 
        /// <see cref="IsIdentity"/>.
        /// Note that setting this to 
        /// true forces the IncludeOnInsert
        /// value to be false.
        /// </summary>
        public bool ReturnOnInsert
        {
            get
            {
                if (_returnOnInsert == null) return IsIdentity;
                return _returnOnInsert.Value;
            }
            set
            {
                _returnOnInsert = value;
                if (value)
                {
                    if (_includeOnInsert == null || !_includeOnInsert.Value)
                        _includeOnInsert = false;
                    else
                        throw new InvalidOperationException(
                            "Cannot IncludeOnInsert and ReturnOnInsert at the same time.");
                }
            }
        }

        private string _selectParam;
        /// <summary>
        /// This property is only used when the entity mapping
        /// references a stored procedure for a SELECT. It 
        /// identifies what the parameter name will be for
        /// this field when invoking that procedure.
        /// </summary>
        public string SelectParam
        {
            get
            {
                return _selectParam ?? ("@" + ColumnName);
            }
            set { _selectParam = value;}
        }

        private string _selectManyParam;
        /// <summary>
        /// This property is only used when the entity mapping
        /// references a stored procedure for a SELECT. It 
        /// identifies what the parameter name will be for
        /// this field when invoking that procedure.
        /// </summary>
        public string SelectManyParam
        {
            get
            {
                return _selectManyParam ?? ("@" + ColumnName);
            }
            set { _selectManyParam = value; }
        }

        private string _insertParam;
        /// <summary>
        /// This property is only used when the entity mapping
        /// references a stored procedure for a INSERT. It 
        /// identifies what the parameter name will be for
        /// this field when invoking that procedure.
        /// </summary>
        public string InsertParam
        {
            get
            {
                return _insertParam ?? ("@" + ColumnName);
            }
            set { _insertParam = value; }
        }

        private string _updateParam;
        /// <summary>
        /// This property is only used when the entity mapping
        /// references a stored procedure for a UPDATE. It 
        /// identifies what the parameter name will be for
        /// this field when invoking that procedure.
        /// </summary>
        public string UpdateParam
        {
            get
            {
                return _updateParam ?? ("@" + ColumnName);
            }
            set { _updateParam = value; }
        }

        private string _deleteParam;
        /// <summary>
        /// This property is only used when the entity mapping
        /// references a stored procedure for a DELETE. It 
        /// identifies what the parameter name will be for
        /// this field when invoking that procedure.
        /// </summary>
        public string DeleteParam
        {
            get
            {
                return _deleteParam ?? ("@" + ColumnName);
            }
            set { _deleteParam = value; }
        }

        ///<summary>
        /// Returns true if the <see cref="DefaultValue"/> has not been defined.
        ///</summary>
        public bool DefaultValueDefined
        {
            get { return _DefaultValue == null; }
        }
    }
}
