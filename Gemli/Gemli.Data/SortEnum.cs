using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Describes how data should be sorted on a particular column.
    /// </summary>
    public enum Sort
    {
        /// <summary>
        /// The data should be sorted ascending (a-Z).
        /// </summary>
        Ascending,
        /// <summary>
        /// The data should be sorted descending (z-A).
        /// </summary>
        Descending
    }
}
