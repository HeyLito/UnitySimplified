using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Vector2Int), false)]
    public sealed class Vector2IntSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                var obj = (Vector2Int)instance;
                fieldData[nameof(Vector2Int.x)] = obj.x;
                fieldData[nameof(Vector2Int.y)] = obj.y;
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                instance = new Vector2Int
                {
                    x = ((IConvertible)fieldData[nameof(Vector2Int.x)]).ToInt32(null),
                    y = ((IConvertible)fieldData[nameof(Vector2Int.y)]).ToInt32(null)
                };
            }
        }
    }
}