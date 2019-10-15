using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Identifies which standard database action (Create, Read, Update, Delete).
    /// </summary>
    public enum CrudOp
    {
        /// <summary>
        /// Represents, for example, an INSERT statement in SQL.
        /// </summary>
        Create,
        /// <summary>
        /// Represents, for example, a SELECT clause in SQL.
        /// </summary>
        Read,
        /// <summary>
        /// Represents, for example, an UPDATE statement in SQL.
        /// </summary>
        Update,
        /// <summary>
        /// Represents, for example, a DELETE statement in SQL.
        /// </summary>
        Delete
    }
}
