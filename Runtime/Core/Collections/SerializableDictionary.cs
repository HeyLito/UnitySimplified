using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace UnitySimplified.Collections
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : 
        ISerializationCallbackReceiver,
        ISerializable,
        IDeserializationCallback,
        IDictionary,
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IList<KeyValuePair<TKey, TValue>>,
        IList
    {
        [SerializeField]
        private List<TKey> keys = new();
        [SerializeField]
        private List<TValue> values = new();

        [NonSerialized]
        private readonly Dictionary<TKey, TValue> _dictionary = new();
        

        public int Count => _dictionary.Count;
        public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;
        public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

        ICollection IDictionary.Keys => Keys;
        ICollection IDictionary.Values => Values;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
        object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).IsReadOnly;
        bool ICollection.IsSynchronized => ((ICollection)_dictionary).IsSynchronized;
        bool IDictionary.IsReadOnly => ((IDictionary)_dictionary).IsReadOnly;
        bool IDictionary.IsFixedSize => ((IDictionary)_dictionary).IsFixedSize;
        bool IList.IsReadOnly => ((IDictionary)_dictionary).IsReadOnly;
        bool IList.IsFixedSize => ((IDictionary)_dictionary).IsFixedSize;
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }
        object IDictionary.this[object key]
        {
            get => ((IDictionary)_dictionary)[key];
            set => ((IDictionary)_dictionary)[key] = value;
        }
        object IList.this[int index]
        {
            get => new KeyValuePair<TKey, TValue>(keys[index], values[index]);
            set
            {
                if (value is not KeyValuePair<TKey, TValue> pairValue)
                    return;
                keys[index] = pairValue.Key;
                values[index] = pairValue.Value;
            }
        }
        KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
        {
            get => new (keys[index], values[index]);
            set
            {
                keys[index] = value.Key;
                values[index] = value.Value;
            }
        }



        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _dictionary.Clear();
            for (int i = 0; i < keys.Count && i < values.Count; i++)
            {
                if (keys[i] != null && values[i] != null)
                {
                    _dictionary.Add(keys[i], values[i]);
                }
            }
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context) => _dictionary.GetObjectData(info, context);
        public void OnDeserialization(object sender) => _dictionary.OnDeserialization(sender);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
        public void Clear() => _dictionary.Clear();
        public void Add(TKey key, TValue value) => _dictionary.Add(key, value);
        public bool Remove(TKey key) =>_dictionary.Remove(key);
        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);
        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (var pair in collection)
                _dictionary.Add(pair.Key, pair.Value);
        }
        public List<KeyValuePair<TKey, TValue>> ToList()
        {
            var pairs = new List<KeyValuePair<TKey, TValue>>();
            foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
                pairs.Add(new KeyValuePair<TKey, TValue>(pair.Key, pair.Value));
            return pairs;
        }
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            var pairs = new KeyValuePair<TKey, TValue>[_dictionary.Count];
            int index = 0;
            foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
            {
                pairs[index] = new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
                index++;
            }
            return pairs;
        }

        void ICollection.CopyTo(Array array, int index) => ((ICollection)_dictionary).CopyTo(array, index);
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => _dictionary.Add(item.Key, item.Value);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionary).GetEnumerator();
        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)_dictionary).GetEnumerator();
        bool IDictionary.Contains(object key) => ((IDictionary)_dictionary).Contains(key);
        void IDictionary.Add(object key, object value) => ((IDictionary)_dictionary).Add(key, value);
        void IDictionary.Remove(object key) => ((IDictionary)_dictionary).Remove(key);

        int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
        {
            int keyIndex = keys.IndexOf(item.Key);
            if (keyIndex == -1)
                return -1;
            TValue value = values[keyIndex];
            if (value == null && item.Value == null)
                return keyIndex;
            if (values[keyIndex].Equals(item.Value))
                return keyIndex;
            return -1;
        }
        void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            keys.Insert(index, item.Key);
            values.Insert(index, item.Value);
        }
        void IList<KeyValuePair<TKey, TValue>>.RemoveAt(int index)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
        int IList.Add(object value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (value is not KeyValuePair<TKey, TValue> pairValue)
                throw new InvalidOperationException();
            ((IList<KeyValuePair<TKey, TValue>>)this).Add(pairValue);
            return Count - 1;
        }
        bool IList.Contains(object value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (value is not KeyValuePair<TKey, TValue> pairValue)
                throw new InvalidOperationException();
            return ((IList<KeyValuePair<TKey, TValue>>)this).Contains(pairValue);
        }
        int IList.IndexOf(object value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (value is not KeyValuePair<TKey, TValue> pairValue)
                throw new InvalidOperationException();
            return ((IList<KeyValuePair<TKey, TValue>>)this).IndexOf(pairValue);
        }
        void IList.Insert(int index, object value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (value is not KeyValuePair<TKey, TValue> pairValue)
                throw new InvalidOperationException();
            ((IList<KeyValuePair<TKey, TValue>>)this).Insert(index, pairValue);
        }
        void IList.Remove(object value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (value is not KeyValuePair<TKey, TValue> pairValue)
                throw new InvalidOperationException();
            ((IList<KeyValuePair<TKey, TValue>>)this).Remove(pairValue);
        }
        void IList.RemoveAt(int index) => ((IList<KeyValuePair<TKey, TValue>>)this).RemoveAt(index);
    }
}