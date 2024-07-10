using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class Vector4DataConverter : IDataConverter<Vector4>
    {
        public void Serialize(Vector4 value, IDictionary<string, object> output)
        {
            output[nameof(Vector4.x)] = value.x;
            output[nameof(Vector4.y)] = value.y;
            output[nameof(Vector4.z)] = value.z;
            output[nameof(Vector4.w)] = value.z;
        }
        public Vector4 Deserialize(Vector4 existingValue, IDictionary<string, object> input) => new()
        {
            x = input.TryGetValue(nameof(Vector4.x), out object xObject) ? xObject.CastDynamically<float>() : 0,
            y = input.TryGetValue(nameof(Vector4.y), out object yObject) ? yObject.CastDynamically<float>() : 0,
            z = input.TryGetValue(nameof(Vector4.z), out object zObject) ? zObject.CastDynamically<float>() : 0,
            w = input.TryGetValue(nameof(Vector4.w), out object wObject) ? wObject.CastDynamically<float>() : 0
        };
    }
}