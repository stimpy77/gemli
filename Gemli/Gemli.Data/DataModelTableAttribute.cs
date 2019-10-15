using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Gemli.Data
{
    /// <summary>
    /// Assigns basic mapping between a database table
    /// and a CLR class.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class DataModelTableAttribute : DataModelMappingAttributeBase
    {
        /// <summary>
        /// Constructs the mapping object with no initial table name
        /// and an assumed schema mapping from 
        /// <see cref="Providers.ProviderDefaults.DefaultSchema"/>.
        /// </summary>
        public DataModelTableAttribute() : this(Providers.ProviderDefaults.DefaultSchema, (string)null) {}

        /// <summary>
        /// Constructs the mapping object with the specfied table
        /// table <paramref name="name"/>
        /// and an assumed schema mapping of 
        /// <see cref="Providers.ProviderDefaults.DefaultSchema"/>
        /// or of
        /// "[schema].[table]" if dot-delimited.
        /// </summary>
        /// <param name="name"></param>
        public DataModelTableAttribute(string name)
            : this(AutoSplit(name)[0], AutoSplit(name)[1], AutoSplit(name)[2])
        {
        }

        /// <summary>
        /// Constructs the mapping object with the specfied table
        /// table <paramref name="name"/>
        /// and an assumed schema mapping of 
        /// <see cref="Providers.ProviderDefaults.DefaultSchema"/>
        /// or of
        /// "[schema].[table]" if dot-delimited.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataProvider">The database provider that stores this model.</param>
        public DataModelTableAttribute(string name, Providers.DbDataProvider dataProvider)
            : this(AutoSplit(name, dataProvider)[0], 
              AutoSplit(name, dataProvider)[1], 
              AutoSplit(name, dataProvider)[2])
        {
        }

        /// <summary>
        /// Constructs the mapping object with the specified 
        /// <paramref name="schema"/> name and the specified
        /// <paramref name="table"/> name.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        public DataModelTableAttribute(string schema, string table)
        {
            this.Table = table;
            this.Schema = schema;
        }

        /// <summary>
        /// Constructs the mapping object with the specified 
        /// <paramref name="catalog"/> name, the specified
        /// <paramref name="schema"/> name and the specified
        /// <paramref name="table"/> name.
        /// </summary>
        /// <param name="catalog">
        /// Indicates the database (or "catalog") that
        /// contains the associated table.
        /// </param>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        public DataModelTableAttribute(string catalog, string schema, string table)
            : this(schema, table)
        {
            Catalog = catalog;
        }

        private static string __autosplit_lastName = "";
        private static string[] __autosplit_lastret = new string[]{};
        private static string[] AutoSplit(string name)
        {
            return AutoSplit(name, Providers.ProviderDefaults.AppProvider as Providers.DbDataProvider);
        }
        private static string[] AutoSplit(string name, Providers.DbDataProvider dataProvider)
        {
            lock (__autosplit_lastret)
            {
                lock (__autosplit_lastName)
                {
                    if (__autosplit_lastName == name) return __autosplit_lastret;
                }
            }
            var ret = new string[3];
            __autosplit_lastret = ret;
            lock (__autosplit_lastret)
            {
                __autosplit_lastName = name;
                lock (__autosplit_lastName)
                {
                    var s = '.';
                    var c = s;
                    DbCommandBuilder cmdb = null;
                    if (dataProvider != null)
                    {
                        cmdb = ((Providers.DbDataProvider) Providers.ProviderDefaults.AppProvider)
                            .DbFactory.CreateCommandBuilder();
                        s = cmdb.SchemaSeparator.ToCharArray()[0];
                        if (cmdb.CatalogSeparator != cmdb.SchemaSeparator)
                        {
                            c = cmdb.CatalogSeparator.ToCharArray()[0];
                        }
                    }
                    var splits = name.Split(s);
                    if (s!=c)
                    {
                        var splits2 = new string[splits.Length + 2];
                        var cat = name.Split(c);
                        if (cmdb != null &&
                            cmdb.CatalogLocation == CatalogLocation.Start)
                        {
                            cat.CopyTo(splits2, 0);
                            splits.CopyTo(splits2, 1);
                        }
                        else if (cmdb != null &&
                            cmdb.CatalogLocation == CatalogLocation.End)
                        {
                            splits.CopyTo(splits2, 0);
                            cat.CopyTo(splits2, splits.Length);
                        }
                        splits = splits2;
                    }
                    if (splits.Length == 1)
                    {
                        ret[0] = Providers.ProviderDefaults.DefaultCatalog;
                        ret[1] = Providers.ProviderDefaults.DefaultSchema;
                        ret[2] = splits[0];
                    }
                    else if (splits.Length == 2)
                    {
                        ret[0] = Providers.ProviderDefaults.DefaultCatalog;
                        ret[1] = splits[0];
                        ret[2] = splits[1];
                    }
                    else if (splits.Length == 3)
                    {
                        ret = splits;
                    }
                    return ret;
                }
            }
        }

        /// <summary>
        /// The name of the database table that the associated
        /// CLR type is mapped to.
        /// </summary>
        [XmlElement("table")]
        public string Table { get; set; }
        
        /// <summary>
        /// The name of the database schema of the table that the associated
        /// CLR type is mapped to.
        /// </summary>
        [XmlElement("schema")]
        public string Schema { get; set; }

        /// <summary>
        /// The name of the database (or "catalog") that contains the
        /// table that the associated CLR type is mapped to.
        /// </summary>
        [XmlElement("catalog")]
        public string Catalog { get; set; }

        /// <summary>
        /// Specifies a SQL stored procedure that should be
        /// used instead of generated ad hoc SQL when performing
        /// a single-row select to obtain a data record that
        /// maps to a single entity object.
        /// </summary>
        [XmlElement("selectProc")]
        public string SelectProcedure { get; set; }

        /// <summary>
        /// Specifies a SQL stored procedure that should be
        /// used instead of generated ad hoc SQL when performing
        /// a multi-row select to obtain a result set of data
        /// that maps to a collection of entity objects.
        /// </summary>
        [XmlElement("selectManyProc")]
        public string SelectManyProcedure { get; set; }

        /// <summary>
        /// Specifies a SQL stored procedure that should be 
        /// used instead of generated ad hoc SQL when performing
        /// an INSERT of a single row of data that originated
        /// from an entity object.
        /// </summary>
        [XmlElement("insertProc")]
        public string InsertProcedure { get; set; }

        /// <summary>
        /// Specifies a SQL stored procedure that should be
        /// used instead of generated ad hoc SQL when performing
        /// an UPDATE to a single row of data that maps
        /// to an entity object.
        /// </summary>
        [XmlElement("updateProc")]
        public string UpdateProcedure { get; set; }

        /// <summary>
        /// Specifies a SQL stored procedure that should be
        /// used instead of generated ad hoc SQL when performing
        /// a DELETE of a single row of data that originated
        /// from an entity object.
        /// </summary>
        [XmlElement("deleteProc")]
        public string DeleteProcedure { get; set; }

        private InferProperties? _inferProps;

        /// <summary>
        /// Indicates that the <see cref="PropertyLoadBehavior"/>
        /// property has not been explicitly set and the default
        /// behavior is applied.
        /// </summary>
        protected internal bool PropertyLoadBehaviorDefaulted
        {
            get { return !_inferProps.HasValue; }
        }
        /// <summary>
        /// Gets or sets the inference behavior of the properties.
        /// </summary>
        [XmlElement("propertyLoadBehavior")]
        public InferProperties PropertyLoadBehavior
        {
            get
            {
                if (_inferProps == null)
                    return DataModelMap.DefaultBehaviors.PropertyLoadBehavior;
                return _inferProps.Value;
            }
            set
            {
                _inferProps = value;
            }
        }

        /// <summary>
        /// Replicates the properties from this object
        /// to the specified <see cref="DataModelMappingAttributeBase"/> object.
        /// </summary>
        /// <param name="dattribute"></param>
        protected internal override void CopyDeltaTo(DataModelMappingAttributeBase dattribute)
        {
            var attribute = (DataModelTableAttribute) dattribute;
            if (!string.IsNullOrEmpty(this.Schema)) attribute.Schema = this.Schema;
            if (!string.IsNullOrEmpty(this.Table)) attribute.Table = this.Table;
        }

        /// <summary>
        /// Returns the schema and table mapping in the format "[Schema].[Table]".
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var ret = "[" + Table + "]";
            if (!string.IsNullOrEmpty(Schema))
            {
                ret = "[" + Schema + "]." + ret;
            }
            if (!string.IsNullOrEmpty(Catalog))
            {
                ret = "[" + Catalog + "]." + ret;
            }
            return ret;
        }

        /// <summary>
        /// Returns the schema and table mapping in the format "[Schema].[Table]".
        /// </summary>
        /// <returns></returns>
        public string ToString(DbProviderFactory dbFactory)
        {
            var cmdBuilder = dbFactory.CreateCommandBuilder();
            var ret = cmdBuilder.QuotePrefix + Table + cmdBuilder.QuoteSuffix;
            if (!string.IsNullOrEmpty(Schema))
            {
                ret = cmdBuilder.QuotePrefix + Schema + cmdBuilder.QuoteSuffix 
                    + cmdBuilder.SchemaSeparator + ret;
            }
            if (!string.IsNullOrEmpty(Catalog))
            {
                switch (cmdBuilder.CatalogLocation)
                {
                    case CatalogLocation.Start:
                        ret = cmdBuilder.QuotePrefix + Catalog + cmdBuilder.QuoteSuffix
                              + cmdBuilder.CatalogSeparator + ret;
                        break;
                    case CatalogLocation.End:
                        ret += cmdBuilder.CatalogSeparator
                               + cmdBuilder.QuotePrefix + Catalog + cmdBuilder.QuoteSuffix;
                        break;
                }
            }
            return ret;
        }
    }
}
