using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Vector3Int), false)]
	public sealed class Vector3IntSerializer : IDataSerializable
	{
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                var obj = (Vector3Int)instance;
                fieldData[nameof(Vector3Int.x)] = obj.x;
                fieldData[nameof(Vector3Int.y)] = obj.y;
                fieldData[nameof(Vector3Int.z)] = obj.z;
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                instance = new Vector3Int
                {
                    x = ((IConvertible)fieldData[nameof(Vector3Int.x)]).ToInt32(null),
                    y = ((IConvertible)fieldData[nameof(Vector3Int.y)]).ToInt32(null),
                    z = ((IConvertible)fieldData[nameof(Vector3Int.z)]).ToInt32(null)
                };
            }
        }
    }
}