using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Used by <see cref="DataModelTableAttribute"/> to
    /// determine which properties are loaded by inference.
    /// </summary>
    public enum InferProperties
    {
        /// <summary>
        /// Indicates that the all properties not marked with
        /// <see cref="DataModelIgnoreAttribute"/> will 
        /// be loaded to the data model map with inferences.
        /// </summary>
        NotIgnored,
        /// <summary>
        /// Indicates that if none of the properties are 
        /// attributed then all of the properties are loaded
        /// with inferences, or if one or more properties 
        /// are attributed then only those with attributes
        /// are loaded with inferences.
        /// </summary>
        OnlyAttributedOrAllIfNoneHaveAttributes
    }
}
