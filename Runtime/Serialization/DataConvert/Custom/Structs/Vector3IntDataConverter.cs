using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class Vector3IntDataConverter : IDataConverter<Vector3Int>
    {
        public void Serialize(Vector3Int value, IDictionary<string, object> output)
        {
            output[nameof(Vector3Int.x)] = value.x;
            output[nameof(Vector3Int.y)] = value.y;
            output[nameof(Vector3Int.z)] = value.z;
        }
        public Vector3Int Deserialize(Vector3Int existingValue, IDictionary<string, object> input) => new()
        {
            x = input.TryGetValue(nameof(Vector3Int.x), out object xObject) ? xObject.CastDynamically<int>() : 0,
            y = input.TryGetValue(nameof(Vector3Int.y), out object yObject) ? yObject.CastDynamically<int>() : 0,
            z = input.TryGetValue(nameof(Vector3Int.z), out object zObject) ? zObject.CastDynamically<int>() : 0
        };
    }
}