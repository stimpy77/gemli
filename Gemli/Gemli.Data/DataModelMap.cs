using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Gemli.Collections;
using System.Reflection;
using System.Data;
using Gemli.Reflection;
using System.Xml.Serialization;
using Gemli.Serialization;

namespace Gemli.Data
{
    /// <summary>
    /// Describes the CLR-to-database mapping metadata
    /// associated with a particular CLR type. This
    /// has a scope of CLR object to database table.
    /// </summary>
    [Serializable]
    [XmlType("dataModel")]
    public class DataModelMap
    {
        static DataModelMap()
        {
            DefaultBehaviors = new DataModelMapDefaultBehaviors();
        }

        /// <summary>
        /// Creates an empty DataModelMap without any initialization.
        /// </summary>
        public DataModelMap() {}

        /// <summary>
        /// Constructs the mapping metadata using the specified type
        /// by introspecting its attributes.
        /// </summary>
        /// <seealso cref="DataModelTableAttribute"/>
        /// <seealso cref="DataModelColumnAttribute"/>
        /// <seealso cref="ForeignKeyAttribute"/>
        /// <param name="type"></param>
        public DataModelMap(Type type)
        {
            if (type.IsDataModelWrapper())
            {
                type = type.GetDataModelWrapperGenericTypeArg();
            }
            if (type.IsOrInherits(typeof(IList)) && type.IsGenericType)
            {
                type = type.GetGenericArguments().Last();
            }

            EntityType = type;
            
            FieldMappings = new CaseInsensitiveDictionary<DataModelColumnAttribute>();
            ForeignModelMappings = new CaseInsensitiveDictionary<ForeignDataModelAttribute>();
            
            TableMapping = GetTableMapByAttributes(type);
            LoadFieldMappingAttributes();
            LoadForeignKeyMappingAttributes();
            LoadForeignDataModelMappingAttributes();
            //todo: LoadMappingXml();
        }

        /// <summary>
        /// Statically returns a <see cref="DataModelMapDefaultBehaviors"/>
        /// object that defines the default behavior for populating a
        /// <see cref="DataModelMap"/>.
        /// </summary>
        public static DataModelMapDefaultBehaviors DefaultBehaviors { get; set; }

        private Type GetEntityType(Type type)
        {
            if (type.IsDataModelWrapper())
            {
                type = type.GetDataModelWrapperGenericTypeArg();
            }
            if (type.IsOrInherits(typeof(IList)) && type.IsGenericType)
            {
                type = type.GetGenericArguments().Last();
            }
            return type;
        }

        private bool _FieldMapBehaviorsExecuted;

