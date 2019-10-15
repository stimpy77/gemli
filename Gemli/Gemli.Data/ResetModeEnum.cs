using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Describes the extent or kind of reset to perform on a <see cref="DataModel"/>.
    /// </summary>
    public enum ResetMode
    {
        /// <summary>
        /// Restores the field values since the last Save
        /// and sets the IsDirty flag to false.
        /// </summary>
        RevertNotDirty,
        /// <summary>
        /// Keeps the current field values
        /// but sets the IsDirty flag to false.
        /// </summary>
        RetainNotDirty,
        /// <summary>
        /// Clears the current field values completely
        /// and sets the IsNew flag to true.
        /// </summary>
        ClearAndNew
    }
}