using System;
using System.Collections.Generic;
using System.IO;

namespace Gemli.Web.Content
{
    ///<summary>
    /// Loads and checks a MIME Type map for looking up MIME types
    /// and their associated DOS/Windows file types and program
    /// associations.
    ///</summary>
    public class MimeTypeMap
    {
        private const string MimeMapCsv = "Gemli.Web.Content.MimeTypes.csv";

        /// <summary>
        /// Static constructor loads on initial access to type.
        /// </summary>
        static MimeTypeMap()
        {
            InnerMimeMap = new Dictionary<string, List<TypeDescription>>();
            var stream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(MimeMapCsv);
// ReSharper disable AssignNullToNotNullAttribute
            var sr = new StreamReader(stream);
// ReSharper restore AssignNullToNotNullAttribute
            LoadFromMimeTypesCsvFile(sr, false);
        }

        /// <summary>
        /// Loads the MimeTypes from the specified CSV file stream reader.
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="reset"></param>
        public static void LoadFromMimeTypesCsvFile(StreamReader sr, bool reset)
        {
            if (reset) InnerMimeMap.Clear();
            while (true)
            {
                if (sr.EndOfStream) break;
                string line = sr.ReadLine();
                var mapping = line.Split(',');
                string fileType = mapping[0];
                string mimeType = mapping[1];
                string description = mapping.Length >= 3 &&
                    !string.IsNullOrEmpty(mapping[2])
                    ? mapping[2]
                    : mimeType;
                if (!InnerMimeMap.ContainsKey(mimeType))
                    InnerMimeMap.Add(mimeType, new List<TypeDescription>());
                InnerMimeMap[mimeType].Add(new TypeDescription(fileType.ToLower(), description));
            }
        }

        private struct TypeDescription
        {
            public TypeDescription(string fileType, string desc)
            {
                FileType = fileType;
                Description = desc;
            }

            public readonly string FileType;
            public readonly string Description;
        }

        private static Dictionary<string, List<TypeDescription>> InnerMimeMap;

        /// <summary>
        /// Returns the MIME Type associated with the DOS/Windows file type
        /// of the specified file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetContentTypeFromFileType(string filename)
        {
            string ext = filename.ToLower();
            if (ext.Contains("."))
            {
                ext = ext.Substring(ext.LastIndexOf("."));
            }
            else ext = "." + ext;
            foreach (var kvp in InnerMimeMap)
            {
                for (int i=0; i<kvp.Value.Count; i++)
                {
                    if (kvp.Value[i].FileType == ext) return kvp.Key;
                }
            }
            throw new ArgumentException("No matching content type found.");
        }

        /// <summary>
        /// Returns the first known DOS/Windows file type associated with 
        /// the specified MIME Type.
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public static string GetFileTypeFromContentType(string mimeType)
        {
            return InnerMimeMap[mimeType][0].FileType;
        }

        /// <summary>
        /// Returns a human-readable description of the specified MIME Type
        /// based on the known MIME Type map.
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public static string GetContentTypeDescriptionFromContentType(string mimeType)
        {
            return InnerMimeMap[mimeType][0].Description;
        }

        /// <summary>
        /// Returns a human-readable description of the specified DOS/Windows file type
        /// based on the known MIME Type map. 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetContentTypeDescriptionFromFileType(string filename)
        {
            string ext = filename.ToLower();
            if (ext.Contains("."))
            {
                ext = ext.Substring(ext.LastIndexOf("."));
            }
            else ext = "." + ext;
            foreach (var kvp in InnerMimeMap)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (kvp.Value[i].FileType == ext)
                    {
                        var ret = kvp.Value[i].Description;
                        if (string.IsNullOrEmpty(ret)) ret = kvp.Key;
                        return ret;
                    }
                }
            }
            throw new ArgumentException("No matching content type found.");
        }
    }
}
