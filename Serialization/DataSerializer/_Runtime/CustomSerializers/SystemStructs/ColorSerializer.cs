using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
	[CustomSerializer(typeof(Color), false)]
	public sealed class ColorSerializer : IDataSerializable
	{
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                var obj = (Color)instance;
                fieldData[nameof(Color.r)] = obj.r;
                fieldData[nameof(Color.g)] = obj.g;
                fieldData[nameof(Color.b)] = obj.b;
                fieldData[nameof(Color.a)] = obj.a;
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                instance = new Color
                {
                    r = ((IConvertible)fieldData[nameof(Color.r)]).ToSingle(null),
                    g = ((IConvertible)fieldData[nameof(Color.g)]).ToSingle(null),
                    b = ((IConvertible)fieldData[nameof(Color.b)]).ToSingle(null),
                    a = ((IConvertible)fieldData[nameof(Color.a)]).ToSingle(null)
                };
            }
        }
    }
}