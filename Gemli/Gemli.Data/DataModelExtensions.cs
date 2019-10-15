using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gemli.Reflection;

namespace Gemli.Data
{
    /// <summary>
    /// Utility class providing extension methods that assist
    /// in working with <see cref="DataModel"/> objects.
    /// </summary>
    internal static class DataModelExtensions
    {
        /// <summary>
        /// Returns true if the specified type is a <see cref="DataModel"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsDataModel(this Type type)
        {
            return type.IsOrInherits(typeof (DataModel));
        }

        /// <summary>
        /// Returns true if the specified object is a <see cref="DataModel"/> object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        internal static bool IsDataModel(this object o)
        {
            return o.GetType().IsOrInherits(typeof (DataModel));
        }

        internal static bool IsDataModelWrapper(this Type type)
        {
            return IsDataModelWrapper(type, true);
        }

        internal static bool IsDataModelWrapper(this Type type, bool checkBaseTypes)
        {
            if (!type.IsDataModel()) return false;
            if (!checkBaseTypes && (!type.IsGenericType || type.GetGenericArguments().Length != 1))
            {
                return false;
            }
            var t = type;
            while (t != typeof(object))
            {
                if (t == typeof(object)) return false;
                if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                {
                    var gtarg = t.GetGenericArguments()[0];
                    var wrapperType = typeof(DataModel<>).MakeGenericType(gtarg);
                    if (type.IsOrInherits(wrapperType)) return true;
                }
                if (!checkBaseTypes) return false;
                t = t.BaseType;
            }
            return false;
        }

        internal static Type GetDataModelWrapperGenericTypeArg(this Type type)
        {
            if (!type.IsDataModel()) return null;
            var t = type;
            while (t != typeof(object))
            {
                if (t == typeof(object)) return null;
                if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                {
                    var gtarg = t.GetGenericArguments()[0];
                    var wrapperType = typeof(DataModel<>).MakeGenericType(gtarg);
                    if (type.IsOrInherits(wrapperType)) return gtarg;
                }
                t = t.BaseType;
            }
            return null;
        }
    }
}
