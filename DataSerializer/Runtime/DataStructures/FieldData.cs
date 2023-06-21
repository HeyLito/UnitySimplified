using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UnitySimplified.Serialization 
{
    [Serializable]
    public sealed class FieldData : SerializableDictionary<string, object>
    {
        public FieldData() { }
        public FieldData(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public FieldData(ICollection<KeyValuePair<string, object>> collection)
        {
            foreach (var pair in collection)
                Add(pair.Key, pair.Value);
        }
    }
}