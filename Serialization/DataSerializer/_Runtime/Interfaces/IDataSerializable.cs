using System.Collections.Generic;

namespace UnitySimplified.Serialization 
{
    public interface IDataSerializable
    {
        void Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags);
        void Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags);
    }
}