using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Vector4), false)]
	public sealed class Vector4Serializer : IDataSerializable
	{
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                var obj = (Vector4)instance;
                fieldData[nameof(Vector4.x)] = obj.x;
                fieldData[nameof(Vector4.y)] = obj.y;
                fieldData[nameof(Vector4.z)] = obj.z;
                fieldData[nameof(Vector4.w)] = obj.w;
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                instance = new Vector4
                {
                    x = ((IConvertible)fieldData[nameof(Vector4.x)]).ToSingle(null),
                    y = ((IConvertible)fieldData[nameof(Vector4.y)]).ToSingle(null),
                    z = ((IConvertible)fieldData[nameof(Vector4.z)]).ToSingle(null),
                    w = ((IConvertible)fieldData[nameof(Vector4.w)]).ToSingle(null)
                };
            }
        }
    }
}