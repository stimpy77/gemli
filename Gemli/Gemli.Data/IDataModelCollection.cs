using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Exposes <see cref="ICollection"/> functions and 
    /// indexed getters/setters for a typed 
    /// <see cref="DataModel"/> collection.
    /// </summary>
    public interface IDataModelCollection : ICollection
    {
        /// <summary>
        /// When implemented, returns the <see cref="DataModel"/>
        /// object from a <see cref="DataModelCollection{TEntity}"/>
        /// at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        DataModel GetDataModelAt(int index);

        /// <summary>
        /// When implemented, sets the <see cref="DataModel"/>
        /// object in the <see cref="DataModelCollection{TEntity}"/>
        /// at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        void SetDataModelAt(int index, DataModel value);
    }
}
