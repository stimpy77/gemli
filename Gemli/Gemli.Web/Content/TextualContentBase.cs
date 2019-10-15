using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Gemli.Data;

namespace Gemli.Web.Content
{
    /// <summary>
    /// String container object for working with non-encoded text.
    /// </summary>
    public class TextualContentBase //: DataModel<TextualContentBase>
    {
        /// <summary>
        /// Gets or sets the non-encoded content.
        /// </summary>
        public virtual string Text { get; set; }
    }
}