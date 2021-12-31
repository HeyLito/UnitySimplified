using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Rigidbody), -1)]
    public class RigidbodySerializer : IConvertibleData
    {
        public void Serialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Rigidbody;

            fieldData[nameof(Rigidbody.velocity)] = obj.velocity;
        }
        public void Deserialize(object instance, Dictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Rigidbody;

            if (fieldData.TryGetValue(nameof(Rigidbody.velocity), out object velocity))
                obj.velocity = (Vector3)velocity;
        }
    }
}