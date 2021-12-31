using System;
using System.Runtime.Serialization;

namespace UnitySimplified.Serialization 
{
    [Serializable]
    public sealed class FieldData : SerializableDictionary<string, object>
    {
        public FieldData(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public FieldData() { }
    }
}