using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Vector2), false)]
    public sealed class Vector2Serializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                var obj = (Vector2)instance;
                fieldData[nameof(Vector2.x)] = obj.x;
                fieldData[nameof(Vector2.y)] = obj.y;
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                instance = new Vector2
                {
                    x = ((IConvertible)fieldData[nameof(Vector2.x)]).ToSingle(null),
                    y = ((IConvertible)fieldData[nameof(Vector2.y)]).ToSingle(null)
                };
            }
        }
    }
}