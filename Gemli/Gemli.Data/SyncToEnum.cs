using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Describes from which direction the data between a CLR object 
    /// and a <see cref="DataModel&lt;T&gt;"/> wrapper should be synchronized.
    /// </summary>
    public enum SyncTo
    {
        /// <summary>
        /// The data in the <see cref="DataModel&lt;T&gt;"/> wrapper is 
        /// transferred to its associated <see cref="DataModel.Entity"/>
        /// for loading from the database.
        /// </summary>
        ClrMembers,
        /// <summary>
        /// The data in the <see cref="DataModel.Entity"/> is
        /// transferred to the <see cref="DataModel"/> data dictionary
        /// for saving to the database.
        /// </summary>
        FieldMappedData
    }
}
