using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<Type, Accessor> Accessors = new();
        [NonSerialized]
        private static readonly Dictionary<Type, Type> AccessorTypesByValueTypes = new();
        [NonSerialized]
        private static readonly Dictionary<string, Type> AccessorTypesByShortNames = new();



        public abstract bool CanAccess(Type valueType);
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
            
            InitializeAccessorDataTypes();
            return AccessorTypesByShortNames.TryGetValue(name, out type);
        }
        private static bool DoTypeToName(Type type, out string name)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            name = string.Empty;
            if (Accessors.ContainsKey(type))
                name = type.Name;
            return !string.IsNullOrEmpty(name);
        }
        private static Accessor DoCreate(Type valueType)
        {
            if (valueType == null)
                throw new ArgumentNullException(nameof(valueType));

            InitializeAccessorDataTypes();
            if (DoTryGetAccessorType(valueType, out Type accessorType))
                return (Accessor)Activator.CreateInstance(accessorType);
            return null;
        }
        private static bool DoTryCreate(Type valueType, out Accessor accessor)
        {
            if (valueType == null)
                throw new ArgumentNullException(nameof(valueType));

            accessor = null;
            InitializeAccessorDataTypes();
            if (DoTryGetAccessorType(valueType, out Type accessorType))
                accessor = (Accessor)Activator.CreateInstance(accessorType);
            return accessor != null;
        }
        private static bool DoTryGetAccessorType(Type valueType, out Type accessorType)
        {
            InitializeAccessorDataTypes();
            if (AccessorTypesByValueTypes.TryGetValue(valueType, out accessorType))
                return accessorType != null;

            AccessorTypesByValueTypes[valueType] = null;
            foreach (var (type, accessorInstance) in Accessors)
            {
                if (!accessorInstance.CanAccess(valueType))
                    continue;
                AccessorTypesByValueTypes[valueType] = accessorType = type;
                break;
            }
            return accessorType != null;
        }
        [RuntimeInitializeOnLoadMethod]
        private static void InitializeAccessorDataTypes()
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
                    Accessors.Add(type, (Accessor)Activator.CreateInstance(type));
                    AccessorTypesByShortNames[type.Name] = type;
                }
        }
    }

    [Serializable]
    public abstract class Accessor<T> : Accessor
    {
        public abstract void Set(T value);
        public abstract void Get(out T value);

        public sealed override bool CanAccess(Type valueType) => valueType == typeof(T);
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