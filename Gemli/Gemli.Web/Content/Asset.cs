using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
//using Gemli.Data;

namespace Gemli.Web.Content
{
    /// <summary>
    /// Describes an Internet or local resource that can be identified 
    /// and located by a URL.
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// Gets or sets the MIME Type of the resource.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the URI that locates the resource.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets the binary size of the resource.
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// Returns the entire resource as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] DownloadBinaryData()
        {
            var wc = new WebClient();
            return wc.DownloadData(Url);
        }

        /// <summary>
        /// Returns the entire resource as a string.
        /// </summary>
        /// <returns></returns>
        public string DownloadString()
        {
            var wc = new WebClient();
            return wc.DownloadString(Url);
        }

        /// <summary>
        /// Downloads the entire resource to a file
        /// at the specified <paramref name="target_path"/>.
        /// </summary>
        /// <param name="target_path"></param>
        public void DownloadFile(string target_path)
        {
            var wc = new WebClient();
            wc.DownloadFile(Url, target_path);
        }

        /// <summary>
        /// Downloads the entire resource to a file
        /// at the specified <paramref name="target_path"/>
        /// asynchronously.
        /// </summary>
        /// <param name="target_path"></param>
        public void DownloadFileAsync(string target_path)
        {
            throw new NotImplementedException();
        }
    }
}