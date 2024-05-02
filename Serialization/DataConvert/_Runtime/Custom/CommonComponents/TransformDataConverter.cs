using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization.Formatters
{
    public sealed class TransformDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => typeof(Transform).IsAssignableFrom(objectType);
        void IDataConverter.Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = instance as Transform;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                output[nameof(Transform.position)] = obj.position;
                output[nameof(Transform.rotation)] = obj.rotation;
                output[nameof(Transform.localScale)] = obj.localScale;
            }
        }
        void IDataConverter.Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = instance as Transform;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (input.TryGetValue(nameof(Transform.position), out object position))
                    obj.position = (Vector3)position;
                if (input.TryGetValue(nameof(Transform.rotation), out object rotation))
                    obj.rotation = (Quaternion)rotation;
                if (input.TryGetValue(nameof(Transform.localScale), out object scale))
                    obj.localScale = (Vector3)scale;
            }
        }
    }
}