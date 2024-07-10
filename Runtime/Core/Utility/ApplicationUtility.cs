using System;
using System.Reflection;
using System.Collections.Generic;

namespace UnitySimplified
{
    public static class ApplicationUtility
    {
        private static List<Assembly> _cachedAssemblies;
        private static Dictionary<Assembly, IEnumerable<Type>> _cachedTypesByAssemblies;

        public static IEnumerable<Assembly> GetAssemblies()
        {
            _cachedAssemblies ??= new List<Assembly>(SearchAssemblies());
            _cachedTypesByAssemblies = new Dictionary<Assembly, IEnumerable<Type>>();
            return _cachedAssemblies;

        }
        public static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            if (!_cachedTypesByAssemblies.TryGetValue(assembly, out IEnumerable<Type> types))
                _cachedTypesByAssemblies[assembly] = types = SearchTypesFromAssembly(assembly);
            return types;
        }

        private static IEnumerable<Type> SearchTypesFromAssembly(Assembly assembly) => assembly.GetTypes();
        private static IEnumerable<Assembly> SearchAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
    }
}