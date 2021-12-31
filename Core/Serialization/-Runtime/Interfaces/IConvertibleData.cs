using System.Collections.Generic;

namespace UnitySimplified.Serialization 
{
    public interface IConvertibleData
    {
        void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags);
        void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags);
    }
}