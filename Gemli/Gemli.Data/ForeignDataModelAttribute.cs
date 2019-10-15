using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Gemli.Reflection;

namespace Gemli.Data
{
    /// <summary>
    /// Describes a foreign relationship to another <see cref="DataModel"/>.
    /// </summary>
    [Serializable]
    [XmlRoot("ForeignDataModel")]
    public class ForeignDataModelAttribute : DataModelMemberAttributeBase
    {
        /// <summary>
        /// Replicates the properties from this object
        /// to the specified <paramref name="attribute"/> object.
        /// </summary>
        /// <param name="attribute"></param>
        protected internal override void CopyDeltaTo(DataModelMappingAttributeBase attribute)
        {
            var obj = (ForeignDataModelAttribute)attribute;
            obj.LocalColumn = this.LocalColumn;
            if (this._LocalColumnDbType.HasValue) obj.LocalColumnDbType = this.LocalColumnDbType;
            if (this._LocalColumnSqlDbType.HasValue) obj.LocalColumnSqlDbType = this.LocalColumnSqlDbType;
            if (this._LocalColumnSize.HasValue) obj.LocalColumnSize = this.LocalColumnSize;
            if (this._LocalColumnIsNullable.HasValue) obj.LocalColumnIsNullable = this.LocalColumnIsNullable;
            obj.RelatedTableColumn = this.RelatedTableColumn;
            obj.RelatedTableSchema = this.RelatedTableSchema;
            obj.RelatedTable = this.RelatedTable;
            obj.MappingTable = this.MappingTable;
            obj.MappingTableSchema = this.MappingTableSchema;
            obj.Relationship = this.Relationship;
        }

        /// <summary>
        /// Constructs the ForeignDataModelAttribute object without
        /// initializing any data.
        /// </summary>
        public ForeignDataModelAttribute()
        {
        }

        /// <summary>
        /// Constructs the ForeignDataModelAttribute object
        /// making an initial assumption that there is a one-to-one
        /// relationship to the specified foreign 
        /// <paramref name="localColumn"/>. 
        /// </summary>
        /// <param name="localColumn"></param>
        public ForeignDataModelAttribute(string localColumn)
        {
            LocalColumn = localColumn;
        }

        /// <summary>
        /// Constructs the ForeignDataModelAttribute object
        /// making an initial assumption that there is a one-to-one
        /// relationship between the specified
        /// <paramref name="localColumn"/> and the specified
        /// <paramref name="relatedTableColumn"/>.
        /// </summary>
        /// <param name="localColumn"></param>
        /// <param name="relatedTableColumn"></param>
        public ForeignDataModelAttribute(string localColumn, string relatedTableColumn)
        {
            LocalColumn = localColumn;
            RelatedTableColumn = relatedTableColumn;
        }

        internal bool IsColumnNameInferred { get; private set; }
        private string _LocalColumn;
        /// <summary>
        /// Gets or sets the local field that references the primary key of the foreign table.
        /// </summary>
        [XmlElement("column")]
        public string LocalColumn
        {
            get {
                if (_LocalColumn == null)
                {
                    if (_relatedColumn != null)
                    {
                        _LocalColumn = _relatedColumn;
                        IsColumnNameInferred = true;
                    }
                    if (TargetMember != null)
                    {
                        var memberMapping = DataModelMap.GetEntityMapping(TargetMember.DeclaringType);
                        if (memberMapping.PrimaryKeyColumns.Length==1)
                        {
                            _LocalColumn = memberMapping.PrimaryKeyColumns[0];
                            IsColumnNameInferred = true;
                        }
                    }
                }
                return _LocalColumn ?? TargetMemberName; 
            }
            set
            {
                _LocalColumn = value;
                IsColumnNameInferred = value == null;
            }
        }

        internal bool IsRelatedColumnNameInferred
        {
            get { return _relatedColumn == null; }
        }

        private string _relatedColumn;

        /// <summary>
        /// Gets or sets the name of the column
        /// that is the owner of the foreign key relationship.
        /// </summary>
        [XmlElement("relatedTableColumn")]
        public string RelatedTableColumn
        {
            get
            {
                if (_relatedColumn == null)
                {
                    if (Relationship != Relationship.ManyToMany &&
                        Relationship != Relationship.ManyToOne)
                    {
                        // infer that the foreign field name is the current table's primary key field
                        var ownerMapping = DataModelMap.GetEntityMapping(TargetMember.DeclaringType);
                        if (ownerMapping.PrimaryKeyColumns.Length == 1)
                        {
                            _relatedColumn = ownerMapping.PrimaryKeyColumns[0];
                            RelatedTableColumnDataType = ownerMapping
                                .GetFieldMappingByDbColumnName(
                                ownerMapping.PrimaryKeyColumns[0]).DbType;
                        }
                    }
                    else
                    {
                        // infer that the foreign field name is the pk mapped to the target type
                        var tmt = TargetMemberType;
                        while (!tmt.IsDataModel() && tmt.IsGenericType)
                        {
                            var genArgs = tmt.GetGenericArguments();
                            tmt = genArgs[genArgs.Length-1];
                        }
                        var targetMapping = DataModelMap.GetEntityMapping(tmt);
                        if (targetMapping.PrimaryKeyColumns.Length == 1)
                        {
                            _relatedColumn = targetMapping.PrimaryKeyColumns[0];
                        }
                        else
                        {
                            throw new AmbiguousMatchException(
                                "There must be one primary key field on " + this.RelatedTable
                                + " for ManyToMany to load by inferred primary key, but "
                                + "in " + TargetMember.DeclaringType.FullName + " too " + 
                                (
                                    targetMapping.PrimaryKeyColumns.Length == 0
                                    ? "few"
                                    : "many"
                                )
                                + " primary key columns were configured "
                                + "for referenced type: " + tmt.FullName);
                        }
                    }
                }
                if (_relatedColumn == null // still
                    && LocalColumn != null)
                {
                    _relatedColumn = LocalColumn;
                }
                return _relatedColumn;
            }
            set
            {
                _relatedColumn = value;
            }
        }

