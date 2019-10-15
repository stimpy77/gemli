using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Gemli.Data.Providers
{
    /// <summary>
    /// Defines global settings for Gemli.Data provider objects.
    /// </summary>
    public class ProviderDefaults
    {
        static ProviderDefaults()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted;
            DefaultSchema = "dbo";
        }

        /// <summary>
        /// Allows uninitialized data objects to default to a
        /// specified application-wide data provider
        /// </summary>
        public static DataProviderBase AppProvider { get; set; }

        /// <summary>
        /// Specifies the default transaction isolation level.
        /// </summary>
        public static IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        /// Gets or sets the default database schema name, i.e. "dbo".
        /// </summary>
        public static string DefaultSchema { get; set; }

        /// <summary>
        /// Gets or sets the default database name.
        /// There is no default value for this property, 
        /// the value must be set.
        /// </summary>
        public static string DefaultCatalog { get; set; }
    }
}
