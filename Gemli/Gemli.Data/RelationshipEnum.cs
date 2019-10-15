using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Describes the JOIN relationship with respect to a foreign key.
    /// </summary>
    public enum Relationship
    {
        /// <summary>
        /// The database relationship between the two tables on the foreign key is one-to-one.
        /// </summary>
        OneToOne,
        /// <summary>
        /// The database relationship between the two tables on the foreign key is one-to-many.
        /// </summary>
        OneToMany,
        /// <summary>
        /// The database relationship between the two tables on the foreign key is many-to-one.
        /// </summary>
        ManyToOne,
        /// <summary>
        /// The database relationship between the two tables on the foreign key is many-to-many.
        /// </summary>
        ManyToMany
    }
}
