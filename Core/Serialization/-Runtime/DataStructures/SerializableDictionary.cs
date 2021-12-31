using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public SerializableDictionary() { }

        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < keys.Count && i < values.Count; i++)
                if (values[i].GetType() != null)
                    Add(keys[i], values[i]);
        }

        public List<KeyValuePair<TKey, TValue>> ToList()
        {
            var pairs = new List<KeyValuePair<TKey, TValue>>();
            foreach (KeyValuePair<TKey, TValue> pair in this)
                pairs.Add(new KeyValuePair<TKey, TValue>(pair.Key, pair.Value));
            return pairs;
        }
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            var pairs = new KeyValuePair<TKey, TValue>[Count];
            int index = 0;
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                pairs[index] = new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
                index++;
            }
            return pairs;
        }
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (var pair in collection)
                Add(pair.Key, pair.Value);
        }
    }
}