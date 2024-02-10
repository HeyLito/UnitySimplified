using System;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [Serializable]
    public class GamePrefData
    {
        [SerializeField]
        private string _identifier;
        [SerializeField]
        private string _key;
        [SerializeField]
        private object _value;
        [SerializeField]
        private string _valueType;


        public string Identifier => _identifier;
        public string Key { get => _key; set => _key = value; }
        public object Value { get => _value; set => _value = value; }
        public Type ValueType
        {
            get
            {
                if (string.IsNullOrEmpty(_identifier) || string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_valueType) || _value is null)
                    return null;
                return Type.GetType(_key);
            }
        }


        internal GamePrefData() { }
        internal GamePrefData(string identifier, string key, object value)
        {
            _identifier = identifier;
            _key = key;
            _value = value;
            _valueType = value.GetType().AssemblyQualifiedName;
        }
        internal GamePrefData(GamePrefData data) : this(data._identifier, data._key, data._value) { }
    }
}