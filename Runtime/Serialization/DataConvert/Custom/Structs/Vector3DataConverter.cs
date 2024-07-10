using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class Vector3DataConverter : IDataConverter<Vector3>
    {
        void IDataConverter<Vector3>.Serialize(Vector3 value, IDictionary<string, object> output)
        {
            output[nameof(Vector3.x)] = value.x;
            output[nameof(Vector3.y)] = value.y;
            output[nameof(Vector3.z)] = value.z;
        }
        Vector3 IDataConverter<Vector3>.Deserialize(Vector3 existingValue, IDictionary<string, object> input) => new()
        {
            x = input.TryGetValue(nameof(Vector3.x), out object xObject) ? xObject.CastDynamically<float>() : 0,
            y = input.TryGetValue(nameof(Vector3.y), out object yObject) ? yObject.CastDynamically<float>() : 0,
            z = input.TryGetValue(nameof(Vector3.z), out object zObject) ? zObject.CastDynamically<float>() : 0
        };
    }
}