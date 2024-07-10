using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class QuaternionDataConverter : IDataConverter<Quaternion>
    {
        public void Serialize(Quaternion value, IDictionary<string, object> output)
        {
            output[nameof(Quaternion.x)] = value.x;
            output[nameof(Quaternion.y)] = value.y;
            output[nameof(Quaternion.z)] = value.z;
            output[nameof(Quaternion.w)] = value.w;
        }
        public Quaternion Deserialize(Quaternion existingValue, IDictionary<string, object> input) => new()
        {
            x = input.TryGetValue(nameof(Quaternion.x), out object xObject) ? xObject.CastDynamically<float>() : 0,
            y = input.TryGetValue(nameof(Quaternion.y), out object yObject) ? yObject.CastDynamically<float>() : 0,
            z = input.TryGetValue(nameof(Quaternion.z), out object zObject) ? zObject.CastDynamically<float>() : 0,
            w = input.TryGetValue(nameof(Quaternion.w), out object wObject) ? wObject.CastDynamically<float>() : 0
        };
    }
}