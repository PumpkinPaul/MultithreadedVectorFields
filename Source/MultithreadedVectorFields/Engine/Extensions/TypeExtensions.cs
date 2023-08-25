using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PumpkinGames.Glitchangels.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<FieldInfo> GetAllFields(this Type t)
        {
            if (t == null)
            {
                return Enumerable.Empty<FieldInfo>();
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.Static | BindingFlags.Instance |
                                       BindingFlags.DeclaredOnly;

            return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(this Type t)
        {
            if (t == null)
            {
                return Enumerable.Empty<PropertyInfo>();
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.Static | BindingFlags.Instance |
                                       BindingFlags.DeclaredOnly;

            return t.GetProperties(flags).Concat(GetAllProperties(t.BaseType));
        }

        public static bool Implements<T>(this Type source) where T : class
        {
            return typeof(T).IsAssignableFrom(source) && source.IsClass;
        }
    }
}