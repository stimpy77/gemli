using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Gemli.Data
{
    /// <summary>
    /// Describes a foreign key relationship.
    /// </summary>
    [Serializable]
    public class ForeignKeyAttribute : DataModelMemberAttributeBase
    {
        /// <summary>
        /// Replicates the properties from this object
        /// to the specified <paramref name="attribute"/> object.
        /// </summary>
        /// <param name="attribute"></param>
        protected internal override void CopyDeltaTo(DataModelMappingAttributeBase attribute)
        {
            var attr = attribute as ForeignKeyAttribute;
            if (attr != null)
            {
                attr.ForeignEntity = this.ForeignEntity;
                attr.ForeignSchemaName = this.ForeignSchemaName;
                attr.ForeignTableName = this.ForeignTableName;
                attr.ForeignColumn = this.ForeignColumn;
                attr.ForeignEntityProperty = this.ForeignEntityProperty;
                attr.Relationship = this.Relationship;
                attr.AssignToMember = this.AssignToMember;
            }
        }

        private Dictionary<Type, DataModelMap> MapItems
        {
            get { return DataModelMap.MapItems; }
        }

        private Type _RelatesTo;
        /// <summary>
        /// Gets or sets the foreign entity that maps to the equivalent database table.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Type ForeignEntity
        {
            get { 
                if (_RelatesTo == null && _RelatesToTableName != null)
                {
                    foreach (var mi in MapItems)
                    {
                        if (mi.Value.TableMapping.Table == ForeignTableName &&
                            (mi.Value.TableMapping.Schema ?? "") == (ForeignSchemaName ?? ""))
                        {
                            _RelatesTo = mi.Key;
                        }
                    }
                }
                return _RelatesTo; 
            }
            set
            {
                _RelatesTo = value;
            }
        }
        
        /// <summary>
        /// XML serializable ForeignEntity type reference.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [XmlElement("foreignEntityType")]
        public string ForeignEntityType
        {
            get
            {
                if (ForeignEntity == null) return string.Empty;
                return ForeignEntity.FullName;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) ForeignEntity = null;
                ForeignEntity = Type.GetType(value);
            }
        }

        private string _RelatesToSchemaName;
        /// <summary>
        /// Gets or sets the foreign schema name.
        /// </summary>
        [XmlElement("foreignSchemaName")]
        public string ForeignSchemaName 
        {
            get
            {
                // trigger getter on table
                ForeignTableName = ForeignTableName;

                return _RelatesToSchemaName;
            }
            set { _RelatesToSchemaName = value; }
        }

        private string _RelatesToTableName;
        /// <summary>
        /// Gets or sets the foreign table name.
        /// </summary>
        [XmlElement("foreignTableName")]
        public string ForeignTableName
        {
            get
            {
                if (string.IsNullOrEmpty(_RelatesToTableName) && _RelatesTo != null)
                {
                    if (MapItems.ContainsKey(_RelatesTo))
                    {
                        var mi = MapItems[_RelatesTo];
                        ForeignSchemaName = mi.TableMapping.Schema;
                        ForeignTableName = mi.TableMapping.Table;
                    }
                }
                return _RelatesToTableName;
            }
            set { _RelatesToTableName = value; }
        }

        private string _OnMatchDataField;
        /// <summary>
        /// Gets or sets the foreign side database field name for the ON subclause in a SQL join.
        /// i.e. "right_table.customerid" after ON in 
        /// <code>
        /// SELECT left_customerid, right_table.other_info
        ///   FROM customer
        ///  INNER JOIN right_table 
        ///     ON customerid = right_table.customerid
        /// </code>
        /// </summary>
        [XmlElement("foreignColumn")]
        public string ForeignColumn
        {
            get
            {
                if (_OnMatchDataField == null && 
                    _OnMatchProperty != null && 
                    ForeignEntity != null)
                {
                    if (!MapItems.ContainsKey(ForeignEntity))
                        DataModelMap.GetEntityMapping(ForeignEntity);
                    _OnMatchDataField = MapItems[ForeignEntity]
                        .FieldMappings[_OnMatchProperty].ColumnName;
                }
                return _OnMatchDataField;
            }
            set { _OnMatchDataField = value; }
        }

        private string _OnMatchProperty;
        /// <summary>
        /// Gets or sets the foreign entity property/field name for the ON subclause in a SQL join.
        /// </summary>
        /// <seealso cref="ForeignColumn"/>
        [XmlElement("foreignEntityProperty")]
        public string ForeignEntityProperty
        {
            get
            {
                if (_OnMatchProperty == null &&
                    _OnMatchDataField != null &&
                    ForeignEntity != null)
                {
                    if (!MapItems.ContainsKey(ForeignEntity))
                        DataModelMap.GetEntityMapping(ForeignEntity);
                    var map = MapItems[ForeignEntity];
                    foreach (var field_kvp in map.FieldMappings)
                    {
                        var field = field_kvp.Value;
                        if (field.ColumnName.ToLower() == _OnMatchDataField.ToLower())
                        {
                            _OnMatchProperty = field_kvp.Key;
                        }
                    }
                }
                return _OnMatchProperty;
            }
            set { _OnMatchProperty = value; }
        }
        /// <summary>
        /// Describes the <see cref="Gemli.Data.Relationship">Relationship</see>
        /// between the two tables/entities of this foreign key.
        /// </summary>
        [XmlElement("relationship")]
        public Relationship Relationship { get; set; }

        /// <summary>
        /// Describes which CLR property/field should contain
        /// the referenced foreign entity. For example, if this
        /// foreign key is a "customer_id", this value might be 
        /// "Customer" to describe the Customer 
        /// property of the entity that contains this foreign key.
        /// In a one-to-many relationship, the value might instead
        /// be "Customers" to reference the Customers
        /// property.
        /// </summary>
        [XmlElement("assignTo")]
        public string AssignToMember { get; set; }
    }
}
