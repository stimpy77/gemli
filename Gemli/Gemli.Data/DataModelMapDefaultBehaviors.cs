using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Defines the default behaviors of the <see cref="DataModelMap"/> 
    /// type and the types that are associated with it
    /// such as <see cref="DataModelTableAttribute"/>.
    /// </summary>
    public class DataModelMapDefaultBehaviors
    {
        /// <summary>
        /// Instantiates the default behaviors object with
        /// default behavior values.
        /// </summary>
        public DataModelMapDefaultBehaviors()
        {
            PropertyLoadBehavior = InferProperties.OnlyAttributedOrAllIfNoneHaveAttributes;
        }
        /// <summary>
        /// Gets or sets the inference behavior of the properties.
        /// </summary>
        public InferProperties PropertyLoadBehavior { get; set; }
    }
}
