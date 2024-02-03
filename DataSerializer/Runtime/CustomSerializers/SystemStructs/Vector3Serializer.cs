using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Vector3), false)]
	public sealed class Vector3Serializer : IDataSerializable
	{
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                var obj = (Vector3)instance;
                fieldData[nameof(Vector3.x)] = obj.x;
                fieldData[nameof(Vector3.y)] = obj.y;
                fieldData[nameof(Vector3.z)] = obj.z;
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                instance = new Vector3
                {
                    x = ((IConvertible)fieldData[nameof(Vector3.x)]).ToSingle(null),
                    y = ((IConvertible)fieldData[nameof(Vector3.y)]).ToSingle(null),
                    z = ((IConvertible)fieldData[nameof(Vector3.z)]).ToSingle(null)
                };
            }
        }
    }
}