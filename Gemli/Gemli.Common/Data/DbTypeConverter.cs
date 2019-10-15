using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Gets the data type as converted from or to <see cref="DbType"/>, 
    /// <see cref="SqlDbType"/>, or <see cref="Type"/>.
    /// </summary>
    public class DbTypeConverter
    {
        /// <summary>
        /// Represents the pairing of a <see cref="DbType"/> and a <see cref="SqlDbType"/>
        /// and the pairing of these with the <see cref="System.Type"/> equivalent.
        /// </summary>
        public struct DbTypeMapping
        {
            /// <summary>
            /// Creates the mapping using the specified details.
            /// </summary>
            /// <param name="dbType"></param>
            /// <param name="sqlDbType"></param>
            /// <param name="type"></param>
            internal DbTypeMapping(DbType? dbType, SqlDbType? sqlDbType, Type type) : this()
            {
                if (dbType.HasValue) DbType = dbType.Value;
                else
                    Message = "There is no supported mapping equivalent to DbType for "
                              + ((object) sqlDbType ?? type).ToString() + ".";
                if (sqlDbType.HasValue) SqlDbType = sqlDbType.Value;
                else
                    Message = "There is no supported mapping equivalent to SqlDbType for "
                              + ((object)dbType ?? type).ToString() + ".";
                Type = type;
            }

            /// <summary>
            /// Creates the mapping using the specified details.
            /// </summary>
            /// <param name="dbType"></param>
            /// <param name="sqlDbType"></param>
            /// <param name="type"></param>
            /// <param name="maxSize">The maximum size of the specified type, i.e. 8000 in varchar(8000).</param>
            internal DbTypeMapping(DbType? dbType, SqlDbType? sqlDbType, Type type, long? maxSize)
                : this(dbType, sqlDbType, type)
            {
                MaxSize = maxSize;
            }

            /// <summary>
            /// Creates the mapping using the specified details.
            /// </summary>
            /// <param name="dbType"></param>
            /// <param name="sqlDbType"></param>
            /// <param name="type"></param>
            /// <param name="maxSize"></param>
            /// <param name="appendMessage"></param>
            internal DbTypeMapping(DbType? dbType, SqlDbType? sqlDbType, Type type, long? maxSize, string appendMessage)
                : this(dbType, sqlDbType, type, maxSize)
            {
                Message += " " + appendMessage;
            }

            /// <summary>
            /// The <see cref="System.Data.DbType"/> mapping element.
            /// </summary>
            public DbType? DbType;
            /// <summary>
            /// The <see cref="System.Data.SqlDbType"/> mapping element.
            /// </summary>
            public SqlDbType? SqlDbType;
            /// <summary>
            /// The <see cref="System.Type"/> mapping element.
            /// </summary>
            public Type Type;
            /// <summary>
            /// The max size of a specified data type, such as 8000 in varchar's max size of varchar(8000).
            /// </summary>
            public long? MaxSize;
            /// <summary>
            /// The mapping error message that describes, for example, the lack of equivalence to another form of data type.
            /// </summary>
            public string Message;
        }

        /// <summary>
        /// The mappings table used as data for convertions.
        /// </summary>
        public static DbTypeMapping[] Mappings;
        private static Dictionary<DbType, DbTypeMapping> DbTypeMappings;
        private static Dictionary<SqlDbType, DbTypeMapping> SqlDbTypeMappings;
        private static Dictionary<Type, DbTypeMapping> TypeMappings;

        private const int NTextSize = Int32.MaxValue/2;
        private const int StdMaxSize = 8000;

        static DbTypeConverter()
        {
            var mappings = new List<DbTypeMapping>
                               {
                                   new DbTypeMapping(DbType.AnsiString, SqlDbType.VarChar, typeof (string), StdMaxSize),
                                   new DbTypeMapping(DbType.AnsiStringFixedLength, SqlDbType.Char, typeof (string), StdMaxSize),
                                   new DbTypeMapping(null, SqlDbType.Text, typeof(string), Int32.MaxValue,
                                       "Consider using SqlDbType.VarChar."),
                                   new DbTypeMapping(null, SqlDbType.Binary, typeof(byte[]), StdMaxSize,
                                       "Consider using SqlDbType.VarBinary."),
                                   new DbTypeMapping(DbType.Binary, SqlDbType.VarBinary, typeof (byte[]), StdMaxSize),
                                   new DbTypeMapping(null,SqlDbType.Image, typeof(byte[]), Int32.MaxValue,
                                       "Consider using SqlDbType.VarBinary."),
                                   new DbTypeMapping(DbType.Boolean, SqlDbType.Bit, typeof (bool)),
                                   new DbTypeMapping(DbType.Byte, SqlDbType.TinyInt, typeof (byte)),
                                   new DbTypeMapping(DbType.Currency, SqlDbType.Money, typeof (decimal)),
                                   new DbTypeMapping(DbType.Date, SqlDbType.Date, typeof (DateTime)),
                                   new DbTypeMapping(DbType.DateTime, SqlDbType.DateTime, typeof (DateTime)),
                                   new DbTypeMapping(DbType.DateTime2, SqlDbType.DateTime2, typeof (DateTime)),
                                   new DbTypeMapping(DbType.DateTimeOffset, SqlDbType.DateTimeOffset, typeof (DateTimeOffset)),
                                   new DbTypeMapping(DbType.Decimal, SqlDbType.Decimal, typeof (decimal)),
                                   new DbTypeMapping(DbType.Double, SqlDbType.Float, typeof (double)),
                                   new DbTypeMapping(DbType.Guid, SqlDbType.UniqueIdentifier, typeof (Guid)),
                                   new DbTypeMapping(DbType.Int16, SqlDbType.SmallInt, typeof (Int16)),
                                   new DbTypeMapping(DbType.Int32, SqlDbType.Int, typeof (Int32)),
                                   new DbTypeMapping(DbType.Int64, SqlDbType.BigInt, typeof (Int64)),
                                   new DbTypeMapping(DbType.Object, SqlDbType.Variant, typeof (object)),
                                   new DbTypeMapping(DbType.SByte, null, typeof (sbyte)),
                                   new DbTypeMapping(DbType.Single, null, typeof (Single)),
                                   new DbTypeMapping(DbType.String, SqlDbType.NVarChar, typeof (string), StdMaxSize),
                                   new DbTypeMapping(DbType.StringFixedLength, SqlDbType.NChar, typeof (string), StdMaxSize),
                                   new DbTypeMapping(null, SqlDbType.NText, typeof(string), NTextSize,
                                       "Consider using SqlDbType.NVarChar."),
                                   new DbTypeMapping(DbType.Time, SqlDbType.Time, typeof (DateTime)),
                                   new DbTypeMapping(DbType.UInt16, null, typeof (UInt16), null, 
                                       "Consider using DbType.Int16 or DbType.Int32."),
                                   new DbTypeMapping(DbType.UInt32, null, typeof (UInt32), null,
                                       "Consider using DbType.Int32 or DbType.Int64."),
                                   new DbTypeMapping(DbType.UInt64, null, typeof (UInt64), null,
                                       "Consider using DbType.Int64."),
                                   new DbTypeMapping(DbType.VarNumeric, null, null)
                               };
            Mappings = mappings.ToArray();

            DbTypeMappings = new Dictionary<DbType, DbTypeMapping>();
            mappings.ForEach(m =>
            {
                if (m.DbType != null && !DbTypeMappings.ContainsKey(m.DbType.Value))
                    DbTypeMappings.Add(m.DbType.Value, m);
            });
            SqlDbTypeMappings = new Dictionary<SqlDbType, DbTypeMapping>();
            mappings.ForEach(m =>
            {
                if (m.SqlDbType != null && !SqlDbTypeMappings.ContainsKey(m.SqlDbType.Value))
                    SqlDbTypeMappings.Add(m.SqlDbType.Value, m);
            });
            TypeMappings = new Dictionary<Type, DbTypeMapping>();
            mappings.ForEach(m =>
            {
                if (m.Type != null && !TypeMappings.ContainsKey(m.Type))
                    TypeMappings.Add(m.Type, m);
            });

        }

        /// <summary>
        /// Returns the <see cref="DbType"/> that most likely matches the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DbType ToDbType(Type type)
        {
            if (type == typeof(string)) return DbType.String;
            if (type == typeof(byte[])) return DbType.Binary;
            if (type == typeof(DateTime)) return DbType.DateTime;
            if (!TypeMappings.ContainsKey(type))
                throw new ArgumentException("No mapping exists for type " 
                    + type.Namespace + "." + type.Name, "type");
            var ret = TypeMappings[type];
            if (ret.DbType.HasValue) return ret.DbType.Value;
            throw new ArgumentException(ret.Message, "type");
        }

        /// <summary>
        /// Returns the <see cref="DbType"/> that most likely matches the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DbType ToDbType(SqlDbType type)
        {
            if (!SqlDbTypeMappings.ContainsKey(type))
                throw new ArgumentException("No mapping exists for this type.", "type");
            var ret = SqlDbTypeMappings[type];
            if (ret.DbType.HasValue) return ret.DbType.Value;
            throw new ArgumentException(ret.Message, "type");
        }

        /// <summary>
        /// Returns the <see cref="DbType"/> that most likely matches the specified <paramref name="clrType_or_sqlDbType"/>.
        /// Allows loose typing but the parameter must be a <see cref="SqlDbType"/>, a <see cref="DbType"/>, or a <see cref="Type"/>.
        /// </summary>
        /// <param name="clrType_or_sqlDbType"></param>
        /// <returns></returns>
        public static DbType ToDbType(object clrType_or_sqlDbType)
        {
            if (clrType_or_sqlDbType is DbType) return (DbType) clrType_or_sqlDbType;
            if (clrType_or_sqlDbType is SqlDbType)
                return ToDbType((SqlDbType)clrType_or_sqlDbType);
            if (clrType_or_sqlDbType is Type)
                return ToDbType((Type) clrType_or_sqlDbType);
            throw new ArgumentException("Unsupported type.", "clrType_or_sqlDbType");
        }

        /// <summary>
        /// Returns the <see cref="SqlDbType"/> that most likely matches the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(DbType type)
        {
            var ret = DbTypeMappings[type];
            if (ret.SqlDbType.HasValue) return ret.SqlDbType.Value;
            throw new ArgumentException(ret.Message, "type");
        }

        /// <summary>
        /// Returns the <see cref="SqlDbType"/> that most likely matches the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(Type type)
        {
            if (type == typeof(string)) return SqlDbType.NVarChar;
            if (type == typeof(byte[])) return SqlDbType.VarBinary;
            if (type == typeof(DateTime)) return SqlDbType.DateTime;
            if (!TypeMappings.ContainsKey(type))
                throw new ArgumentException("No mapping exists for type " + type.Namespace + "." + type.Name, "type");
            return TypeMappings[type].SqlDbType.Value;
        }

        /// <summary>
        /// Returns the <see cref="SqlDbType"/> that most likely matches the specified <paramref name="clrType_or_dbType"/>.
        /// Allows loose typing but the parameter must be a <see cref="SqlDbType"/>, a <see cref="DbType"/>, or a <see cref="Type"/>.
        /// </summary>
        /// <param name="clrType_or_dbType"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(object clrType_or_dbType)
        {
            if (clrType_or_dbType is SqlDbType) return (SqlDbType) clrType_or_dbType;
            if (clrType_or_dbType is DbType)
                return ToSqlDbType((DbType)clrType_or_dbType);
            if (clrType_or_dbType is Type)
                return ToSqlDbType((Type)clrType_or_dbType);
            throw new ArgumentException("Unsupported type.", "clrType_or_dbType");
        }

        /// <summary>
        /// Returns the <see cref="Type"/> that most likely matches the specified DB <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type ToClrType(DbType type)
        {
            var ret = DbTypeMappings[type];
            if (ret.Type != null) return ret.Type;
            throw new ArgumentException(ret.Message, "type");
        }

        /// <summary>
        /// Returns the <see cref="Type"/> that most likely matches the specified DB <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type ToClrType(SqlDbType type)
        {
            var ret = SqlDbTypeMappings[type];
            if (ret.Type != null) return ret.Type;
            throw new ArgumentException(ret.Message, "type");
        }

        /// <summary>
        /// Returns the <see cref="Type"/> that most likely matches the specified <paramref name="dbType_or_sqlDbType"/>.
        /// </summary>
        /// <param name="dbType_or_sqlDbType"></param>
        /// <returns></returns>
        public static Type ToClrType(object dbType_or_sqlDbType)
        {
            if (dbType_or_sqlDbType is Type) return (Type) dbType_or_sqlDbType;
            if (dbType_or_sqlDbType is DbType)
                return ToClrType((DbType)dbType_or_sqlDbType);
            if (dbType_or_sqlDbType is SqlDbType)
                return ToClrType((SqlDbType)dbType_or_sqlDbType);
            throw new ArgumentException("Unsupported type.", "dbType_or_sqlDbType");
        }
    }
}
