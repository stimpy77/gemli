using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [XmlRoot("columnMapping")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DataModelColumnAttribute : DataModelMemberAttributeBase
    {
        /// <summary>
        /// Constructs the mapping attribute with the specified 
        /// <paramref name="columnName"/> as the database column
        /// name that the associated CLR member is mapped to.
        /// </summary>
        /// <param name="columnName"></param>
        public DataModelColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        /// <summary>
        /// Constructs the mapping attribute with no initial
        /// mapping information.
        /// </summary>
        public DataModelColumnAttribute()
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
            var attr = attribute as DataModelColumnAttribute;
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
        [XmlElement("columnName")]
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
        [XmlElement("dbType")]
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
        [XmlElement("sqlType")]
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
        [XmlElement("isNullable")]
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
        [XmlElement("isPrimaryKey")]
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
        [XmlElement("isIdentity")]
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
        [XmlIgnore]
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
        /// See <see cref="DefaultValue"/>.
        /// This is a serialization wrapper.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("defaultValue")]
        public string DefaultValueSerialized
        {
            get
            {
                return (DefaultValue ?? string.Empty).ToString();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (DataType == typeof(Guid))
                    {
                        DefaultValue = new Guid(value);
                    }
                    // else if (DataType == [another type not handled by Convert.ChangeType()]) .. add as needed?
                    else
                    {
                        DefaultValue = Convert.ChangeType(value, DataType);
                    }
                }
                else DefaultValue = null;
            }
        }

        /// <summary>
        /// Returns true if a <see cref="ForeignKeyMapping"/> has been assigned
        /// to the associated CLR member with this mapping, or if this
        /// is associated with a <see cref="ForeignDataModelAttribute"/>.
        /// </summary>
        [XmlElement("isForeignKey")]
        public bool IsForeignKey { get { return ForeignKeyMapping != null; } }

        /// <summary>
        /// Gets or sets a <see cref="ForeignKeyAttribute"/> that
        /// is associated with the CLR member that this mapping is also
        /// associated with.
        /// </summary>
        [XmlElement("foreignKeyMapping")]
        public ForeignKeyAttribute ForeignKeyMapping { get; set; }

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
        [XmlElement("includeOnInsert")]
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
        [XmlElement("outputOnInsert")]
        public bool ReturnAsOutputOnInsert
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
        [XmlElement("selectParam")]
        public string SelectParam
        {
            get
            {
                return _selectParam ?? ("@" + ColumnName.Replace(" ", "_").Replace("'", "_"));
            }
            set { _selectParam = value;}
        }

        private string _selectManyParam;
        /// <summary>
        /// This property  
        /// identifies what the parameter name will be for
        /// this field when invoking that procedure or statement.
        /// </summary>
        [XmlElement("selectManyParam")]
        public string SelectManyParam
        {
            get
            {
                return _selectManyParam ?? ("@" + ColumnName.Replace(" ", "_").Replace("'", "_"));
            }
            set { _selectManyParam = value; }
        }

        private string _insertParam;
        /// <summary>
        /// This property 
        /// identifies what the parameter name will be for
        /// this field when invoking that procedure or statement.
        /// </summary>
        [XmlElement("insertParam")]
        public string InsertParam
        {
            get
            {
                return _insertParam ?? ("@" + ColumnName.Replace(" ", "_").Replace("'", "_"));
            }
            set { _insertParam = value; }
        }

        private string _updateParam;
        /// <summary>
        /// This property  
        /// identifies what the parameter name will be for
        /// this field when invoking that procedure or statement.
        /// </summary>
        [XmlElement("updateParam")]
        public string UpdateParam
        {
            get
            {
                return _updateParam ?? ("@" + ColumnName.Replace(" ", "_").Replace("'", "_"));
            }
            set { _updateParam = value; }
        }

        private string _deleteParam;
        /// <summary>
        /// This property  
        /// identifies what the parameter name will be for
        /// this field when invoking that procedure or statement.
        /// </summary>
        [XmlElement("deleteParam")]
        public string DeleteParam
        {
            get
            {
                return _deleteParam ?? ("@" + ColumnName.Replace(" ", "_").Replace("'", "_"));
            }
            set { _deleteParam = value; }
        }

        ///<summary>
        /// Returns true if the <see cref="DefaultValue"/> has not been defined.
        ///</summary>
        [XmlIgnore]
        public bool DefaultValueDefined
        {
            get { return _DefaultValue == null; }
        }

        /// <summary>
        /// Gets or sets the size of the column.
        /// For example, the '25' in varchar(25).
        /// This is the same value that will populate
        /// DbParameter instances' Size property.
        /// </summary>
        public int? ColumnSize { get; set; }
    }
}
