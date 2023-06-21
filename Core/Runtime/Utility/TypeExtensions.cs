using System;
using System.Collections.Generic;

namespace UnitySimplified
{
    public static class TypeExtensions
    {
        private static readonly HashSet<Type> numericTypes = new HashSet<Type>
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
        {   return numericTypes.Contains(type) || numericTypes.Contains(Nullable.GetUnderlyingType(type));   }
    }
}