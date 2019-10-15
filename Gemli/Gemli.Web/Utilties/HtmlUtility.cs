using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gemli.Xml;

namespace Gemli.Web.Utilties
{
    /// <summary>
    /// Contains utility methods that assist in working with HTML content.
    /// </summary>
    public class HtmlUtility
    {
        /// <summary>
        /// Replaces all special characters with encoded HTML sequences.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string HtmlEncode(string s)
        {
            return XmlUtility.EncodeText(s); // todo: implement HtmlEncode(s)
        }

        /// <summary>
        /// Cleans out malicious or malformatted HTML.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string MakeSafe(string s)
        {
            return s; // todo: implement HtmlUtility.MakeSafe()
        }
    }
}