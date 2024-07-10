using System;
using System.Collections.Generic;

namespace UnitySimplified
{
    public static class TypeExtensions
    {
        private static readonly HashSet<Type> NumericTypes = new()
        {
            typeof(byte),
            typeof(sbyte),
            typeof(int),
            typeof(uint),
            typeof(short),
            typeof(ushort),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
        };

        public static bool IsNumericType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException($"Parameter \"{nameof(type)}\" can not be NULL.");
            return NumericTypes.Contains(type) || NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }
    }
}