        private void ExecuteFieldMapBehaviors()
        {
            // Auto-infer IsIdentity if no field is declared as primary key 
            // and no field is declared as identity and one member is named
            // "ID" and that member is an int or a long. 
            bool hasPkOrIdent = false;
            DataModelColumnAttribute idFieldMapping = null;
            string typename = null;
            foreach (var field_kvp in FieldMappings)
            {
                var field = field_kvp.Value;
                if (!field.IsForeignKey)
                {
                    if ((field.IsIdentity && field.IsIdentityDefined) || 
                        (field.IsPrimaryKey && field.IsPrimaryKeyDefined))
                    {
                        hasPkOrIdent = true;
                    }
                    if (typename == null) typename = field.TargetMember.DeclaringType.Name.ToLower();
                    var propname = field.TargetMember.Name.ToLower();
                    if (propname == "id" || propname == typename + "id" || propname == typename + "_id")
                    {
                        idFieldMapping = field;
                    }
                }
            }
            if (!hasPkOrIdent && idFieldMapping != null && 
                (idFieldMapping.DbType == DbType.Int32 ||
                idFieldMapping.DbType == DbType.Int64))
            {
                idFieldMapping.IsIdentity = true;
            } 

            // Auto-infer a dictionary entry placeholder for foreign entity mappings
            // with no referenced foreign key column
            foreach (var fk in ForeignModelMappings)
            {
                //if (fk.Value.TargetMemberType.IsOrInherits(typeof(IEnumerable))) continue;
                var tmt = GetEntityType(fk.Value.TargetMemberType);
                var ttmap = GetEntityMapping(tmt);
                var ff = ttmap != null
                             ? ttmap.GetFieldMappingByDbColumnName(fk.Value.RelatedTableColumn)
                             : null;
                if (FieldMappings.ToList().Exists(p => p
                    .Value.ColumnName.ToLower() == fk.Value.LocalColumn.ToLower()))
                {
                    continue;
                }
                var fm = new DataModelColumnAttribute();
                if (ff != null)
                {
                    ff.CopyDeltaTo(fm);
                    fm.ColumnName = fk.Value.LocalColumn;
                    fm.DbType = fk.Value.LocalColumnDbType;
                    fm.SqlDbType = fk.Value.LocalColumnSqlDbType;
                    fm.ColumnSize = fk.Value.LocalColumnSize;
                    fm.IsNullable = fk.Value.LocalColumnIsNullable;
                    fm.TargetMember = fk.Value.TargetMember;
                    fm.TargetMemberType = fk.Value.TargetMemberType;
                    fm.IsPrimaryKey = false;
                    fm.IsIdentity = false;
                    FieldMappings.Add("field:" + fk.Value.LocalColumn, fm);
                }
                _FieldMapBehaviorsExecuted = true;
            }

            // predetermine NULLables based on CLR nullability
            foreach (var field_kvp in FieldMappings)
            {
                var field = field_kvp.Value;
                if (!field.IsNullableDefined)
                {
                    var memtype = field.TargetMemberType;
                    bool nullable = !memtype.IsValueType;
                    if (memtype.IsGenericType && memtype.GetGenericTypeDefinition() == typeof(Nullable<>)) 
                    {
                        memtype = memtype.GetGenericArguments()[0];
                        nullable = true;
                    }
                    field.IsNullable = (memtype.IsValueType || memtype == typeof(string)) && nullable && !field.IsPrimaryKey;
                }
            }
        }

        private string[] _PrimaryKeyFields;
        /// <summary>
        /// Returns the string array consisting of the 
        /// database primary key columns.
        /// </summary>
        public string[] PrimaryKeyColumns
        {
            get
            {
                if (_PrimaryKeyFields == null)
                {
                    var f = new List<string>();
                    foreach (var field_kvp in this.FieldMappings)
                    {
                        var field = field_kvp.Value;
                        if (field.IsPrimaryKey) f.Add(field.ColumnName);
                    }
                    _PrimaryKeyFields = f.ToArray();
                }
                return _PrimaryKeyFields;
            }
        }

        /// <summary>
        /// Gets the <see cref="Type"/> associated with this mapping object.
        /// </summary>
        [XmlIgnore]
        public Type EntityType
        {
            get { return _EntityType; }
            private set { _EntityType = value; }
        }

        [NonSerialized] private Type _EntityType;

