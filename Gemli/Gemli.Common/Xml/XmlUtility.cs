using System.IO;
using System.Text;
using System.Xml;

namespace Gemli.Xml
{
    /// <summary>
    /// Contains a number of utility functions for working with XML data.
    /// </summary>
    public class XmlUtility
    {
        private static XmlDocument _staticDoc;
        private static StringWriter _staticStringWriter;
        private static XmlWriter _staticXmlWriter;

        /// <summary>Converts Unicode text into ASCII-compliant XML encoded text</summary>
        public static string EncodeText(string str)
        {
            if (str == null) return "";
            if (_staticDoc == null)
            {
                _staticDoc = new XmlDocument();
                _staticDoc.LoadXml("<text></text>");
                _staticStringWriter = new StringWriter();
                var settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                _staticXmlWriter = XmlWriter.Create(_staticStringWriter, settings);
            }
            lock (_staticDoc)
            {
                _staticDoc.LastChild.InnerText = str;
                str = _staticDoc.LastChild.InnerXml;
            }

            // ASCII enforcement
            var sb = new StringBuilder();
            char[] chars = str.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (c > 127) // goes beyond ASCII charset
                {
                    lock (_staticStringWriter)
                    {
                        lock (_staticXmlWriter)
                        {
                            _staticXmlWriter.WriteCharEntity(c);
                            _staticXmlWriter.Flush();
                            StringBuilder _sb = _staticStringWriter.GetStringBuilder();
                            sb.Append(_sb.ToString());
                            _sb.Length = 0;
                        }
                    }
                }
                else sb.Append(c);
            }
            return sb.ToString();
        }
    }
}