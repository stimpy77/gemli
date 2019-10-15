using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Describes how a <see cref="DataModelQuery&lt;T&gt;"/> comparison
    /// is made on behalf of a particular property/field/column.
    /// </summary>
    public enum Compare
    {
        /// <summary>
        /// Evaluate whether the two values are equal.
        /// </summary>
        Equal,
        /// <summary>
        /// Evaluate whether the two values are not equal.
        /// </summary>
        NotEqual,
        /// <summary>
        /// Evaluate whether the two values match with a LIKE clause.
        /// </summary>
        Like,
        /// <summary>
        /// Evaluate whether the left (CLR) value is less than the right (DB) value.
        /// </summary>
        LessThan,
        /// <summary>
        /// Evaluate whether the left (CLR) value is less than or equal to the right (DB) value.
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        /// Evaluate whether the left (CLR) value is greater than the right (DB) value.
        /// </summary>
        GreaterThan,
        /// <summary>
        /// Evaluate whether the left (CLR) value is greater than or equal to the right (DB) value.
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// Evaluate whether the right (DB) value is null.
        /// </summary>
        Null,
        /// <summary>
        /// Evaluate whether the right (DB) value is not null.
        /// </summary>
        NotNull
    }
}
