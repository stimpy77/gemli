using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Marks a property that should not be loaded with inferences
    /// into a <see cref="DataModelMap"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DataModelIgnoreAttribute : Attribute
    {
    }
}
