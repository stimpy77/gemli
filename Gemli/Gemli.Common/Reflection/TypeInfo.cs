using System;
using System.Linq;
using System.Reflection;

namespace Gemli.Reflection
{
    /// <summary>
    /// Contains utility methods for comparing <see cref="System.Type"/> objects.
    /// </summary>
    public static class TypeInfo
    {
        /// <summary>
        /// Returns true if the two type objects are either the same or if
        /// the <paramref name="typeSubject"/> parameter inherits
        /// the <paramref name="consideredBaseType"/> parameter.
        /// </summary>
        /// <param name="typeSubject"></param>
        /// <param name="consideredBaseType"></param>
        /// <returns></returns>
        public static bool IsOrInherits(this Type typeSubject, Type consideredBaseType)
        {
            return consideredBaseType.IsAssignableFrom(typeSubject);
        }

        /// <summary>
        /// Returns true if the specified type is, for example, an int?, decimal?, or 
        /// other variation of <see cref="Nullable{T}"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullableWrappedValueType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericArguments().Length == 1)
            {
                var gp = type.GetGenericArguments()[0];
                if (typeof (Nullable<>).MakeGenericType(gp) == type)
                {
                    return true;
                }
            }
            return false;
        }

        public static MethodInfo GetUnmadeGenericMethod(this Type typeSubject, string methodName, int genericArgumentCount, Type[] parameters)
        {
            var mis = typeSubject.GetMethods().ToList();
            return mis.Find(mi =>
                                {
                                    if (mi.Name != methodName) return false;
                                    if (mi.GetGenericArguments().Length != genericArgumentCount) return false;
                                    var p = mi.GetParameters();
                                    if (p.Length != parameters.Length) return false;
                                    for (var pi=0; pi<p.Length; pi++)
                                    {
                                        if (p[pi].ParameterType.IsGenericType)
                                        {
                                            if (!parameters[pi].IsGenericType) return false;
                                            if (p[pi].ParameterType.Name != parameters[pi].Name ||
                                                p[pi].ParameterType.Namespace != parameters[pi].Namespace)
                                            {
                                                return false;
                                            }
                                            var pgargs = p[pi].ParameterType.GetGenericArguments();
                                            var prargs = parameters[pi].GetGenericArguments();
                                            if (pgargs.Length < prargs.Length) return false;
                                        } else
                                        {
                                            if (p[pi].ParameterType != parameters[pi]) return false;
                                        }
                                    }
                                    return true;
                                });
        }

        public static MethodInfo GetMadeGenericMethod(this Type typeSubject, string methodName, Type[] genericParameters, Type[] parameters)
        {
            var mi = GetUnmadeGenericMethod(typeSubject, methodName, genericParameters.Length, parameters);
            return mi.MakeGenericMethod(genericParameters);
        }
    }
}
