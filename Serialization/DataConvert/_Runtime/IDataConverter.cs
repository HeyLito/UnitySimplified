using System;
using System.Collections.Generic;

namespace UnitySimplified.Serialization
{
    public interface IDataConverter
    {
        int GetConversionPriority() => 0;
        bool CanConvert(Type objectType);

        void Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags);
        void Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags);
    }
}