using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Describes how two or more sibling comparisons are joined.
    /// </summary>
    public enum ConjunctionOp
    {
        /// <summary>
        /// Filter on all comparisons return true.
        /// </summary>
        And,
        /// <summary>
        /// Filter on any comparison returns true.
        /// </summary>
        Or
    }

}
