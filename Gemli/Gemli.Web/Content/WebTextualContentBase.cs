using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gemli.Web.Content;
using Gemli.Web.Utilties;

namespace Gemli.Web.Content
{
    /// <summary>
    /// String container object for working with HTML-encoded and
    /// non-encoded text.
    /// </summary>
    public class WebTextualContentBase : TextualContentBase
    {
        /// <summary>
        /// Gets or sets the HTML text.
        /// </summary>
        public string HtmlText { get; set; }
    }
}