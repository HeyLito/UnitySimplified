using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Transform), -1)]
    public sealed class TransformSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Transform;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(Transform.position)] = obj.position;
                fieldData[nameof(Transform.rotation)] = obj.rotation;
                fieldData[nameof(Transform.localScale)] = obj.localScale;
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as Transform;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(Transform.position), out object position))
                    obj.position = (Vector3)position;
                if (fieldData.TryGetValue(nameof(Transform.rotation), out object rotation))
                    obj.rotation = (Quaternion)rotation;
                if (fieldData.TryGetValue(nameof(Transform.localScale), out object scale))
                    obj.localScale = (Vector3)scale;
            }
        }
    }
}