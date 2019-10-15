using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Gemli.Data
{
    /// <summary>
    /// An attribute class that describes the mapping between a database
    /// and a CLR object. This class is abstract.
    /// </summary>
    [Serializable]
    public abstract class DataModelMappingAttributeBase : Attribute
    {
        /// <summary>
        /// When implemented, replicates the properties from this object
        /// to the specified <paramref name="attribute"/> object.
        /// </summary>
        /// <param name="attribute"></param>
        protected internal abstract void CopyDeltaTo(DataModelMappingAttributeBase attribute);

        /// <summary>
        /// When true, clears the CLR inheritence hierarchy for the mapping attributes
        /// so that the mappings start with a clean slate.
        /// </summary>
        [XmlElement("clearBase")]
        public bool ClearBaseObjectMapping { get; set; }

        // hide
        /// <summary>
        /// Gets a unique identifier for this attribute object.
        /// </summary>
        protected virtual new object TypeId
        {
            get { return base.TypeId; }
        }

        // hide
        /// <summary>
        /// Indicates whether the value of this instance is the default value for the derived class.
        /// </summary>
        /// <returns></returns>
        protected virtual new bool IsDefaultAttribute()
        {
            return base.IsDefaultAttribute();
        }

        // hide
        /// <summary>
        /// Returns a value that indicates whether this instance equals a specified object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual new bool Match(object obj)
        {
            return base.Match(obj);
        }
    }
}
