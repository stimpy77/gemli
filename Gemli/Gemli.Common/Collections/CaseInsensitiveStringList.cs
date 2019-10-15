using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Collections
{
    /// <summary>
    /// A string collection that evaluates the various match methods
    /// such as Contains() or Remove() without case sensitivity.
    /// </summary>
    [Serializable] 
    public class CaseInsensitiveStringList : List<string>
    {
        /// <summary>
        /// Returns true if the collection contains the specified
        /// value, without case sensitivity.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new bool Contains(string value)
        {
            if (base.Contains(value)) return true;
            string lcval = value.ToLower();
            for (int i=0; i<Count; i++)
            {
                if (base[i].ToLower() == lcval) return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the specified value from the collection, without
        /// case sensitivity.
        /// </summary>
        /// <param name="value"></param>
        public new void Remove(string value)
        {
            if (base.Contains(value))
            {
                base.Remove(value);
                return;
            }
            string lcval = value.ToLower();
            for (int i = 0; i < Count; i++)
            {
                if (base[i].ToLower() == lcval)
                {
                    RemoveAt(i);
                    return;
                }
            }
            throw new ArgumentException("Item not found.");
        }
    }
}