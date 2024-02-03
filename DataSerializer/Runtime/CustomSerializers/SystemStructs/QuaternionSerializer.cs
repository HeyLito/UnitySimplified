using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(Quaternion), false)]
	public sealed class QuaternionSerializer : IDataSerializable
	{
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                var obj = (Quaternion)instance;
                fieldData[nameof(Quaternion.x)] = obj.x;
                fieldData[nameof(Quaternion.y)] = obj.y;
                fieldData[nameof(Quaternion.z)] = obj.z;
                fieldData[nameof(Quaternion.w)] = obj.w;
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                instance = new Quaternion
                {
                    x = ((IConvertible)fieldData[nameof(Quaternion.x)]).ToSingle(null),
                    y = ((IConvertible)fieldData[nameof(Quaternion.y)]).ToSingle(null),
                    z = ((IConvertible)fieldData[nameof(Quaternion.z)]).ToSingle(null),
                    w = ((IConvertible)fieldData[nameof(Quaternion.w)]).ToSingle(null)
                };
            }
        }
    }
}