        private string _relatedTable;
        /// <summary>
        /// Gets or sets the name of the table
        /// that is the target of this foreign key relationship.
        /// </summary>
        [XmlElement("relatedTable")]
        public string RelatedTable
        {
            get
            {
                if (_relatedTable == null)
                {
                    var ttype = TargetMemberType;
                    while (!ttype.IsDataModel() &&
                        ttype.IsOrInherits(typeof(IEnumerable)) &&
                        ttype.IsGenericType)
                    {
                        ttype = ttype.GetGenericArguments().Last();
                    }
                    var memberMapping = DataModelMap.GetEntityMapping(ttype);
                    var memberTableMapping = memberMapping.TableMapping;
                    _relatedTable = memberTableMapping.Table;
                    if (_relatedSchema == null) _relatedSchema = memberTableMapping.Schema;
                }
                return _relatedTable;
            }
            set { _relatedTable = value; }
        }

        private string _relatedSchema;
        /// <summary>
        /// Gets or sets the name of the schema of the table
        /// that is the target of this foreign key relationship.
        /// </summary>
        [XmlElement("relatedTableSchema")]
        public string RelatedTableSchema
        {
            get
            {
                if (_relatedSchema == null && _relatedTable == null)
                {
                    var memberMapping = DataModelMap.GetEntityMapping(TargetMember.DeclaringType);
                    var memberTableMapping = memberMapping.TableMapping;
                    _relatedTable = memberTableMapping.Table;
                    _relatedSchema = memberTableMapping.Schema;
                }
                return _relatedSchema;
            }
            set { _relatedSchema = value; }
        }

        /// <summary>
        /// Gets or sets the name of the local table that 
        /// is associated with this foreign key relationship.
        /// </summary>
        [XmlElement("mappingTable")]
        public string MappingTable { get; set; }
        /// <summary>
        /// Gets or sets the name of the local schema of the table
        /// that is associated with this foreign key relationship.
        /// </summary>
        [XmlElement("mappingTableSchema")]
        public string MappingTableSchema { get; set; }

        private Relationship? _Relationship;
        /// <summary>
        /// Describes the JOIN relationship type for
        /// this foreign key relationship (i.e. one-to-one, one-to-many, etc.).
        /// </summary>
        [XmlElement("relationship")]
        public Relationship Relationship
        {
            get
            {
                if (!_Relationship.HasValue)
                {
                    return TargetMemberType.IsOrInherits(typeof (ICollection))
                               ? Relationship.OneToMany
                               : Relationship.OneToOne;
                }
                return _Relationship.Value;
            }
            set { _Relationship = value; }
        }

        /// <summary>
        /// Describes the data type of the foreign key column.
        /// </summary>
        [XmlElement("relatedColumnType")]
        public DbType RelatedTableColumnDataType { get; set; }

        private SqlDbType? _LocalColumnSqlDbType;
        public SqlDbType LocalColumnSqlDbType
        {
            get {
                try
                {
                    if (_LocalColumnSqlDbType.HasValue)
                        return _LocalColumnSqlDbType.Value;
                    var targType = TargetMemberType;
                    if (targType.IsOrInherits(typeof(IEnumerable)) &&
                        targType != typeof(string))
                    {
                        if (targType.IsGenericType)
                        {
                            var genArgs = targType.GetGenericArguments();
                            targType = genArgs[genArgs.Length - 1];
                        }
                        else throw new SqlTypeException("Cannot infer a SqlDbType on an IEnumerable type");
                    }
                    return DataModelMap.GetEntityMapping(targType)
                               .GetFieldMappingByDbColumnName(this.RelatedTableColumn).SqlDbType;
                } catch
                {
                    throw;
                }
            }
            set { _LocalColumnSqlDbType = value; }
        }

        private DbType? _LocalColumnDbType;
        public DbType LocalColumnDbType
        {
            get {
                try
                {
                    if (_LocalColumnSqlDbType.HasValue)
                        return _LocalColumnDbType.Value;
                    var targType = TargetMemberType;
                    if (targType.IsOrInherits(typeof(IEnumerable)) &&
                        targType != typeof(string))
                    {
                        if (targType.IsGenericType)
                        {
                            var genArgs = targType.GetGenericArguments();
                            targType = genArgs[genArgs.Length - 1];
                        }
                        else throw new SqlTypeException("Cannot infer a DbType on an IEnumerable type");
                    }
                    return DataModelMap.GetEntityMapping(targType)
                               .GetFieldMappingByDbColumnName(this.RelatedTableColumn).DbType;
                }
                catch
                {
                    throw;
                }
            }
            set { _LocalColumnDbType = value; }
        }

        private int? _LocalColumnSize;
        public int? LocalColumnSize
        {
            get { return _LocalColumnSize; }
            set { _LocalColumnSize = value; }
        }

        private bool? _LocalColumnIsNullable;
        public bool LocalColumnIsNullable
        {
            get { return _LocalColumnIsNullable ?? true; }
            set { _LocalColumnIsNullable = value; }
        }
    }
}