        /// <summary>
        /// Gets or sets the name of the Type associated with this mapping object.
        /// </summary>
        [XmlElement("entityType")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string EntityTypeName
        {
            get
            {
                return EntityType.FullName;
            }
            set
            {
                if (EntityType != null) 
                    throw new InvalidOperationException("EntityType is read-only once it has been set.");
                EntityType = Type.GetType(value);
            }
        }

        /// <summary>
        /// Returns a <see cref="DataModelColumnAttribute"/>
        /// for the associated CLR object's mapping
        /// using the specified <paramref name="columnName"/> as the
        /// database column name with which to identify the 
        /// CLR member / DB column field mapping.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public DataModelColumnAttribute GetFieldMappingByDbColumnName(string columnName)
        {
            // todo: optimize
            foreach (var fieldMap_kvp in FieldMappings)
            {
                if (fieldMap_kvp.Value.ColumnName.ToLower() == (columnName ?? "").ToLower())
                {
                    return fieldMap_kvp.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a dictionary of <see cref="DataModelColumnAttribute"/>s
        /// associated with this table / CLR type map. The
        /// <see cref="DataModelColumnAttribute"/>s identify the mappings
        /// between a CLR object's properties/fields and the columns of a database 
        /// table. The indexer for this dictionary is keyed by the CLR object's
        /// property/field member name, not the DB column name.
        /// </summary>
        [XmlIgnore]
        public CaseInsensitiveDictionary<DataModelColumnAttribute> FieldMappings
        {
            get { return _ColumnMappings; }
            private set { _ColumnMappings = value; }
        }

        [NonSerialized]
        private CaseInsensitiveDictionary<DataModelColumnAttribute> _ColumnMappings;

        /// <summary>
        /// Exposes an XML serializable interface for populating the ColumnMappings dictionary.
        /// </summary>
        /// <remarks>This property is not exposed to IntelliSense if the binary assembly is referenced.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlArray("columnMappings")]
        [XmlArrayItem("column", typeof(DataModelColumnAttribute))]
        public DataModelColumnAttribute[] ColumnMappingsList
        {
            get
            {
                return FieldMappings.Values.ToArray();
            }
            set
            {
                foreach (var fmap in value)
                {
                    (FieldMappings ?? (FieldMappings = new CaseInsensitiveDictionary<DataModelColumnAttribute>()))
                        [fmap.TargetMemberName] = fmap;
                }
            }
        }

        /// <summary>
        /// Returns a dictionary of <see cref="ForeignDataModelAttribute"/>s
        /// associated with this table / CLR type map. The
        /// <see cref="ForeignDataModelAttribute"/>s identify the foreign key
        /// relationships between a CLR object's property/field and a foreign
        /// key in a database.
        /// </summary>
        [XmlIgnore]
        public CaseInsensitiveDictionary<ForeignDataModelAttribute> ForeignModelMappings
        {
            get { return _ForeignDataModelMappings; }
            private set { _ForeignDataModelMappings = value; }
        }

        [NonSerialized]
        private CaseInsensitiveDictionary<ForeignDataModelAttribute> _ForeignDataModelMappings;

        /// <summary>
        /// Exposes an XML serializable interface for populating the ForeignModelMappings dictionary.
        /// </summary>
        /// <remarks>This property is not exposed to IntelliSense if the binary assembly is referenced.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlArray("foreignModelMappings")]
        [XmlArrayItem("foreignModelMappingsList", typeof(ForeignDataModelAttribute))]
        public ForeignDataModelAttribute[] ForeignModelMappingsList
        {
            get
            {
                return ForeignModelMappings.Values.ToArray();
            }
            set
            {
                foreach (var fdm in value)
                {
                    try
                    {
                        (ForeignModelMappings ??
                         (ForeignModelMappings = new CaseInsensitiveDictionary<ForeignDataModelAttribute>()))
                            [fdm.TargetMemberName] = fdm;
                    } catch
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="DataModelTableAttribute"/> associated with this
        /// table / CLR type map. The <see cref="DataModelTableAttribute"/>
        /// describes, for example, the name of the table and its containing schema.
        /// </summary>
        [XmlElement("tableMapping")]
        public DataModelTableAttribute TableMapping { get; set; }

        /// <summary>
        /// Gets the <see cref="DataModelColumnAttribute"/> having the
        /// specified name and naming type.
        /// </summary>
        /// <param name="propertyNameOrColumnName"></param>
        /// <returns></returns>
        public DataModelColumnAttribute this[string propertyNameOrColumnName]
        {
            get
            {
                if (FieldMappings.ContainsKey(propertyNameOrColumnName))
                    return FieldMappings[propertyNameOrColumnName];
                var ret = GetFieldMappingByDbColumnName(propertyNameOrColumnName);
                if (ret != null) return ret;
                if (ForeignModelMappings.ContainsKey(propertyNameOrColumnName))
                {
                    if (FieldMappings.ContainsKey(ForeignModelMappings[propertyNameOrColumnName].TargetMember.Name))
                    {
                        return FieldMappings[ForeignModelMappings[propertyNameOrColumnName].TargetMember.Name];
                    }
                    var foreignMapping = ForeignModelMappings[propertyNameOrColumnName];
                    var mapping = FieldMappings.ToList().Find(fm => fm.Value.ColumnName == foreignMapping.LocalColumn).Value;
                    return mapping;
                }
                return null;
            }
        }

        /// <summary>f
        /// Gets the <see cref="DataModelColumnAttribute"/> having the
        /// specified name and naming type.
        /// </summary>
        /// <param name="propertyNameOrColumnName"></param>
        /// <param name="keyType"></param>
        /// <returns></returns>
        public DataModelColumnAttribute this[string propertyNameOrColumnName, FieldMappingKeyType keyType]
        {
            get
            {
                switch (keyType)
                {
                    case FieldMappingKeyType.ClrMember:
                        return this.FieldMappings[propertyNameOrColumnName];
                    case FieldMappingKeyType.DbColumn:
                        return this.GetFieldMappingByDbColumnName(propertyNameOrColumnName);
                }
                return null;
            }
        }

        private DataModelTableAttribute GetTableMapByAttributes(Type type)
        {
            var hierarchy = new List<Type>();
            var t = type;
            while (t != typeof(DataModel) && t != typeof(object))
            {
                hierarchy.Insert(0, t = t.BaseType);
            }
            hierarchy.Add(type);

            DataModelTableAttribute ret = null;
            // walk up hierarchy
            foreach (var baseType in hierarchy)
            {
                var attrs = baseType.GetCustomAttributes(typeof(DataModelTableAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    if (ret != null &&
                        !((DataModelTableAttribute)attrs[0]).ClearBaseObjectMapping)
                    {
                        ((DataModelTableAttribute)attrs[0]).CopyDeltaTo(ret);
                    }
                    else ret = (DataModelTableAttribute) attrs[0];
                    if (string.IsNullOrEmpty(ret.Table)) ret.Table = type.Name;
                }
            }
            if (ret == null) ret = new DataModelTableAttribute(type.Name);
            return ret;
        }

        private void LoadFieldMappingAttributes()
        {
            var hierarchy = new List<Type>();
            var t = this.EntityType;
            while (t != typeof(DataModel) && t != typeof(object))
            {
                hierarchy.Insert(0, t);
                t = t.BaseType;
            }
            // walk up hierarchy
            foreach (var type in hierarchy)
            {
                var pis = type.GetProperties();
                var fis = type.GetFields();
                var mis = new Dictionary<MemberInfo, Type>();
                foreach (var fi in fis) mis.Add(fi, fi.FieldType); // fee fi fo fum
                foreach (var pi in pis) mis.Add(pi, pi.PropertyType);
                foreach (var mi_kvp in mis)
                {
                    var mi = mi_kvp.Key;
                    var miType = mi_kvp.Value;
                    var attrs = mi.GetCustomAttributes(typeof(DataModelColumnAttribute), false);
                    if (!TypeIsFieldMappable(miType))
                    {
                        if (attrs.Length > 0)
                        {
                            throw new InvalidAttributeException("Cannot apply a " + typeof(DataModelColumnAttribute).Name
                                + " to a member having a custom or complex type. "
                                + "Use " + typeof(ForeignDataModelAttribute).Name + ".",
                                mi.Name, (Attribute)attrs[0]);
                        }
                        continue;
                    }
                    foreach (DataModelColumnAttribute attr in attrs)
                    {
                        attr.TargetMember = mi;
                        attr.TargetMemberType = miType;
                        if (string.IsNullOrEmpty(attr.ColumnName))
                        {
                            attr.ColumnName = mi.Name;
                            attr.IsColumnNameInferred = true;
                        }
                        if (FieldMappings.ContainsKey(mi.Name) && !attr.ClearBaseObjectMapping)
                        {
                            attr.CopyDeltaTo(FieldMappings[mi.Name]);
                        }
                        else FieldMappings[mi.Name] = attr;
                        var fm = FieldMappings[mi.Name];
                        if (fm.DataType == null)
                        {
                            var def = miType;
                            Type[] genargs;
                            if (def.IsGenericType &&
                                (genargs = def.GetGenericArguments()).Length == 1)
                            {
                                def = genargs[0];
                            }
                            try
                            {
                                fm.DbType = DbTypeConverter.ToDbType(def);
                            }
                            catch { }
                        }
                    }
                }
            }
            if ((FieldMappings.Count == 0 && TableMapping.PropertyLoadBehavior
                == InferProperties.OnlyAttributedOrAllIfNoneHaveAttributes) ||
                TableMapping.PropertyLoadBehavior == InferProperties.NotIgnored)
            {
                foreach (var type in hierarchy)
                {
                    var pis = type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
                    var fis = type.GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
                    var mis = new Dictionary<MemberInfo, Type>();
                    foreach (var fi in fis) mis.Add(fi, fi.FieldType); // fee fi fo fum
                    foreach (var pi in pis) mis.Add(pi, pi.PropertyType);
                    foreach (var mi_kvp in mis)
                    {
                        var mi = mi_kvp.Key;
                        if (!FieldMappings.ContainsKey(mi.Name))
                        {
                            var miType = mi_kvp.Value;
                            var ignoreAttrib = mi.GetCustomAttributes(typeof (DataModelIgnoreAttribute), false);
                            if (ignoreAttrib != null && ignoreAttrib.Length > 0) continue;
                            var gt = miType;
                            if (!TypeIsFieldMappable(gt)) continue;
                            var attr = new DataModelColumnAttribute(mi.Name);
                            attr.TargetMember = mi;
                            attr.TargetMemberType = miType;
                            FieldMappings[mi.Name] = attr;
                            var fm = attr;
                            if (fm.DataType == null)
                            {
                                var def = miType;
                                Type[] genargs;
                                if (def.IsGenericType &&
                                    (genargs = def.GetGenericArguments()).Length == 1)
                                {
                                    def = genargs[0];
                                }
                                try
                                {
                                    fm.DbType = DbTypeConverter.ToDbType(def);
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool TypeIsFieldMappable(Type t)
        {
            if (t.IsGenericType &&
                t.FullName.StartsWith("System.Nullable`1") &&
                t == (typeof(Nullable<>).MakeGenericType(t.GetGenericArguments()[0])))
            {
                t = t.GetGenericArguments()[0];
            }
            if (!(t.IsPrimitive) &&
                t != typeof(decimal) &&
                t != typeof(string) &&
                t != typeof(byte[]) &&
                t != typeof(Guid) &&
                t != typeof(DateTime) &&
                t != typeof(DateTimeOffset) &&
                t != typeof(object) &&
                t != typeof(System.Xml.XmlDocument)) return false;
            return true;
        }

        private void LoadForeignKeyMappingAttributes()
        {
            var hierarchy = new List<Type>();
            var t = this.EntityType;
            while (t != typeof(DataModel) && t != typeof(object))
            {
                hierarchy.Insert(0, t);
                t = t.BaseType;
            }
            // walk up hierarchy
            foreach (var type in hierarchy)
            {
                var pis = type.GetProperties();
                var fis = type.GetFields();
                var mis = new Dictionary<MemberInfo, Type>();
                foreach (var fi in fis) mis.Add(fi, fi.FieldType); // fee fi fo fum
                foreach (var pi in pis) mis.Add(pi, pi.PropertyType);
                foreach (var mi_kvp in mis)
                {
                    var mi = mi_kvp.Key;
                    var miType = mi_kvp.Value;
                    var attrs = mi.GetCustomAttributes(typeof (ForeignKeyAttribute), false);
                    foreach (ForeignKeyAttribute attr in attrs)
                    {
                        attr.TargetMember = mi;
                        attr.TargetMemberType = miType;
                        if (!FieldMappings.ContainsKey(mi.Name))
                        {
                            FieldMappings[mi.Name] 
                                = new DataModelColumnAttribute(attr.ForeignColumn);
                        }
                        if (FieldMappings[mi.Name].ForeignKeyMapping != null && !attr.ClearBaseObjectMapping)
                        {
                            attr.CopyDeltaTo(FieldMappings[mi.Name].ForeignKeyMapping);
                        }
                        else FieldMappings[mi.Name].ForeignKeyMapping = attr;
                    }
                }
            }
        }

        private void LoadForeignDataModelMappingAttributes()
        {
            var hierarchy = new List<Type>();
            var t = this.EntityType;
            while (t != typeof(DataModel) && t != typeof(object))
            {
                hierarchy.Insert(0, t);
                t = t.BaseType;
            }
            // walk up hierarchy
            foreach (var type in hierarchy)
            {
                var pis = type.GetProperties();
                var fis = type.GetFields();
                var mis = new Dictionary<MemberInfo, Type>();
                foreach (var fi in fis) mis.Add(fi, fi.FieldType); // fee fi fo fum
                foreach (var pi in pis) mis.Add(pi, pi.PropertyType);
                foreach (var mi_kvp in mis)
                {
                    var mi = mi_kvp.Key;
                    var miType = mi_kvp.Value;
                    var attrs = mi.GetCustomAttributes(typeof(ForeignDataModelAttribute), false);
                    foreach (ForeignDataModelAttribute attr in attrs)
                    {
                        attr.TargetMember = mi;
                        attr.TargetMemberType = miType;
                        if (ForeignModelMappings.ContainsKey(mi.Name) && !attr.ClearBaseObjectMapping)
                        {
                            attr.CopyDeltaTo(ForeignModelMappings[mi.Name]);
                        }
                        else ForeignModelMappings[mi.Name] = attr;
                    }
                }
            }
        }

        private void LoadMappingXml()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// A two-argument generic type 
        /// for generic argument type containment.
        /// </summary>
        /// <remarks>
        /// Used internally to deal with classes that are, for example,
        /// mapped to mapping tables (many-to-many relationships).
        /// <seealso cref="RuntimeMappingTable{TLeft,TRight}"/>
        /// </remarks>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        public class MappingType<A,B> { }

        /// <summary>
        /// Maps a virtual foreign key relationship with CLR type bindings.
        /// </summary>
        /// <remarks>
        /// Used internally to deal with classes that are, for example,
        /// mapped to mapping tables (many-to-many relationships).
        /// <seealso cref="DataModelMap.MappingType{A,B}"/>
        /// </remarks>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        public class RuntimeMappingTable<TLeft, TRight> : DataModel where TRight : DataModel
        {

            /// <summary>
            /// The local column of the relationship in this binding.
            /// </summary>
            [DataModelColumn]
            public object LeftColumn
            {
                get { return base["LeftColumn"]; }
                set { base["LeftColumn"] = value; }
            }

            /// <summary>
            /// The object or the collection that is related to the <see cref="LeftColumn"/>.
            /// This field declaration is used only for the attribute.
            /// </summary>
            [ForeignDataModel(Relationship = Relationship.ManyToMany)]
            public DataModelCollection<TRight> ForeignModelCollection;
        }

        /// <summary>
        /// Returns the <see cref="DataModelMap"/> that is associated
        /// with the specified CLR <paramref name="type"/>. This value
        /// contains all the mapping information needed to bind a CLR
        /// type to a database table and its relationships.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DataModelMap GetEntityMapping(Type type)
        {
            if (type.IsDataModelWrapper(false))
            {
                type = type.GetDataModelWrapperGenericTypeArg();
            }
            if (!MapItems.ContainsKey(type))
            {
                return GetEntityMapping(type, MapItems);
            }

            return MapItems[type];
        }


        private static readonly DataModelMappingsDefinition _MapDictionary
            = new DataModelMappingsDefinition();
        /// <summary>
        /// Returns the static type map dictionary for all
        /// loaded <see cref="DataModelMap"/>s.
        /// </summary>
        public static DataModelMappingsDefinition MapItems
        {
            get { return _MapDictionary; }
        }

        /// <summary>
        /// Loads complete mapping configuration definitions
        /// from XML.
        /// </summary>
        /// <param name="filePath"></param>
        public static void LoadMappings(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                var xml = sr.ReadToEnd();
                var serializedDef = new XmlSerialized<DataModelMappingsDefinition>(xml);
                var mappings = serializedDef.Deserialize();
                foreach (var mapping_kvp in mappings)
                {
                    var key = mapping_kvp.Key;
                    var mapping = mapping_kvp.Value;
                    MapItems[key] = mapping;
                }
            }
        }

        /// <summary>
        /// Writes the complete mapping configuration definitions to an XML file.
        /// </summary>
        /// <param name="filePath"></param>
        public static void SaveMappings(string filePath)
        {
            using (var sw = new StreamWriter(filePath))
            {
                var serializedDef = new XmlSerialized<DataModelMappingsDefinition>(MapItems);
                sw.Write(serializedDef.SerializedValue);
                sw.Flush();
            }
        }

        private static DataModelMap GetEntityMapping(Type type, 
            IDictionary<Type, DataModelMap> dictionary)
        {
            if (dictionary.ContainsKey(type)) return dictionary[type];
            var hierarchy = new List<Type>();
            var t = type;
            while(t != typeof(DataModel) && t != typeof(object))
            {
                hierarchy.Insert(0, t = t.BaseType);
            }
            hierarchy.Add(type);

            // walk up hierarchy
            foreach (var baseType in hierarchy)
            {
                if (!dictionary.ContainsKey(baseType) && baseType != typeof(object))
                {
                    dictionary.Add(baseType, new DataModelMap(baseType));
                }
            }
            var ret = dictionary[type];
            if (!ret._FieldMapBehaviorsExecuted)
            {
                ret.ExecuteFieldMapBehaviors();
            }
            return ret;
        }

    }
}
