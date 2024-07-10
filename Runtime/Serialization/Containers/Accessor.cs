using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
#if ENABLE_UNITYSIMPLIFIED_NEWTONSOFT
    [Newtonsoft.Json.JsonConverter(typeof(AccessorJsonConverter))]
#endif
    [Serializable]
    public abstract class Accessor
    {
        [NonSerialized]
        private static bool _initialized;
        [NonSerialized]
        private static readonly Dictionary<Type, Accessor> AccessorsByAccessorTypes = new();
        [NonSerialized]
        private static readonly Dictionary<string, Type> AccessorTypesByNames = new();
        [NonSerialized]
        private static readonly Dictionary<Type, Type> AccessorTypesByValueTypes = new();



        public virtual int SortPriority => -1;
        public abstract bool CanConvert(Type valueType);
        public abstract void Set(object value);
        public abstract void Get(out object value);

        public static Accessor Create<TValue>() => DoCreate(typeof(TValue));
        public static Accessor Create(Type valueType) => DoCreate(valueType);
        public static bool TryCreate<TValue>(out Accessor accessor) => DoTryCreate(typeof(TValue), out accessor);
        public static bool TryCreate(Type valueType, out Accessor accessor) => DoTryCreate(valueType, out accessor);
        public static bool NameToType(string name, out Type type) => DoNameToType(name, out type);
        public static bool TypeToName(Type type, out string name) => DoTypeToName(type, out name);

        private static bool DoNameToType(string name, out Type type)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Initialize();
            return AccessorTypesByNames.TryGetValue(name, out type);
        }
        private static bool DoTypeToName(Type type, out string name)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Initialize();
            name = string.Empty;
            if (AccessorTypesByValueTypes.ContainsValue(type))
                name = type.Name;
            return !string.IsNullOrEmpty(name);
        }


        private static Accessor DoCreate(Type valueType)
        {
            if (valueType == null)
                throw new ArgumentNullException(nameof(valueType));

            if (DoLocateAccessor(valueType, out Type accessorType))
                return (Accessor)Activator.CreateInstance(accessorType);
            return null;
        }
        private static bool DoTryCreate(Type valueType, out Accessor accessor)
        {
            if (valueType == null)
                throw new ArgumentNullException(nameof(valueType));

            accessor = null;
            if (DoLocateAccessor(valueType, out Type accessorType))
                accessor = (Accessor)Activator.CreateInstance(accessorType);
            return accessor != null;
        }
        private static bool DoLocateAccessor(Type objectType, out Type accessorType)
        {
            Initialize();

            accessorType = null;
            if (AccessorTypesByValueTypes.TryGetValue(objectType, out accessorType))
                return accessorType != null;

            var lookup = AccessorsByAccessorTypes.Values.ToLookup(x => x.CanConvert(objectType))[true];
            var lookupOrderedResult = lookup.OrderByDescending(x => x.SortPriority).FirstOrDefault();

            AccessorTypesByValueTypes[objectType] = accessorType = lookupOrderedResult?.GetType();
            return accessorType != null;
        }
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;

            Type accessorType = typeof(Accessor);
            Type accessorGenericType = typeof(Accessor<>);
            foreach (var assembly in ApplicationUtility.GetAssemblies())
                foreach (var type in ApplicationUtility.GetTypesFromAssembly(assembly))
                {
                    if (type == accessorType || type == accessorGenericType)
                        continue;
                    if (!accessorType.IsAssignableFrom(type))
                        continue;
                    AccessorsByAccessorTypes[type] = (Accessor)Activator.CreateInstance(type);
                    AccessorTypesByNames[type.Name] = type;
                }
        }
    }

    [Serializable]
    public abstract class Accessor<T> : Accessor
    {
        public abstract void Set(T value);
        public abstract void Get(out T value);

        public sealed override bool CanConvert(Type valueType) => valueType == typeof(T);
        public sealed override void Set(object value)
        {
            Set((T)value);
        }
        public sealed override void Get(out object value)
        {
            Get(out T genericValue);
            value = genericValue;
        }
    }
}