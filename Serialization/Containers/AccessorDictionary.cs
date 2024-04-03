using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnitySimplified.Collections;
// ReSharper disable JoinNullCheckWithUsage

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public class AccessorDictionary : IReadOnlyDictionary<string, object>, IDictionary<string, object>, IReadOnlyDictionary<string, Accessor>, IDictionary<string, Accessor>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private SerializableDictionary<string, Accessor> accessors = new();
        
        [NonSerialized]
        private static Dictionary<Type, Type> _accessorTypesByValueTypes;

        public int Count => accessors.Count;
        public IEnumerable<string> Keys => accessors.Keys;
        public IEnumerable<object> Values
        {
            get
            {
                foreach (var accessor in accessors.Values)
                {
                    if (accessor == null)
                    {
                        yield return null;
                        continue;
                    }

                    accessor.Get(out var result);
                    yield return result;
                }
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => ((ICollection<KeyValuePair<string, Accessor>>)accessors).IsReadOnly;
        bool ICollection<KeyValuePair<string, Accessor>>.IsReadOnly => ((ICollection<KeyValuePair<string, Accessor>>)accessors).IsReadOnly;
        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;
        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Values;
        ICollection<string> IDictionary<string, object>.Keys => Keys.ToList();
        ICollection<object> IDictionary<string, object>.Values => Values.ToList();
        ICollection<string> IDictionary<string, Accessor>.Keys => accessors.Keys;
        IEnumerable<Accessor> IReadOnlyDictionary<string, Accessor>.Values => accessors.Values;
        ICollection<Accessor> IDictionary<string, Accessor>.Values => accessors.Values;



        Accessor IDictionary<string, Accessor>.this[string key]
        {
            get
            {
                if (!DoTryGetValue(key, out Accessor value))
                    throw new KeyNotFoundException();
                return value;
            }
            set => DoSet(key, value);
        }
        Accessor IReadOnlyDictionary<string, Accessor>.this[string key]
        {
            get
            {
                if (!DoTryGetValue(key, out Accessor value))
                    throw new KeyNotFoundException();
                return value;
            }
        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                if (!DoTryGetValue(key, out object value))
                    throw new KeyNotFoundException();
                return value;
            }
            set => DoSet(key, value);
        }

        object IReadOnlyDictionary<string, object>.this[string key]
        {
            get
            {
                if (!DoTryGetValue(key, out object value))
                    throw new KeyNotFoundException();
                return value;
            }
        }


        public void Clear() => DoClear();
        public bool ContainsKey(string key) => DoContainsKey(key);
        public void Add<T>(string key, T value) => DoTryAdd(key, value);
        public bool TryAdd<T>(string key, T value) => DoTryAdd(key, value);
        public bool Remove(string key) => DoRemove(key);
        public bool TryGetValue<T>(string key, out T value) => DoTryGetValue(key, out value);

        void ISerializationCallbackReceiver.OnBeforeSerialize() => ((ISerializationCallbackReceiver)accessors).OnBeforeSerialize();
        void ISerializationCallbackReceiver.OnAfterDeserialize() => ((ISerializationCallbackReceiver)accessors).OnAfterDeserialize();
        

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        void IDictionary<string, object>.Add(string key, object value) => DoTryAdd(key, value);
        bool IDictionary<string, object>.TryGetValue(string key, out object value) => DoTryGetValue(key, out value);
        bool IReadOnlyDictionary<string, Accessor>.TryGetValue(string key, out Accessor value) => DoTryGetValue(key, out value);
        bool IReadOnlyDictionary<string, object>.TryGetValue(string key, out object value) => DoTryGetValue(key, out value);

        bool IDictionary<string, Accessor>.TryGetValue(string key, out Accessor value) => DoTryGetValue(key, out value);
        void IDictionary<string, Accessor>.Add(string key, Accessor value) => DoTryAdd(key, value);
        
        IEnumerator<KeyValuePair<string, Accessor>> IEnumerable<KeyValuePair<string, Accessor>>.GetEnumerator() => accessors.GetEnumerator();
        void ICollection<KeyValuePair<string, Accessor>>.CopyTo(KeyValuePair<string, Accessor>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, Accessor>>)accessors).CopyTo(array, arrayIndex);
        bool ICollection<KeyValuePair<string, Accessor>>.Contains(KeyValuePair<string, Accessor> item) => ((ICollection<KeyValuePair<string, Accessor>>)accessors).Contains(item);
        void ICollection<KeyValuePair<string, Accessor>>.Add(KeyValuePair<string, Accessor> item) => ((ICollection<KeyValuePair<string, Accessor>>)accessors).Add(item);
        bool ICollection<KeyValuePair<string, Accessor>>.Remove(KeyValuePair<string, Accessor> item) => ((ICollection<KeyValuePair<string, Accessor>>)accessors).Remove(item);

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => throw new NotImplementedException();
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException();
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) => DoContains(item);
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) => throw new NotImplementedException();
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) => throw new NotImplementedException();


        private void DoClear() => accessors.Clear();
        private bool DoContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            return accessors.TryGetValue(key, out _);
        }
        private bool DoContains(KeyValuePair<string, object> item)
        {
            if (item.Equals(default(KeyValuePair<string, object>)))
                throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(item.Key))
                throw new ArgumentNullException($"{nameof(item)}.{nameof(item.Key)}");

            if (DoTryGetValue(item.Key, out object value))
                return item.Value == value;
            return false;
        }

        private void DoSet(string key, Accessor accessor)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (accessor == null)
