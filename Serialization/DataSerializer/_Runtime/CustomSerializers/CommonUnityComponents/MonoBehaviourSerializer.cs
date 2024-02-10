using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(MonoBehaviour), true, -5)]
    public sealed class MonoBehaviourSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags) => DataSerializerUtility.Serialize(instance, fieldData, flags);
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags) => DataSerializerUtility.Deserialize(instance, fieldData, flags);
    }
}
