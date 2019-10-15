using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Assigns basic mapping between a database table
    /// and a CLR class.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class DataModelTableMappingAttribute : DataModelMappingAttributeBase
    {
        /// <summary>
        /// Constructs the mapping object with no initial table name
        /// and an assumed schema mapping of "dbo".
        /// </summary>
        public DataModelTableMappingAttribute() : this("dbo", null) {}

        /// <summary>
        /// Constructs the mapping object with the specfied table
        /// table <paramref name="name"/>
        /// and an assumed schema mapping of "dbo" or of
        /// "[schema].[table]" if dot-delimited.
        /// </summary>
        /// <param name="name"></param>
        public DataModelTableMappingAttribute(string name)
            : this(
                name.Split('.').Length == 2
                    ? name.Split('.')[0]
                    : "dbo",
                name.Split('.').Length == 2
                    ? name.Split('.')[1]
                    : name)
        {
        }

        /// <summary>
        /// Constructs the mapping object with the specified 
        /// <paramref name="schema"/> name and the specified
        /// <paramref name="table"/> name.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        public DataModelTableMappingAttribute(string schema, string table)
        {
            this.Table = table;
            this.Schema = schema;
        }

        /// <summary>
        /// The name of the database table that the associated
        /// CLR type is mapped to.
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        /// The name of the database schema of the table that the associated
        /// CLR type is mapped to.
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Specifies a SQL stored procedure that should be
        /// used instead of generated ad hoc SQL when performing
        /// a single-row select to obtain a data record that
        /// maps to a single entity object.
        /// </summary>
        public string SelectProcedure { get; set; }

        /// <summary>
        /// Specifies a SQL stored procedure that should be
        /// used instead of generated ad hoc SQL when performing
        /// a multi-row select to obtain a result set of data
        /// that maps to a collection of entity objects.
        /// </summary>
        public string SelectManyProcedure { get; set; }

        /// <summary>
        /// Specifies a SQL stored procedure that should be 
        /// used instead of generated ad hoc SQL when performing
        /// an INSERT of a single row of data that originated
        /// from an entity object.
        /// </summary>
        public string InsertProcedure { get; set; }

        /// <summary>
        /// Specifies a SQL stored procedure that should be
        /// used instead of generated ad hoc SQL when performing
        /// an UPDATE to a single row of data that maps
        /// to an entity object.
        /// </summary>
        public string UpdateProcedure { get; set; }

        /// <summary>
        /// Specifies a SQL stored procedure that should be
        /// used instead of generated ad hoc SQL when performing
        /// a DELETE of a single row of data that originated
        /// from an entity object.
        /// </summary>
        public string DeleteProcedure { get; set; }

        /// <summary>
        /// Replicates the properties from this object
        /// to the specified <see cref="DataModelMappingAttributeBase"/> object.
        /// </summary>
        /// <param name="dattribute"></param>
        protected internal override void CopyDeltaTo(DataModelMappingAttributeBase dattribute)
        {
            var attribute = (DataModelTableMappingAttribute) dattribute;
            if (!string.IsNullOrEmpty(this.Schema)) attribute.Schema = this.Schema;
            if (!string.IsNullOrEmpty(this.Table)) attribute.Table = this.Table;
        }

        /// <summary>
        /// Returns the schema and table mapping in the format "[Schema].[Table]".
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var ret = Table;
            if (!string.IsNullOrEmpty(Schema)) //&&
                //Schema != "dbo")
            {
                ret = "[" + Schema + "].[" + ret + "]";
            }
            return ret;
        }
    }
}
