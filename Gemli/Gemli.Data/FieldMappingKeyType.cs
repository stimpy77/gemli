using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Describes how a database field mapping is matched 
    /// by name--by the CLR property/field or by the database
    /// column name.
    /// </summary>
    public enum FieldMappingKeyType
    {
        /// <summary>
        /// Matched by the name of the CLR property or field.
        /// </summary>
        ClrMember,
        /// <summary>
        /// Matched by the database column name.
        /// </summary>
        DbColumn
    }

}