#pragma warning disable IDE0016
                throw new ArgumentNullException(nameof(accessor));
#pragma warning restore IDE0016

            accessors[key] = accessor;
        }
        private void DoSet(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value is Accessor)
                throw new InvalidOperationException();

            InitializeAccessorDataTypes();
            if (!_accessorTypesByValueTypes.TryGetValue(value.GetType(), out Type accessorValueType))
                return;

            var result = (Accessor)Activator.CreateInstance(accessorValueType);
            result.Set(value);
            accessors[key] = result;
        }

        private bool DoTryAdd(string key, Accessor accessor)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (accessor == null)
                throw new ArgumentNullException(nameof(accessor));

            if (accessors.ContainsKey(key))
                return false;
            accessors.Add(key, accessor);
            return true;
        }
        private bool DoTryAdd(string key, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value is Accessor)
                throw new InvalidOperationException();

            InitializeAccessorDataTypes();
            if (!_accessorTypesByValueTypes.TryGetValue(value.GetType(), out Type accessorValueType))
                return false;

            var accessor = (Accessor)Activator.CreateInstance(accessorValueType);
            if (!DoTryAdd(key, accessor))
                return false;
            accessor.Set(value);
            return true;
        }
        private bool DoRemove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return accessors.Remove(key);
        }
        private bool DoTryGetValue(string key, out object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            value = default;
            if (!accessors.TryGetValue(key, out Accessor accessor))
                return false;
            if (accessor == null)
                return false;

            accessor.Get(out var result);
            value = result;
            return true;
        }

        private bool DoTryGetValue(string key, out Accessor accessor)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return accessors.TryGetValue(key, out accessor);
        }
        private bool DoTryGetValue<T>(string key, out T value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            value = default;
            if (!DoTryGetValue(key, out object genericValue))
                return false;
            if (genericValue is not T valueCast)
                return false;

            value = valueCast;
            return true;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeAccessorDataTypes()
        {
            if (_accessorTypesByValueTypes != null)
                return;
            _accessorTypesByValueTypes = new Dictionary<Type, Type>();

            Type objectType = typeof(object);
            Type accessorDataType = typeof(Accessor);
            Type accessorGenericDataType = typeof(Accessor<>);
            foreach (var assembly in ApplicationUtility.GetAssemblies())
            foreach (var type in ApplicationUtility.GetTypesFromAssembly(assembly))
            {
                if (!accessorDataType.IsAssignableFrom(type) || accessorDataType == type || type == accessorGenericDataType)
                    continue;

                Type valueType = null;
                Type currentType = type;
                while (currentType is { BaseType: { } baseType } && baseType != objectType && valueType == null)
                {
                    baseType = currentType.BaseType;
                    if (baseType.IsGenericType)
                        valueType = baseType.GenericTypeArguments.FirstOrDefault();
                    currentType = baseType;
                }
                if (valueType != null)
                    _accessorTypesByValueTypes[valueType] = type;
            }
        }
    }
}