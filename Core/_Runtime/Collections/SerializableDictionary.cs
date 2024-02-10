using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace UnitySimplified.Collections
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : 
        Dictionary<TKey, TValue>,
        IList,
        IList<KeyValuePair<TKey, TValue>>,
        ISerializationCallbackReceiver
    {
        public SerializableDictionary() { }
        public SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        [SerializeField]
        private List<TKey> _keys = new();
        [SerializeField]
        private List<TValue> _values = new();

        bool IList.IsFixedSize => false;
        bool IList.IsReadOnly => false;
        object IList.this[int index]
        {
            get => ((IList<KeyValuePair<TKey, TValue>>)this)[index];
            set
            {
                switch (value)
                {
                    case KeyValuePair<TKey, TValue> pairValue:
                        ((IList<KeyValuePair<TKey, TValue>>)this)[index] = pairValue;
                        return;
                    case Tuple<TKey, TValue> tupleValue:
                        ((IList<KeyValuePair<TKey, TValue>>)this)[index] = new KeyValuePair<TKey, TValue>(tupleValue.Item1, tupleValue.Item2);
                        return;
                    default:
                        throw new InvalidCastException();
                }
            }
        }
        KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
        {
            get => new(_keys[index], _values[index]);
            set
            {
                _keys[index] = value.Key;
                _values[index] = value.Value;
            }
        }

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < _keys.Count && i < _values.Count; i++)
                if (_keys[i] != null && _values[i] != null)
                    Add(_keys[i], _values[i]);
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (var pair in collection)
                Add(pair.Key, pair.Value);
        }
        int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
        {
            var keyIndex = _keys.IndexOf(item.Key);
            if (keyIndex <= -1)
                return -1;
            if (_values[keyIndex].Equals(item.Value))
                return keyIndex;
            return -1;
        }
        void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            _keys.Insert(index, item.Key);
            _values.Insert(index, item.Value);
        }
        void IList<KeyValuePair<TKey, TValue>>.RemoveAt(int index)
        {
            _keys.RemoveAt(index);
            _values.RemoveAt(index);
        }
        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }
        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }
        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }
        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }
        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }


        //int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

        //int IList.Add(object value)
        //{
        //    throw new NotImplementedException();
        //}

        //bool IList.Contains(object value)
        //{
        //    throw new NotImplementedException();
        //}
    }
}