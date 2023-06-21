using System.Collections.Generic;

namespace UnitySimplified.Serialization 
{
    [CustomSerializer(typeof(object), true, -10)]
    public class CustomSerializer_SystemObject : IDataSerializable
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags) => DataSerializerUtility.Serialize(instance, fieldData, flags);
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags) => DataSerializerUtility.Deserialize(instance, fieldData, flags);
    }
}